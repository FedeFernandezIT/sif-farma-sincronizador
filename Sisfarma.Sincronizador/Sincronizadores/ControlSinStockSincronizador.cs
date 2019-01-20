using System;
using Sisfarma.Sincronizador.Consejo;
using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Fisiotes;
using Sisfarma.Sincronizador.Helpers;
using static Sisfarma.Sincronizador.Fisiotes.Repositories.ConfiguracionesRepository;

namespace Sisfarma.Sincronizador.Sincronizadores
{
    public class ControlSinStockSincronizador : BaseSincronizador
    {
        private const string FIELD_POR_DONDE_VOY_SIN_STOCK = FieldsConfiguracion.FIELD_POR_DONDE_VOY_SIN_STOCK;

        private readonly ConsejoService _consejo;        

        public ControlSinStockSincronizador(FarmaticService farmatic, FisiotesService fisiotes, ConsejoService consejo) 
            : base(farmatic, fisiotes)
        {
            _consejo = consejo ?? throw new ArgumentNullException(nameof(consejo));
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
                if (medicamento == null)
                    fisiotes.Medicamentos.Insert(medicamentoGenerado);
                else
                {
                    if (HayDiferencias(medicamento, medicamentoGenerado))
                        fisiotes.Medicamentos.Update(medicamentoGenerado, withSqlExtra: true);
                    else
                        fisiotes.Medicamentos.Update(medicamentoGenerado);
                }
            }

            fisiotes.Configuraciones.Update(FIELD_POR_DONDE_VOY_SIN_STOCK, "0");
            fisiotes.Medicamentos.ResetPorDondeVoySinStock();
        }

        private bool HayDiferencias(Fisiotes.Models.Medicamento remoto, Fisiotes.Models.Medicamento generado)
        {
            return
                generado.nombre != remoto.nombre ||
                generado.precio != remoto.precio ||
                generado.laboratorio != remoto.laboratorio ||
                generado.iva != remoto.iva ||
                generado.stock != remoto.stock ||
                generado.presentacion != remoto.presentacion ||
                generado.descripcion != remoto.descripcion;
        }        
    }
}
