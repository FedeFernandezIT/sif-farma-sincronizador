using Sisfarma.Sincronizador.Consejo;
using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Fisiotes;
using Sisfarma.Sincronizador.Fisiotes.Models;
using Sisfarma.Sincronizador.Helpers;
using Sisfarma.Sincronizador.Sincronizadores.SuperTypes;

namespace Sisfarma.Sincronizador.Sincronizadores
{
    public class ControlStockFechaEntradaSincronizador : ControlSincronizador
    {
        public ControlStockFechaEntradaSincronizador(FarmaticService farmatic, FisiotesService fisiotes, ConsejoService consejo)
            : base(farmatic, fisiotes, consejo)
        { }

        public override void Process() => ProcessControlStockFechasEntrada();

        private void ProcessControlStockFechasEntrada()
        {
            var configuracion = _fisiotes.Configuraciones.GetByCampo(Configuracion.FIELD_STOCK_ENTRADA);

            var fechaActualizacionStock = Calculator.CalculateFechaActualizacion(configuracion);

            var articulosWithIva = _farmatic.Articulos.GetByFechaUltimaEntradaGreaterOrEqual(fechaActualizacionStock);

            foreach (var articulo in articulosWithIva)
            {
                _cancellationToken.ThrowIfCancellationRequested();
                var medicamentoGenerado = Generator.GenerarMedicamento(_farmatic, _consejo, articulo);
                _fisiotes.Medicamentos.Insert(medicamentoGenerado);
            }
        }
    }
}