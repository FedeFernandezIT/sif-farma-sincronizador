using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Fisiotes;
using Sisfarma.Sincronizador.Sincronizadores.SuperTypes;
using System.Linq;

namespace Sisfarma.Sincronizador.Sincronizadores
{
    public class RecetaPendienteActualizacionSincronizador : BaseSincronizador
    {
        public RecetaPendienteActualizacionSincronizador(FarmaticService farmatic, FisiotesService fisiotes) 
            : base(farmatic, fisiotes)
        {
        }

        public override void Process() => ProcessUpdateRecetasPendientes(_farmatic, _fisiotes);

        private void ProcessUpdateRecetasPendientes(FarmaticService farmatic, FisiotesService fisiotes)
        {
            var puntos = fisiotes.PuntosPendientes.GetOfRecetasPendientes()
                .Where(punto => string.IsNullOrEmpty(punto.recetaPendiente));

            foreach (var punto in puntos)
            {
                var lineaVenta = farmatic.Ventas.GetLineaVentaOrDefaultByKey(punto.idventa, punto.idnlinea);
                    
                if (lineaVenta != null && lineaVenta.RecetaPendiente != "D")
                    fisiotes.PuntosPendientes.Update(punto.idventa, punto.idnlinea, lineaVenta.RecetaPendiente);
                else 
                    fisiotes.PuntosPendientes.Update(punto.idventa, punto.idnlinea);
            }
        }
    }
}
