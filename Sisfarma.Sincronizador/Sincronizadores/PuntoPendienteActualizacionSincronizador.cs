using System;
using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Fisiotes;

namespace Sisfarma.Sincronizador.Sincronizadores
{
    public class PuntoPendienteActualizacionSincronizador : BaseSincronizador
    {
        public PuntoPendienteActualizacionSincronizador(FarmaticService farmatic, FisiotesService fisiotes) 
            : base(farmatic, fisiotes)
        {
        }

        public override void Process() => ProcessUpdatePuntosPendientes(_farmatic, _fisiotes);

        private void ProcessUpdatePuntosPendientes(FarmaticService farmatic, FisiotesService fisiotes)
        {
            var puntos = fisiotes.PuntosPendientes.GetWithoutRedencion();
            foreach (var pto in puntos)
            {
                var venta = farmatic.Ventas.GetById(pto.idventa);
                if (venta != null)
                {
                    var lineas = farmatic.Ventas.GetLineasVentaByVenta(venta.IdVenta);
                    foreach (var linea in lineas)
                    {
                        var lineaRedencion =
                            farmatic.Ventas.GetLineaRedencionByKey(linea.IdVenta, linea.IdNLinea);

                        var redencion = lineaRedencion?.Redencion ?? 0;

                        var articulo = farmatic.Articulos.GetById(linea.Codigo);

                        var proveedor = (articulo != null)
                            ? farmatic.Proveedores.GetById(articulo.ProveedorHabitual)?.FIS_NOMBRE ?? string.Empty
                            : string.Empty;                        

                        fisiotes.PuntosPendientes.Update(venta.TipoVenta, proveedor,
                            Convert.ToSingle(linea.DescuentoLinea), Convert.ToSingle(venta.DescuentoOpera),
                            Convert.ToSingle(redencion), linea.IdVenta, linea.IdNLinea);
                    }
                }
                else
                    fisiotes.PuntosPendientes.Update(pto.idventa);
            }
        }
    }
}
