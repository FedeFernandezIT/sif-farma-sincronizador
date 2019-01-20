using System;
using System.Linq;
using Sisfarma.Sincronizador.Consejo;
using Sisfarma.Sincronizador.Extensions;
using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Farmatic.Models;
using Sisfarma.Sincronizador.Fisiotes;
using Sisfarma.Sincronizador.Sincronizadores.SuperTypes;

namespace Sisfarma.Sincronizador.Sincronizadores
{
    public class ProductoCriticoSincronizador : BaseSincronizador
    {
        private const string LABORATORIO_DEFAULT = "<Sin Laboratorio>";
        private const string FAMILIA_DEFAULT = "<Sin Clasificar>";
        private const int STOCK_CRITICO = 0;        

        private readonly ConsejoService _consejo;

        public ProductoCriticoSincronizador(FarmaticService farmatic, FisiotesService fisiotes, ConsejoService consejo) 
            : base(farmatic, fisiotes)
        {
            _consejo = consejo ?? throw new ArgumentNullException(nameof(consejo));
        }

        public override void Process() => ProcessProductosCrticos(_farmatic, _fisiotes, _consejo);

        public void ProcessProductosCrticos(FarmaticService farmatic, FisiotesService fisiotes, ConsejoService consejo)
        {            
            var falta = fisiotes.Faltas.LastOrDefault();
            var pedidos = (falta == null)
                ? farmatic.Pedidos.GetByFechaGreaterOrEqual(DateTime.Now.Date)
                : farmatic.Pedidos.GetByIdGreaterOrEqual(falta.idPedido);

            foreach (var pedido in pedidos)
            {                
                var detallePedido = farmatic.Pedidos.GetLineasByPedido(pedido.IdPedido)
                    .Where(linea => !string.IsNullOrEmpty(linea.XArt_IdArticu?.Trim()));

                foreach (var linea in detallePedido)
                {                    
                    var articulo = farmatic.Articulos.GetById(linea.XArt_IdArticu);
                    if (articulo != null && articulo.StockActual == STOCK_CRITICO)
                    {
                        if(!fisiotes.Faltas.ExistsLineaDePedido(linea.IdPedido, linea.IdLinea))
                            fisiotes.Faltas.Insert(GenerarFaltante(farmatic, pedido, linea, articulo, consejo));                        
                    }
                }
            }
        }

        private Fisiotes.Models.Falta GenerarFaltante(FarmaticService farmatic, Pedido pedido, LineaPedido linea, Articulo articulo, ConsejoService consejo)
        {

            var fechaPedido = pedido.Hora;
            var fechaActual = DateTime.Now;            

            var pcoste = articulo.Puc;
            var precioMed = articulo.Pvp;

            var familia = farmatic.Familias.GetById(articulo.XFam_IdFamilia)?.Descripcion;
            if (string.IsNullOrWhiteSpace(familia))
                familia = FAMILIA_DEFAULT;

            var superFamilia = !familia.Equals(FAMILIA_DEFAULT)
                ? farmatic.Familias.GetSuperFamiliaDescripcionByFamilia(familia) ?? FAMILIA_DEFAULT
                : familia;

            var proveedor = farmatic.Proveedores.GetById(articulo.ProveedorHabitual)?.FIS_NOMBRE
                ?? string.Empty;

            var codLaboratorio = articulo.Laboratorio ?? string.Empty;
            var nombreLaboratorio = GetNombreLaboratorioFromLocalOrDefault(farmatic, consejo, codLaboratorio, LABORATORIO_DEFAULT);

            return new Fisiotes.Models.Falta
            {
                idPedido = linea.IdPedido,
                idLinea = linea.IdLinea,
                cod_nacional = articulo.IdArticu.Strip(),
                descripcion = articulo.Descripcion,
                familia = familia.Strip(),
                superFamilia = superFamilia.Strip(),
                cantidadPedida = linea.Unidades,
                fechaFalta = fechaActual,
                cod_laboratorio = codLaboratorio.Strip(),
                laboratorio = nombreLaboratorio.Strip(),
                proveedor = proveedor.Strip(),
                fechaPedido = fechaPedido,
                pvp = Convert.ToSingle(precioMed),
                puc = Convert.ToSingle(pcoste)
            };            
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
    }
}
