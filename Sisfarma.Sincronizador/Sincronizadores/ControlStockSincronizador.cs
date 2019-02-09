using Sisfarma.Sincronizador.Consejo;
using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Fisiotes;
using Sisfarma.Sincronizador.Helpers;
using Sisfarma.Sincronizador.Sincronizadores.SuperTypes;
using System.Linq;
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

        public override void Process() => ProcessControlStockInicial();

        public void ProcessControlStockInicial()
        {
            var configuracion = _fisiotes.Configuraciones.GetByCampo(FIELD_POR_DONDE_VOY_CON_STOCK);
            var codArticulo = !string.IsNullOrEmpty(configuracion)
                ? configuracion
                : "0";

            var articulos = _farmatic.Articulos.GetWithStockByIdGreaterOrEqual(codArticulo);
            if (!articulos.Any())
            {
                _fisiotes.Configuraciones.Update(FIELD_POR_DONDE_VOY_CON_STOCK, "0");
                _fisiotes.Medicamentos.ResetPorDondeVoy();
                return;
            }

            foreach (var articulo in articulos)
            {
                _cancellationToken.ThrowIfCancellationRequested();

                _fisiotes.Configuraciones.Update(FIELD_POR_DONDE_VOY_CON_STOCK, articulo.IdArticu);

                var medicamentoGenerado = Generator.GenerarMedicamento(_farmatic, _consejo, articulo);
                var medicamento = _fisiotes.Medicamentos.GetOneOrDefaultByCodNacional(articulo.IdArticu);                

                SincronizarMedicamento(_fisiotes, medicamento, medicamentoGenerado);
            }

            if (_farmatic.Articulos.GetControlArticuloFisrtOrDefault(articulos.Last().IdArticu) == null)
            {                
                _fisiotes.Medicamentos.ResetPorDondeVoy();
            }

            _fisiotes.Configuraciones.Update(FIELD_POR_DONDE_VOY_CON_STOCK, "0");
        }
    }
}
