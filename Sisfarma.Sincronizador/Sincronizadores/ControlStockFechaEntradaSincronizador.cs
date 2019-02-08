using Sisfarma.Sincronizador.Consejo;
using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Fisiotes;
using Sisfarma.Sincronizador.Helpers;
using Sisfarma.Sincronizador.Sincronizadores.SuperTypes;
using System.Linq;
using static Sisfarma.Sincronizador.Fisiotes.Repositories.ConfiguracionesRepository;

namespace Sisfarma.Sincronizador.Sincronizadores
{
    public class ControlStockFechaEntradaSincronizador : ControlSincronizador
    {
        private const string FIELD_STOCK_ENTRADA = FieldsConfiguracion.FIELD_STOCK_ENTRADA;

        public ControlStockFechaEntradaSincronizador(FarmaticService farmatic, FisiotesService fisiotes, ConsejoService consejo) 
            : base(farmatic, fisiotes, consejo)
        {
        }

        public override void Process() => ProcessControlStockFechasEntrada();

        private  void ProcessControlStockFechasEntrada()
        {                        
            var configuracion = _fisiotes.Configuraciones.GetByCampo(FIELD_STOCK_ENTRADA);

            var fechaActualizacionStock = Calculator.CalculateFechaActualizacion(configuracion);

            var articulosWithIva = _farmatic.Articulos.GetByFechaUltimaEntradaGreaterOrEqual(fechaActualizacionStock);
            if (!articulosWithIva.Any())
            {
                _fisiotes.Configuraciones.Update(FIELD_STOCK_ENTRADA, "0");
                _fisiotes.Medicamentos.ResetPorDondeVoy();
                return;
            }

            foreach (var articulo in articulosWithIva)
            {
                _cancellationToken.ThrowIfCancellationRequested();

                var strFecha = articulo.FechaUltimaEntrada?.ToString("yyyy-MM-dd");
                _fisiotes.Configuraciones.Update(FIELD_STOCK_ENTRADA, strFecha);

                var medicamentoGenerado = Generator.GenerarMedicamento(_farmatic, _consejo, articulo);
                var medicamento = _fisiotes.Medicamentos.GetOneOrDefaultByCodNacional(articulo.IdArticu);

                SincronizarMedicamento(_fisiotes, medicamento, medicamentoGenerado);
            }

            if (_farmatic.Articulos.GetControlArticuloFisrtOrDefault(articulosWithIva.Last().IdArticu) == null)
            {
                _fisiotes.Configuraciones.Update(FIELD_STOCK_ENTRADA, "0");
                _fisiotes.Medicamentos.ResetPorDondeVoySinStock();
            }
        }
    }
}
