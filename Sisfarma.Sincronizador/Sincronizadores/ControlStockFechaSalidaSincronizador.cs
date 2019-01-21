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
        {
        }

        public override void Process() => ProcessControlStockFechasSalida(_farmatic, _fisiotes, _consejo);

        private void ProcessControlStockFechasSalida(FarmaticService farmatic, FisiotesService fisiotes, ConsejoService consejo)
        {            
            var configuracion = fisiotes.Configuraciones.GetByCampo(FIELD_STOCK_SALIDA);

            var fechaActualizacionStock = Calculator.CalculateFechaActualizacion(configuracion);
            
            var articulosWithIva = farmatic.Articulos.GetByFechaUltimaSalidaGreaterOrEqual(fechaActualizacionStock);
            foreach (var articulo in articulosWithIva)
            {
                var strFecha = articulo.FechaUltimaSalida?.ToString("yyyy-MM-dd");
                fisiotes.Configuraciones.Update(FIELD_STOCK_SALIDA, strFecha);

                var medicamentoGenerado = Generator.GenerarMedicamento(farmatic, consejo, articulo);
                var medicamento = fisiotes.Medicamentos.GetOneOrDefaultByCodNacional(articulo.IdArticu);

                SincronizarMedicamento(fisiotes, medicamento, medicamentoGenerado);
            }
        }
    }
}
