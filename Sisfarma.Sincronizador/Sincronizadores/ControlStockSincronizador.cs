using Sisfarma.Sincronizador.Consejo;
using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Fisiotes;
using Sisfarma.Sincronizador.Helpers;
using Sisfarma.Sincronizador.Sincronizadores.SuperTypes;
using static Sisfarma.Sincronizador.Fisiotes.Repositories.ConfiguracionesRepository;

namespace Sisfarma.Sincronizador.Sincronizadores
{
    public class ControlStockSincronizador : ControlSincronizador
    {        
        const string FIELD_POR_DONDE_VOY_CON_STOCK = FieldsConfiguracion.FIELD_POR_DONDE_VOY_CON_STOCK;             

        public ControlStockSincronizador(FarmaticService farmatic, FisiotesService fisiotes, ConsejoService consejo) 
            : base(farmatic, fisiotes, consejo)
        {            
        }

        public override void Process() => ProcessControlStockInicial(_farmatic, _fisiotes, _consejo);

        public void ProcessControlStockInicial(FarmaticService farmatic, FisiotesService fisiotes, ConsejoService consejo)
        {
            var configuracion = fisiotes.Configuraciones.GetByCampo(FIELD_POR_DONDE_VOY_CON_STOCK);
            var codArticulo = !string.IsNullOrEmpty(configuracion)
                ? configuracion
                : "0";

            var articulos = farmatic.Articulos.GetWithStockByIdGreaterOrEqual(codArticulo);
            foreach (var articulo in articulos)
            {
                fisiotes.Configuraciones.Update(FIELD_POR_DONDE_VOY_CON_STOCK, articulo.IdArticu);

                var medicamentoGenerado = Generator.GenerarMedicamento(farmatic, consejo, articulo);
                var medicamento = fisiotes.Medicamentos.GetOneOrDefaultByCodNacional(articulo.IdArticu);                

                SincronizarMedicamento(fisiotes, medicamento, medicamentoGenerado);
            }

            fisiotes.Configuraciones.Update(FIELD_POR_DONDE_VOY_CON_STOCK, "0");
            fisiotes.Medicamentos.ResetPorDondeVoy();
        }
    }
}
