using Sisfarma.Sincronizador.Consejo;
using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Fisiotes;
using Sisfarma.Sincronizador.Fisiotes.Models;
using Sisfarma.Sincronizador.Helpers;
using Sisfarma.Sincronizador.Sincronizadores.SuperTypes;
using System.Linq;

namespace Sisfarma.Sincronizador.Sincronizadores
{
    public class ControlStockSincronizador : ControlSincronizador
    {
        public ControlStockSincronizador(FarmaticService farmatic, FisiotesService fisiotes, ConsejoService consejo)
            : base(farmatic, fisiotes, consejo)
        {
        }

        public override void Process() => ProcessControlStockInicial();

        public void ProcessControlStockInicial()
        {
            var configuracion = _fisiotes.Configuraciones.GetByCampo(Configuracion.FIELD_POR_DONDE_VOY_CON_STOCK);
            var codArticulo = !string.IsNullOrEmpty(configuracion)
                ? configuracion
                : "0";

            var articulos = _farmatic.Articulos.GetWithStockByIdGreaterOrEqual(codArticulo);
            if (!articulos.Any())
            {
                _fisiotes.Configuraciones.Update(Configuracion.FIELD_POR_DONDE_VOY_CON_STOCK, "0");
                return;
            }

            foreach (var articulo in articulos)
            {
                _cancellationToken.ThrowIfCancellationRequested();

                var medicamentoGenerado = Generator.GenerarMedicamento(_farmatic, _consejo, articulo);
                _fisiotes.Medicamentos.Insert(medicamentoGenerado);
            }

            if (_farmatic.Articulos.GetControlArticuloFisrtOrDefault(articulos.Last().IdArticu) == null)
            {
                _fisiotes.Configuraciones.Update(Configuracion.FIELD_POR_DONDE_VOY_CON_STOCK, "0");
            }
        }
    }
}