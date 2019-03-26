using Sisfarma.Sincronizador.Consejo;
using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Fisiotes;
using Sisfarma.Sincronizador.Helpers;
using Sisfarma.Sincronizador.Sincronizadores.SuperTypes;
using static Sisfarma.Sincronizador.Fisiotes.Repositories.ConfiguracionesRepository;

namespace Sisfarma.Sincronizador.Sincronizadores
{
    public class ControlStockFechaSalidaSincronizador : ControlSincronizador
    {
        private const string FIELD_STOCK_SALIDA = FieldsConfiguracion.FIELD_STOCK_SALIDA;

        public ControlStockFechaSalidaSincronizador(FarmaticService farmatic, FisiotesService fisiotes, ConsejoService consejo)
            : base(farmatic, fisiotes, consejo)
        { }

        public override void Process() => ProcessControlStockFechasSalida();

        private void ProcessControlStockFechasSalida()
        {
            var configuracion = _fisiotes.Configuraciones.GetByCampo(FIELD_STOCK_SALIDA);

            var fechaActualizacionStock = Calculator.CalculateFechaActualizacion(configuracion);

            var articulosWithIva = _farmatic.Articulos.GetByFechaUltimaSalidaGreaterOrEqual(fechaActualizacionStock);

            foreach (var articulo in articulosWithIva)
            {
                _cancellationToken.ThrowIfCancellationRequested();
                var medicamentoGenerado = Generator.GenerarMedicamento(_farmatic, _consejo, articulo);
                base._fisiotes.Medicamentos.Insert(medicamentoGenerado);
            }
        }
    }
}