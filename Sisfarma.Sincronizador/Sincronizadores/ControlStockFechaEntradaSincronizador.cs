using Sisfarma.Sincronizador.Consejo;
using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Fisiotes;
using Sisfarma.Sincronizador.Helpers;
using Sisfarma.Sincronizador.Sincronizadores.SuperTypes;
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

        public override void Process() => ProcessControlStockFechasEntrada(_farmatic, _fisiotes, _consejo);

        private  void ProcessControlStockFechasEntrada(FarmaticService farmatic, FisiotesService fisiotes, ConsejoService consejo)
        {                        
            var configuracion = fisiotes.Configuraciones.GetByCampo(FIELD_STOCK_ENTRADA);

            var fechaActualizacionStock = Calculator.CalculateFechaActualizacion(configuracion);

            var articulosWithIva = farmatic.Articulos.GetByFechaUltimaEntradaGreaterOrEqual(fechaActualizacionStock);

            foreach (var articulo in articulosWithIva)
            {
                var strFecha = articulo.FechaUltimaEntrada?.ToString("yyyy-MM-dd");
                fisiotes.Configuraciones.Update(FIELD_STOCK_ENTRADA, strFecha);

                var medicamentoGenerado = Generator.GenerarMedicamento(farmatic, consejo, articulo);
                var medicamento = fisiotes.Medicamentos.GetOneOrDefaultByCodNacional(articulo.IdArticu);

                SincronizarMedicamento(fisiotes, medicamento, medicamentoGenerado);
            }
        }
    }
}
