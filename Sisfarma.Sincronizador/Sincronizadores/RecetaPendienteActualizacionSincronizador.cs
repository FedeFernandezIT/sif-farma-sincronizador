using Sisfarma.Sincronizador.Extensions;
using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Fisiotes;
using Sisfarma.Sincronizador.Sincronizadores.SuperTypes;
using System;
using System.Linq;
using static Sisfarma.Sincronizador.Fisiotes.Repositories.ConfiguracionesRepository;

namespace Sisfarma.Sincronizador.Sincronizadores
{
    public class RecetaPendienteActualizacionSincronizador : TaskSincronizador
    {
        private const string YEAR_FOUND = FieldsConfiguracion.FIELD_ANIO_INICIO;

        public RecetaPendienteActualizacionSincronizador(FarmaticService farmatic, FisiotesService fisiotes) 
            : base(farmatic, fisiotes)
        {
        }

        public override void Process() => ProcessUpdateRecetasPendientes();

        private void ProcessUpdateRecetasPendientes()
        {
            var anioInicio = _fisiotes.Configuraciones.GetByCampo(YEAR_FOUND)
               .ToIntegerOrDefault(@default: DateTime.Now.Year - 2);

            var puntos = _fisiotes.PuntosPendientes.GetOfRecetasPendientes(anioInicio)
                .Where(punto => string.IsNullOrEmpty(punto.recetaPendiente) || punto.recetaPendiente == "D");

            foreach (var punto in puntos)
            {
                _cancellationToken.ThrowIfCancellationRequested();

                var lineaVenta = _farmatic.Ventas.GetLineaVentaOrDefaultByKey(punto.idventa, punto.idnlinea);
                    
                if (lineaVenta != null && lineaVenta.RecetaPendiente != "D")
                    _fisiotes.PuntosPendientes.Update(punto.idventa, punto.idnlinea, lineaVenta.RecetaPendiente);
                else 
                    _fisiotes.PuntosPendientes.Update(punto.idventa, punto.idnlinea);
            }
        }
    }
}
