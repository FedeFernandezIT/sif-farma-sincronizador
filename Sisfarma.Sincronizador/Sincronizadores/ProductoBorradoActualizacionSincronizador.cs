using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Fisiotes;
using Sisfarma.Sincronizador.Sincronizadores.SuperTypes;
using static Sisfarma.Sincronizador.Fisiotes.Repositories.ConfiguracionesRepository;

namespace Sisfarma.Sincronizador.Sincronizadores
{
    public class ProductoBorradoActualizacionSincronizador : BaseSincronizador
    {
        private const string FIELD_POR_DONDE_VOY_BORRAR = FieldsConfiguracion.FIELD_POR_DONDE_VOY_BORRAR;

        public ProductoBorradoActualizacionSincronizador(FarmaticService farmatic, FisiotesService fisiotes) 
            : base(farmatic, fisiotes)
        {
        }

        public override void Process() => ProcessUpdateProductosBorrados(_farmatic, _fisiotes);

        private  void ProcessUpdateProductosBorrados(FarmaticService farmatic, FisiotesService fisiotes)
        {
            var codArticulo = fisiotes.Configuraciones.GetByCampo(FIELD_POR_DONDE_VOY_BORRAR);
            
            var medicamentos = fisiotes.Medicamentos
                .GetGreaterOrEqualCodigosNacionales(codArticulo);
            
            fisiotes.Configuraciones.Update(FIELD_POR_DONDE_VOY_BORRAR, "0");
            foreach (var med in medicamentos)
            {
                _cancellationToken.ThrowIfCancellationRequested();

                if (!farmatic.Articulos.Exists(med.cod_nacional.PadLeft(6, '0')))
                    fisiotes.Medicamentos.DeleteByCodigoNacional(med.cod_nacional);

                fisiotes.Configuraciones.Update(FIELD_POR_DONDE_VOY_BORRAR, med.cod_nacional);
            }
        }
    }
}
