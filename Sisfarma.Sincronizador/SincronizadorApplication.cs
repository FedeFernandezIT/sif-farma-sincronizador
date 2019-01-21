using Sisfarma.Sincronizador.Consejo;
using Sisfarma.Sincronizador.Extensions;
using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Farmatic.Models;
using Sisfarma.Sincronizador.Fisiotes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Sisfarma.Sincronizador.Fisiotes.Repositories.ConfiguracionesRepository;

namespace Sisfarma.Sincronizador
{
    public class SincronizadorApplication : ApplicationContext
    {

        private string _remoteBase, _remoteServer, _remoteUsername, _remotePassword;
        private string _localBase, _localServer, _localUser, _localPass;
        private int _marketCodeList;





        private System.Timers.Timer timerListasTiendas;
        private System.Timers.Timer timerListasFechas;




        public SincronizadorApplication()
        {                        
        }

        private void InitializeTimer()
        {
            timerListasTiendas = new System.Timers.Timer(4500);
            timerListasTiendas.Elapsed += (sender, @event) =>
            {
                timerListasTiendas.Stop();
                FarmaticService farmatic = new FarmaticService();
                FisiotesService fisiotes = new FisiotesService();
                ConsejoService consejo = new ConsejoService();
                ProcessListaTienda(farmatic, consejo, fisiotes);
                timerListasTiendas.Start();
            };

            timerListasFechas = new System.Timers.Timer(62000);
            timerListasFechas.Elapsed += (sender, @event) =>
            {
                timerListasFechas.Stop();
                FarmaticService farmatic = new FarmaticService();
                FisiotesService fisiotes = new FisiotesService();
                ProcessListasFechas(farmatic, fisiotes);
                timerListasFechas.Start();
            };



        }

        private string GetProveedorFromLocalOrDefault(FarmaticService farmaticService, string proveedor, string byDefault = "")
        {
            var proveedorDb = farmaticService.Proveedores.GetById(proveedor);
            return proveedorDb?.FIS_NOMBRE ?? byDefault;
        }
                

        

        private string GetSuperFamiliaFromLocalOrDefault(FarmaticService farmaticService, string familia, string byDefault = "")
        {
            return farmaticService.Familias.GetSuperFamiliaDescripcionByFamilia(familia) ?? byDefault;
        }

        private string GetCodidoBarrasFromLocalOrDefault(FarmaticService farmaticService, string articulo)
        {
            var sinonimo = farmaticService.Sinonimos.GetByArticulo(articulo);
            return sinonimo?.Sinonimo ?? "847000".PadLeft(6, '0');
        }

        private string GetNombreLaboratorioFromLocalOrDefault(FarmaticService farmaticService, ConsejoService consejoService, string codigo, string byDefault = "")
        {
            var nombreLaboratorio = byDefault;
            if (!string.IsNullOrEmpty(codigo?.Trim()) && !string.IsNullOrWhiteSpace(codigo))
            {
                var laboratorioDb = default(Consejo.Models.Labor); //consejoService.Laboratorios.Get(codigo);
                if (laboratorioDb == null)
                {
                    var laboratorioLocal =
                        farmaticService.Laboratorios.GetById(codigo);
                    nombreLaboratorio = laboratorioLocal?.Nombre ?? byDefault;
                }
                else nombreLaboratorio = laboratorioDb.NOMBRE;
            }
            else nombreLaboratorio = byDefault;
            return nombreLaboratorio;
        }

        

        



        public void ProcessListaTienda(FarmaticService farmatic, ConsejoService consejo, FisiotesService fisiotes)
        {
            try
            {
                if (_marketCodeList > 0)
                {
                    var lista = farmatic.ListasArticulos.Get(_marketCodeList);
                    if (lista != null)
                    {
                        var listaRemote = fisiotes.Listas.Get(lista.IdLista);
                        if (listaRemote == null)
                            fisiotes.Listas.Insert(lista.IdLista, lista.Descripcion.Strip());
                        else
                            fisiotes.Listas.Update(lista.IdLista, lista.Descripcion.Strip());

                        fisiotes.Listas.DeArticulos.Delete(lista.IdLista);
                        var articulos = farmatic.ListasArticulos.GetArticulosByLista(lista.IdLista);
                        foreach (var articulo in articulos)
                        {
                            fisiotes.Listas.DeArticulos.Insert(articulo.XItem_IdLista, Convert.ToInt32(articulo.XItem_IdArticu));
                            var awi = farmatic.ListasArticulos.GetArticuloWithIva(_marketCodeList, articulo.XItem_IdArticu);

                            var precio = awi.Pvp;
                            var pcoste = awi.Puc;
                            var pvpSinIva = Math.Round(precio * 100 / (awi.Iva + 100), 2);
                            var stock = awi.StockActual;
                            var stockMinimo = awi.StockMaximo;
                            var stockMaximo = awi.StockMaximo;
                            var desc = awi.Descripcion.Strip();
                            var familia = farmatic.Familias.GetById(awi.XFam_IdFamilia);
                            var superFamilia = string.IsNullOrEmpty(familia?.Descripcion)
                                    ? string.Empty
                                    : GetSuperFamiliaFromLocalOrDefault(farmatic, familia.Descripcion);
                            var codigoBarras = GetCodidoBarrasFromLocalOrDefault(farmatic, awi.IdArticu);
                            var proveedor = GetProveedorFromLocalOrDefault(farmatic, awi.ProveedorHabitual);
                            var nombreLaboratorio = GetNombreLaboratorioFromLocalOrDefault(farmatic, consejo, awi.Laboratorio, "<Sin Laboratorio>");

                            var espepara = consejo.Esperas.Get(awi.IdArticu);
                            var present = espepara?.PRESENTACION ?? string.Empty;

                            var descripcionHtml = string.Empty;
                            var textos = consejo.Esperas.GetTextos(awi.IdArticu);
                            foreach (var texto in textos)
                            {
                                if (string.IsNullOrEmpty(descripcionHtml))
                                    descripcionHtml = texto;
                                descripcionHtml += $@" <br> {texto}";
                            }
                            descripcionHtml = descripcionHtml.Length < 30000
                                ? descripcionHtml.Replace(Environment.NewLine, "<br>").Replace("\0", string.Empty).Strip()
                                : string.Empty;

                            var baja = awi.Baja;
                            var activo = !baja;
                            var medicamento = fisiotes.Medicamentos.GetByCodNacional(awi.IdArticu);
                            if (medicamento == null)
                                fisiotes.Medicamentos.Insert(codigoBarras.Strip(), awi.IdArticu.Strip(), desc.Strip(), superFamilia.Strip(),
                                        familia?.Descripcion.Strip(), Convert.ToSingle(precio), desc.Strip(), awi.Laboratorio.Strip(), nombreLaboratorio.Strip(),
                                        proveedor.Strip(), Convert.ToSingle(pvpSinIva), Convert.ToInt32(awi.Iva), stock, Convert.ToSingle(pcoste), stockMinimo, stockMaximo, present.Strip(),
                                        descripcionHtml.Strip(), activo, baja);
                            else
                            {
                                if (desc.Strip() != medicamento.nombre || precio != medicamento.precio || familia?.Descripcion.Strip() != medicamento.familia ||
                                    awi.Laboratorio != medicamento.laboratorio || awi.Iva != medicamento.iva || pcoste != medicamento.puc ||
                                    stock != medicamento.stock || present.Strip() != medicamento.presentacion ||
                                    descripcionHtml != medicamento.descripcion)
                                    fisiotes.Medicamentos.Update(codigoBarras.Strip(), desc.Strip(), superFamilia.Strip(),
                                        familia?.Descripcion.Strip(), Convert.ToSingle(precio), desc.Strip(), awi.Laboratorio.Strip(), nombreLaboratorio.Strip(),
                                        proveedor.Strip(), Convert.ToInt32(awi.Iva), Convert.ToSingle(pvpSinIva), stock, Convert.ToSingle(pcoste), stockMinimo, stockMaximo, present.Strip(),
                                        descripcionHtml.Strip(), activo, baja, awi.IdArticu);
                            }
                        }
                        farmatic.ListasArticulos.Update(_marketCodeList);
                    }
                }
            }
            catch (Exception e)
            {
                Task.Delay(1500).Wait();
            }
        }


        public void ProcessListasFechas(FarmaticService farmatic, FisiotesService fisiotes)
        {
            try
            {
                var listas = farmatic.ListasArticulos.GetByFechaExceptList(_marketCodeList);
                foreach (var lista in listas)
                {
                    var listaRemote = fisiotes.Listas.Get(lista.IdLista);
                    if (listaRemote == null)
                        fisiotes.Listas.Insert(lista.IdLista, lista.Descripcion.Strip());
                    else
                        fisiotes.Listas.Update(lista.IdLista, lista.Descripcion.Strip());

                    fisiotes.Listas.DeArticulos.Delete(lista.IdLista);
                    var articulos = farmatic.ListasArticulos.GetArticulosByLista(lista.IdLista);
                    foreach (var articulo in articulos)
                    {
                        fisiotes.Listas.DeArticulos.Insert(articulo.XItem_IdLista, Convert.ToInt32(articulo.XItem_IdArticu));
                    }
                    if (articulos.Count != 0)
                    {
                        var take = 1000;
                        for (int i = 0; i < articulos.Count; i += take)
                        {
                            var items = articulos.Skip(i).Take(1000).Select(x => new Fisiotes.Models.ListaArticulo
                            {
                                cod_lista = x.XItem_IdLista,
                                cod_articulo = Convert.ToInt32(x.XItem_IdArticu)
                            }).ToList();
                            fisiotes.Listas.DeArticulos.Insert(items);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Task.Delay(1500).Wait();
            }
        }

    }
}