using Sisfarma.Sincronizador.Consejo;
using Sisfarma.Sincronizador.Extensions;
using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Farmatic.Models;
using Sisfarma.Sincronizador.Fisiotes;
using Sisfarma.Sincronizador.Fisiotes.Models;
using Sisfarma.Sincronizador.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
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
                
        
        private System.Timers.Timer timerControlSinStockInicial;
        private System.Timers.Timer timerControlStockFechasSalida;
        private System.Timers.Timer timerControlStockInicial;
        private System.Timers.Timer timerControlStockFechasEntrada;
        
        //private System.Timers.Timer timerListasTiendas;
        //private System.Timers.Timer timerCategorias;
        //private System.Timers.Timer timerListasFechas;
        //private System.Timers.Timer timerListas;
    


        public SincronizadorApplication()
        {                        
        }

        private void InitializeTimer()
        {                                    
            timerControlSinStockInicial = new System.Timers.Timer(2500);//(10000)
            timerControlSinStockInicial.Elapsed += (sender, @event) =>
            {
                timerControlSinStockInicial.Stop();
                FarmaticService farmatic = new FarmaticService(_localServer, _localBase, _localUser, _localPass);
                FisiotesService fisiotes = new FisiotesService(_remoteServer, _remoteUsername, _remoteUsername);
                ConsejoService consejo = new ConsejoService();
                ProcessControlSinStockInicial(farmatic, consejo, fisiotes);
                timerControlSinStockInicial.Start();
            };

            timerControlStockFechasSalida = new System.Timers.Timer(2500);//(60000);
            timerControlStockFechasSalida.Elapsed += (sender, @event) =>
            {
                timerControlStockFechasSalida.Stop();
                FarmaticService farmatic = new FarmaticService(_localServer, _localBase, _localUser, _localPass);
                FisiotesService fisiotes = new FisiotesService(_remoteServer, _remoteUsername, _remoteUsername);
                ConsejoService consejo = new ConsejoService();
                ProcessControlStockFechasSalida(farmatic, consejo, fisiotes);
                timerControlStockFechasSalida.Start();
            };

            timerControlStockInicial = new System.Timers.Timer(2500);//(5000);
            timerControlStockInicial.Elapsed += (sender, @event) =>
            {
                timerControlStockInicial.Stop();
                FarmaticService farmatic = new FarmaticService(_localServer, _localBase, _localUser, _localPass);
                FisiotesService fisiotes = new FisiotesService(_remoteServer, _remoteUsername, _remoteUsername);
                ConsejoService consejo = new ConsejoService();
                ProcessControlStockInicial(farmatic, consejo, fisiotes);
                timerControlStockInicial.Start();
            };

            timerControlStockFechasEntrada = new System.Timers.Timer(2500);//(60000);
            timerControlStockFechasEntrada.Elapsed += (sender, @event) =>
            {
                timerControlStockFechasEntrada.Stop();
                FarmaticService farmatic = new FarmaticService(_localServer, _localBase, _localUser, _localPass);
                FisiotesService fisiotes = new FisiotesService(_remoteServer, _remoteUsername, _remoteUsername);
                ConsejoService consejo = new ConsejoService();
                ProcessControlStockFechasEntrada(farmatic, consejo, fisiotes);
                timerControlStockFechasEntrada.Start();
            };


            //timerListasTiendas = new System.Timers.Timer(4500);
            //timerListasTiendas.Elapsed += (sender, @event) =>
            //{
            //    timerListasTiendas.Stop();
            //    FarmaticService farmatic = new FarmaticService();
            //    FisiotesService fisiotes = new FisiotesService();
            //    ConsejoService consejo = new ConsejoService();
            //    ProcessListaTienda(farmatic, consejo, fisiotes);
            //    timerListasTiendas.Start();
            //};

            //timerCategorias = new System.Timers.Timer(64000);
            //timerCategorias.Elapsed += (sender, @event) =>
            //{
            //    timerCategorias.Stop();
            //    FarmaticService farmatic = new FarmaticService();
            //    FisiotesService fisiotes = new FisiotesService();
            //    ProcessCategorias(farmatic, fisiotes);
            //    timerCategorias.Start();
            //};

            //timerListasFechas = new System.Timers.Timer(62000);
            //timerListasFechas.Elapsed += (sender, @event) =>
            //{
            //    timerListasFechas.Stop();
            //    FarmaticService farmatic = new FarmaticService();
            //    FisiotesService fisiotes = new FisiotesService();
            //    ProcessListasFechas(farmatic, fisiotes);
            //    timerListasFechas.Start();
            //};

            //timerListas = new System.Timers.Timer(61000);
            //timerListas.Elapsed += (sender, @event) =>
            //{
            //    timerListas.Stop();
            //    FarmaticService farmatic = new FarmaticService();
            //    FisiotesService fisiotes = new FisiotesService();
            //    ProcessListas(farmatic, fisiotes);
            //    timerListas.Start();
            //};
            


        }

        private string GetProveedorFromLocalOrDefault(FarmaticService farmaticService, string proveedor, string byDefault = "")
        {
            var proveedorDb = farmaticService.Proveedores.GetById(proveedor);
            return proveedorDb?.FIS_NOMBRE ?? byDefault;
        }
        

        public void ProcessControlSinStockInicial(FarmaticService farmaticService, ConsejoService consejoService, FisiotesService fisiotesService)
        {
            const string FIELD_POR_DONDE_VOY_SIN_STOCK = FieldsConfiguracion.FIELD_POR_DONDE_VOY_SIN_STOCK;
            try
            {
                var valorConfiguracion = fisiotesService.Configuraciones.GetByCampo(FIELD_POR_DONDE_VOY_SIN_STOCK);

                var codArticulo = !string.IsNullOrEmpty(valorConfiguracion)
                    ? valorConfiguracion
                    : "0";                
                                    
                var articulos = farmaticService.Articulos.GetWithoutStockByIdGreaterOrEqual(codArticulo);
                foreach (var articulo in articulos)
                {
                    SyncUpArticuloWithIva(farmaticService, consejoService, fisiotesService, articulo, FIELD_POR_DONDE_VOY_SIN_STOCK, articulo.IdArticu);
                }

                fisiotesService.Configuraciones.Update(FIELD_POR_DONDE_VOY_SIN_STOCK, "0");
                fisiotesService.Medicamentos.ResetPorDondeVoySinStock();
            }
            catch (Exception e)
            {
                //Task.Delay(1500).Wait();
                Console.WriteLine(e.Message);
                throw;
            }
        }

        private void SyncUpArticuloWithIva(FarmaticService farmaticService, ConsejoService consejoService, FisiotesService fisiotesService, ArticuloWithIva articulo, string updatefield, string updateValue)
        {
            fisiotesService.Configuraciones.Update(updatefield, updateValue);

            var familila = farmaticService.Familias.GetById(articulo.XFam_IdFamilia);
            var fechaUltimaCompra = articulo.FechaUltimaEntrada;
            var fechaUltimaVenta = articulo.FechaUltimaSalida;
            var precio = articulo.Pvp;
            var pcoste = articulo.Puc;
            var pvpsIva = Math.Round(articulo.Pvp * 100 / (articulo.Iva + 100), 2);
            var stock = articulo.StockActual;
            var stockMinimo = articulo.StockMinimo;
            var stockMaximo = articulo.StockMaximo;
            var desc = articulo.Descripcion.Strip();

            var superFamilia = !string.IsNullOrEmpty(familila?.Descripcion)
                    ? GetSuperFamiliaFromLocalOrDefault(farmaticService, familila.Descripcion)
                    : string.Empty;

            var codigoBarra = GetCodidoBarrasFromLocalOrDefault(farmaticService, articulo.IdArticu);
            var proveedor = GetProveedorFromLocalOrDefault(farmaticService, articulo.ProveedorHabitual);
            var nombreLaboratorio = GetNombreLaboratorioFromLocalOrDefault(farmaticService, consejoService, articulo.Laboratorio, "<Sin Laboratorio>");

            var espepara = default(Consejo.Models.Esperara); // consejoService.Esperas.Get(articulo.IdArticu);
            var present = espepara?.PRESENTACION ?? string.Empty;

            var descripcionHtml = string.Empty;
            var textos = new List<string>(); //consejoService.Esperas.GetTextos(articulo.IdArticu);
            foreach (var texto in textos)
            {
                if (string.IsNullOrEmpty(descripcionHtml))
                    descripcionHtml = texto;
                descripcionHtml += $@" <br> {texto}";
            }
            descripcionHtml = descripcionHtml.Length < 30000
                ? descripcionHtml.Replace(Environment.NewLine, "<br>").Replace("\0", string.Empty).Strip()
                : string.Empty;

            var baja = articulo.Baja;
            var activo = !baja;

            var fechaCaducidad = articulo.FechaCaducidad;

            var medicamento = fisiotesService.Medicamentos.GetOneOrDefaultByCodNacional(articulo.IdArticu);
            if (medicamento == null)
                fisiotesService.Medicamentos.Insert(codigoBarra.Strip(), articulo.IdArticu.Strip(), desc.Strip(), superFamilia.Strip(),
                        familila?.Descripcion.Strip(), Convert.ToSingle(precio), desc.Strip(), articulo.Laboratorio.Strip(), nombreLaboratorio.Strip(),
                        proveedor.Strip(), Convert.ToSingle(pvpsIva), Convert.ToInt32(articulo.Iva), stock, Convert.ToSingle(pcoste), stockMinimo, stockMaximo, present.Strip(),
                        descripcionHtml.Strip(), activo, fechaCaducidad, fechaUltimaCompra, fechaUltimaVenta, baja);
            else
            {
                if (desc.Strip() != medicamento.nombre || precio != medicamento.precio ||
                    articulo.Laboratorio != medicamento.laboratorio || articulo.Iva != medicamento.iva ||
                    stock != medicamento.stock || present.Strip() != medicamento.presentacion ||
                    descripcionHtml != medicamento.descripcion)
                    fisiotesService.Medicamentos.Update(codigoBarra.Strip(), desc.Strip(), superFamilia.Strip(),
                        familila?.Descripcion.Strip(), Convert.ToSingle(precio), desc.Strip(), articulo.Laboratorio.Strip(), nombreLaboratorio.Strip(),
                        proveedor.Strip(), Convert.ToInt32(articulo.Iva), Convert.ToSingle(pvpsIva), stock, Convert.ToSingle(pcoste), stockMinimo, stockMaximo, present.Strip(),
                        descripcionHtml.Strip(), activo, fechaCaducidad, fechaUltimaCompra, fechaUltimaVenta, baja, articulo.IdArticu,
                        withSqlExtra: true);
                else
                    fisiotesService.Medicamentos.Update(codigoBarra.Strip(), desc.Strip(), superFamilia.Strip(),
                        familila?.Descripcion.Strip(), Convert.ToSingle(precio), desc.Strip(), articulo.Laboratorio.Strip(), nombreLaboratorio.Strip(),
                        proveedor.Strip(), Convert.ToInt32(articulo.Iva), Convert.ToSingle(pvpsIva), stock, Convert.ToSingle(pcoste), stockMinimo, stockMaximo, present.Strip(),
                        descripcionHtml.Strip(), activo, fechaCaducidad, fechaUltimaCompra, fechaUltimaVenta, baja, articulo.IdArticu);
            }
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

        public void ProcessControlStockFechasSalida(FarmaticService farmaticService, ConsejoService consejoService, FisiotesService fisiotesService)
        {
            const string FIELD_NAME = FieldsConfiguracion.FIELD_STOCK_SALIDA;
            try
            {
                DateTime? fechaActualizacionStock = null;
                var configuracion = fisiotesService.Configuraciones.GetByCampo(FIELD_NAME);                                                    
                fechaActualizacionStock = CalculateFechaActualizacion(configuracion);

                // Recuperamos artículos de la base local junto al iva
                var articulosWithIva = farmaticService.Articulos.GetByFechaUltimaSalidaGreaterOrEqual(fechaActualizacionStock);
                foreach (var articulo in articulosWithIva)
                {
                    var strFecha = articulo.FechaUltimaSalida?.ToString("yyyy-MM-dd");
                    SyncUpArticuloWithIva(farmaticService, consejoService, fisiotesService, articulo, FIELD_NAME, strFecha);
                }
            }
            catch (Exception e)
            {                
                //Task.Delay(1500).Wait();
                Console.WriteLine(e.Message);
                throw;
            }
        }

        private static DateTime CalculateFechaActualizacion(string fecha)
        {
            try
            {
                return string.IsNullOrEmpty(fecha) || string.IsNullOrWhiteSpace(fecha)
                    ? DateTime.Now.AddDays(-7)
                    : (DateTime.Now - DateTime.ParseExact(fecha, "yyyy-dd-MM", CultureInfo.InvariantCulture)).TotalDays > 7
                    ? DateTime.Now.AddDays(-7)
                    : DateTime.ParseExact(fecha, "yyyy-dd-MM", CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return DateTime.ParseExact(fecha, "yyyy-dd-MM", CultureInfo.InvariantCulture);
            }
        }

        public void ProcessControlStockInicial(FarmaticService farmaticService, ConsejoService consejoService, FisiotesService fisiotesService)
        {
            const string FIELD_STOCK_ENTRADA = FieldsConfiguracion.FIELD_STOCK_ENTRADA;
            const string FIELD_STOCK_SALIDA = FieldsConfiguracion.FIELD_STOCK_SALIDA;
            const string FIELD_POR_DONDE_VOY_CON_STOCK = FieldsConfiguracion.FIELD_POR_DONDE_VOY_CON_STOCK;
            const string FIELD_POR_DONDE_VOY_SIN_STOCK = FieldsConfiguracion.FIELD_POR_DONDE_VOY_SIN_STOCK;
            try
            {
                //fisiotesService.Medicamentos.CheckAndCreateFields();                
                var configuracion = fisiotesService.Configuraciones.GetByCampo(FIELD_POR_DONDE_VOY_CON_STOCK);                
                var codArticulo = !string.IsNullOrEmpty(configuracion)
                    ? configuracion 
                    : "0";

                var articulos = farmaticService.Articulos.GetWithStockByIdGreaterOrEqual(codArticulo);
                foreach (var articulo in articulos)
                {
                    SyncUpArticuloWithIva(farmaticService, consejoService, fisiotesService, articulo, FIELD_POR_DONDE_VOY_CON_STOCK, articulo.IdArticu);
                }
                fisiotesService.Configuraciones.Update(FIELD_POR_DONDE_VOY_CON_STOCK, "0");
                fisiotesService.Medicamentos.ResetPorDondeVoy();
            }
            catch (Exception e)
            {
                //Task.Delay(1500).Wait();
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public void ProcessControlStockFechasEntrada(FarmaticService farmaticService, ConsejoService consejoService, FisiotesService fisiotesService)
        {
            const string FIELD_NAME = FieldsConfiguracion.FIELD_STOCK_ENTRADA;
            try
            {
                // Estblecemos la fecha de actualización del stock
                DateTime? fechaActualizacionStock = null;
                var configuracion = fisiotesService.Configuraciones.GetByCampo(FIELD_NAME);                
                fechaActualizacionStock = CalculateFechaActualizacion(configuracion);

                // Recuperamos artículos de la base local junto al iva
                var articulosWithIva = farmaticService.Articulos.GetByFechaUltimaEntradaGreaterOrEqual(fechaActualizacionStock);
                foreach (var articulo in articulosWithIva)
                {
                    var strFecha = articulo.FechaUltimaEntrada?.ToString("yyyy-MM-dd");
                    SyncUpArticuloWithIva(farmaticService, consejoService, fisiotesService, articulo, FIELD_NAME, strFecha);
                }
            }
            catch (Exception e)
            {
                Task.Delay(1500).Wait();
                //Task.Delay(1500).Wait();
                Console.WriteLine(e.Message);
                throw;
            }
        }



        //public void ProcessListaTienda(FarmaticService farmatic, ConsejoService consejo, FisiotesService fisiotes)
        //{
        //    try
        //    {
        //        if (_marketCodeList > 0)
        //        {
        //            var lista = farmatic.ListasArticulos.Get(_marketCodeList);
        //            if (lista != null)
        //            {
        //                var listaRemote = fisiotes.Listas.Get(lista.IdLista);
        //                if (listaRemote == null)
        //                    fisiotes.Listas.Insert(lista.IdLista, lista.Descripcion.Strip());
        //                else
        //                    fisiotes.Listas.Update(lista.IdLista, lista.Descripcion.Strip());

        //                fisiotes.Listas.DeArticulos.Delete(lista.IdLista);
        //                var articulos = farmatic.ListasArticulos.GetArticulosByLista(lista.IdLista);
        //                foreach (var articulo in articulos)
        //                {
        //                    fisiotes.Listas.DeArticulos.Insert(articulo.XItem_IdLista, Convert.ToInt32(articulo.XItem_IdArticu));
        //                    var awi = farmatic.ListasArticulos.GetArticuloWithIva(_marketCodeList, articulo.XItem_IdArticu);

        //                    var precio = awi.Pvp;
        //                    var pcoste = awi.Puc;
        //                    var pvpSinIva = Math.Round(precio * 100 / (awi.Iva + 100), 2);
        //                    var stock = awi.StockActual;
        //                    var stockMinimo = awi.StockMaximo;
        //                    var stockMaximo = awi.StockMaximo;
        //                    var desc = awi.Descripcion.Strip();
        //                    var familia = farmatic.Familias.GetById(awi.XFam_IdFamilia);
        //                    var superFamilia = string.IsNullOrEmpty(familia?.Descripcion)
        //                            ? string.Empty
        //                            : GetSuperFamiliaFromLocalOrDefault(farmatic, familia.Descripcion);
        //                    var codigoBarras = GetCodidoBarrasFromLocalOrDefault(farmatic, awi.IdArticu);
        //                    var proveedor = GetProveedorFromLocalOrDefault(farmatic, awi.ProveedorHabitual);
        //                    var nombreLaboratorio = GetNombreLaboratorioFromLocalOrDefault(farmatic, consejo, awi.Laboratorio, "<Sin Laboratorio>");

        //                    var espepara = consejo.Esperas.Get(awi.IdArticu);
        //                    var present = espepara?.PRESENTACION ?? string.Empty;

        //                    var descripcionHtml = string.Empty;
        //                    var textos = consejo.Esperas.GetTextos(awi.IdArticu);
        //                    foreach (var texto in textos)
        //                    {
        //                        if (string.IsNullOrEmpty(descripcionHtml))
        //                            descripcionHtml = texto;
        //                        descripcionHtml += $@" <br> {texto}";
        //                    }
        //                    descripcionHtml = descripcionHtml.Length < 30000
        //                        ? descripcionHtml.Replace(Environment.NewLine, "<br>").Replace("\0", string.Empty).Strip()
        //                        : string.Empty;

        //                    var baja = awi.Baja;
        //                    var activo = !baja;
        //                    var medicamento = fisiotes.Medicamentos.GetByCodNacional(awi.IdArticu);
        //                    if (medicamento == null)
        //                        fisiotes.Medicamentos.Insert(codigoBarras.Strip(), awi.IdArticu.Strip(), desc.Strip(), superFamilia.Strip(),
        //                                familia?.Descripcion.Strip(), Convert.ToSingle(precio), desc.Strip(), awi.Laboratorio.Strip(), nombreLaboratorio.Strip(),
        //                                proveedor.Strip(), Convert.ToSingle(pvpSinIva), Convert.ToInt32(awi.Iva), stock, Convert.ToSingle(pcoste), stockMinimo, stockMaximo, present.Strip(),
        //                                descripcionHtml.Strip(), activo, baja);
        //                    else
        //                    {
        //                        if (desc.Strip() != medicamento.nombre || precio != medicamento.precio || familia?.Descripcion.Strip() != medicamento.familia ||
        //                            awi.Laboratorio != medicamento.laboratorio || awi.Iva != medicamento.iva || pcoste != medicamento.puc ||
        //                            stock != medicamento.stock || present.Strip() != medicamento.presentacion ||
        //                            descripcionHtml != medicamento.descripcion)
        //                            fisiotes.Medicamentos.Update(codigoBarras.Strip(), desc.Strip(), superFamilia.Strip(),
        //                                familia?.Descripcion.Strip(), Convert.ToSingle(precio), desc.Strip(), awi.Laboratorio.Strip(), nombreLaboratorio.Strip(),
        //                                proveedor.Strip(), Convert.ToInt32(awi.Iva), Convert.ToSingle(pvpSinIva), stock, Convert.ToSingle(pcoste), stockMinimo, stockMaximo, present.Strip(),
        //                                descripcionHtml.Strip(), activo, baja, awi.IdArticu);
        //                    }
        //                }
        //                farmatic.ListasArticulos.Update(_marketCodeList);
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Task.Delay(1500).Wait();
        //    }
        //}

        //public void ProcessCategorias(FarmaticService farmatic, FisiotesService fisiotes)
        //{
        //    try
        //    {
        //        var familias = farmatic.Familias.GetByDescripcion();
        //        foreach (var familia in familias)
        //        {
        //            var padre = GetPadreFromLocalOrDefault(farmatic, familia.IdFamilia, @"<SIN PADRE>");
        //            var categoria = fisiotes.Categorias.GetByCategoriaAndPadre(familia.Descripcion.Strip(), padre.Strip());
        //            if (categoria == null)
        //            {
        //                var categoriaOnlyPadre = fisiotes.Categorias.GetByPadre(padre.Strip());
        //                var padreId = categoriaOnlyPadre?.prestashopPadreId;
        //                fisiotes.Categorias.Insert(familia.Descripcion.Strip(), padre.Strip(), padreId);
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Task.Delay(1500).Wait();
        //    }
        //}

        //private string GetPadreFromLocalOrDefault(FarmaticService farmatic, short familia, string byDefault = "")
        //{
        //    return farmatic.Familias.GetSuperFamiliaDescripcionById(familia) ?? byDefault;
        //}

        //public void ProcessListasFechas(FarmaticService farmatic, FisiotesService fisiotes)
        //{
        //    try
        //    {
        //        var listas = farmatic.ListasArticulos.GetByFechaExceptList(_marketCodeList);
        //        foreach (var lista in listas)
        //        {
        //            var listaRemote = fisiotes.Listas.Get(lista.IdLista);
        //            if (listaRemote == null)
        //                fisiotes.Listas.Insert(lista.IdLista, lista.Descripcion.Strip());
        //            else
        //                fisiotes.Listas.Update(lista.IdLista, lista.Descripcion.Strip());

        //            fisiotes.Listas.DeArticulos.Delete(lista.IdLista);
        //            var articulos = farmatic.ListasArticulos.GetArticulosByLista(lista.IdLista);
        //            foreach (var articulo in articulos)
        //            {
        //                fisiotes.Listas.DeArticulos.Insert(articulo.XItem_IdLista, Convert.ToInt32(articulo.XItem_IdArticu));
        //            }
        //            if (articulos.Count != 0)
        //            {
        //                var take = 1000;
        //                for (int i = 0; i < articulos.Count; i += take)
        //                {
        //                    var items = articulos.Skip(i).Take(1000).Select(x => new Fisiotes.Models.ListaArticulo
        //                    {
        //                        cod_lista = x.XItem_IdLista,
        //                        cod_articulo = Convert.ToInt32(x.XItem_IdArticu)
        //                    }).ToList();
        //                    fisiotes.Listas.DeArticulos.Insert(items);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Task.Delay(1500).Wait();
        //    }
        //}

        //public void ProcessListas(FarmaticService farmatic, FisiotesService fisiotes)
        //{
        //    try
        //    {
        //        fisiotes.Listas.CheckAndCreatePorDondeVoyField();
        //        var codLista = fisiotes.Listas.GetCodPorDondeVoy();
        //        var listas = farmatic.ListasArticulos.GetByIdGreaterThan(codLista);
        //        foreach (var lista in listas)
        //        {
        //            var listaRemote = fisiotes.Listas.Get(lista.IdLista);
        //            fisiotes.Listas.ResetPorDondeVoy();
        //            if (listaRemote == null)
        //                fisiotes.Listas.InsertWithPorDondeVoy(lista.IdLista, lista.Descripcion.Strip());
        //            else
        //                fisiotes.Listas.UpdateWithPorDondeVoy(lista.IdLista, lista.Descripcion.Strip());

        //            var articulos = farmatic.ListasArticulos.GetArticulosByLista(lista.IdLista);
        //            if (articulos.Count != 0)
        //            {
        //                fisiotes.Listas.DeArticulos.Delete(lista.IdLista);
        //                var take = 1000;
        //                for (int i = 0; i < articulos.Count; i += take)
        //                {
        //                    var items = articulos.Skip(i).Take(1000).Select(x => new Fisiotes.Models.ListaArticulo
        //                    {
        //                        cod_lista = x.XItem_IdLista,
        //                        cod_articulo = Convert.ToInt32(x.XItem_IdArticu)
        //                    }).ToList();
        //                    fisiotes.Listas.DeArticulos.Insert(items);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Task.Delay(1500).Wait();
        //    }
        //}       
    }
}