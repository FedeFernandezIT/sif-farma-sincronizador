using System;
using Sisfarma.Sincronizador.Consejo;
using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Fisiotes;
using Sisfarma.Sincronizador.Helpers;
using Sisfarma.Sincronizador.Sincronizadores.SuperTypes;
using static Sisfarma.Sincronizador.Fisiotes.Repositories.ConfiguracionesRepository;

namespace Sisfarma.Sincronizador.Sincronizadores
{
    public class ControlSinStockSincronizador : ControlSincronizador
    {
        private const string FIELD_POR_DONDE_VOY_SIN_STOCK = FieldsConfiguracion.FIELD_POR_DONDE_VOY_SIN_STOCK;
        
        public ControlSinStockSincronizador(FarmaticService farmatic, FisiotesService fisiotes, ConsejoService consejo) 
            : base(farmatic, fisiotes, consejo)
        {        
        }

        public override void Process() => ProcessControlSinStockInicial(_farmatic, _fisiotes, _consejo);

        public void ProcessControlSinStockInicial(FarmaticService farmatic, FisiotesService fisiotes, ConsejoService consejo)
        {

            var valorConfiguracion = fisiotes.Configuraciones.GetByCampo(FIELD_POR_DONDE_VOY_SIN_STOCK);

            var codArticulo = !string.IsNullOrEmpty(valorConfiguracion)
                ? valorConfiguracion
                : "0";

            var articulos = farmatic.Articulos.GetWithoutStockByIdGreaterOrEqual(codArticulo);
            foreach (var articulo in articulos)
            {
                fisiotes.Configuraciones.Update(FIELD_POR_DONDE_VOY_SIN_STOCK, articulo.IdArticu);

                var medicamentoGenerado = Generator.GenerarMedicamento(farmatic, consejo, articulo);
                var medicamento = fisiotes.Medicamentos.GetOneOrDefaultByCodNacional(articulo.IdArticu);

                SincronizarMedicamento(fisiotes, medicamento, medicamentoGenerado);                
            }

            fisiotes.Configuraciones.Update(FIELD_POR_DONDE_VOY_SIN_STOCK, "0");
            fisiotes.Medicamentos.ResetPorDondeVoySinStock();
        }                
    }
}
