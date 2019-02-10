using System;
using System.Linq;
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

        public override void Process() => ProcessControlSinStockInicial();

        public void ProcessControlSinStockInicial()
        {

            var valorConfiguracion = _fisiotes.Configuraciones.GetByCampo(FIELD_POR_DONDE_VOY_SIN_STOCK);

            var codArticulo = !string.IsNullOrEmpty(valorConfiguracion)
                ? valorConfiguracion
                : "0";

            var articulos = _farmatic.Articulos.GetWithoutStockByIdGreaterOrEqual(codArticulo);
            if (!articulos.Any())
            {
                _fisiotes.Configuraciones.Update(FIELD_POR_DONDE_VOY_SIN_STOCK, "0");
                return;
            }

            foreach (var articulo in articulos)
            {
                _cancellationToken.ThrowIfCancellationRequested();

                _fisiotes.Configuraciones.Update(FIELD_POR_DONDE_VOY_SIN_STOCK, articulo.IdArticu);

                var medicamentoGenerado = Generator.GenerarMedicamento(_farmatic, _consejo, articulo);
                var medicamento = _fisiotes.Medicamentos.GetOneOrDefaultByCodNacional(articulo.IdArticu);

                SincronizarMedicamento(_fisiotes, medicamento, medicamentoGenerado);                
            }

            if (_farmatic.Articulos.GetControlArticuloSinStockFisrtOrDefault(articulos.Last().IdArticu) == null)
            {
                _fisiotes.Configuraciones.Update(FIELD_POR_DONDE_VOY_SIN_STOCK, "0");
            }            
        }                
    }
}
