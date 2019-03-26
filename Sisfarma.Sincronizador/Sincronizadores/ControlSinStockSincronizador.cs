using System.Linq;
using Sisfarma.Sincronizador.Consejo;
using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Fisiotes;
using Sisfarma.Sincronizador.Fisiotes.Models;
using Sisfarma.Sincronizador.Helpers;
using Sisfarma.Sincronizador.Sincronizadores.SuperTypes;

namespace Sisfarma.Sincronizador.Sincronizadores
{
    public class ControlSinStockSincronizador : ControlSincronizador
    {
        public ControlSinStockSincronizador(FarmaticService farmatic, FisiotesService fisiotes, ConsejoService consejo)
            : base(farmatic, fisiotes, consejo)
        { }

        public override void Process() => ProcessControlSinStockInicial();

        public void ProcessControlSinStockInicial()
        {
            var valorConfiguracion = _fisiotes.Configuraciones.GetByCampo(Configuracion.FIELD_POR_DONDE_VOY_SIN_STOCK);

            var codArticulo = !string.IsNullOrEmpty(valorConfiguracion)
                ? valorConfiguracion
                : "0";

            var articulos = _farmatic.Articulos.GetWithoutStockByIdGreaterOrEqual(codArticulo);
            if (!articulos.Any())
            {
                _fisiotes.Configuraciones.Update(Configuracion.FIELD_POR_DONDE_VOY_SIN_STOCK, "0");
                return;
            }

            foreach (var articulo in articulos)
            {
                _cancellationToken.ThrowIfCancellationRequested();

                var medicamentoGenerado = Generator.GenerarMedicamento(_farmatic, _consejo, articulo);
                _fisiotes.Medicamentos.Insert(medicamentoGenerado);
            }

            if (_farmatic.Articulos.GetControlArticuloSinStockFisrtOrDefault(articulos.Last().IdArticu) == null)
            {
                _fisiotes.Configuraciones.Update(Configuracion.FIELD_POR_DONDE_VOY_SIN_STOCK, "0");
            }
        }
    }
}