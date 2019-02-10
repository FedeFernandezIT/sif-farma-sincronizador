using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Fisiotes;
using Sisfarma.Sincronizador.Sincronizadores.SuperTypes;
using System.Linq;
using static Sisfarma.Sincronizador.Fisiotes.Repositories.ConfiguracionesRepository;

namespace Sisfarma.Sincronizador.Sincronizadores
{
    public class ProductoBorradoActualizacionSincronizador : TaskSincronizador
    {
        private const string FIELD_POR_DONDE_VOY_BORRAR = FieldsConfiguracion.FIELD_POR_DONDE_VOY_BORRAR;

        public ProductoBorradoActualizacionSincronizador(FarmaticService farmatic, FisiotesService fisiotes) 
            : base(farmatic, fisiotes)
        {
        }

        public override void Process() => ProcessUpdateProductosBorrados();

        private  void ProcessUpdateProductosBorrados()
        {
            var codArticulo = _fisiotes.Configuraciones.GetByCampo(FIELD_POR_DONDE_VOY_BORRAR);
            
            var medicamentos = _fisiotes.Medicamentos
                .GetGreaterOrEqualCodigosNacionales(codArticulo);
            if (!medicamentos.Any())
            {
                _fisiotes.Configuraciones.Update(FIELD_POR_DONDE_VOY_BORRAR, "0");
                return;
            }

            if (medicamentos.Count() == 1)
            {
                var med = medicamentos.First();
                if (!_farmatic.Articulos.Exists(med.cod_nacional.PadLeft(6, '0')))
                    _fisiotes.Medicamentos.DeleteByCodigoNacional(med.cod_nacional);
                _fisiotes.Configuraciones.Update(FIELD_POR_DONDE_VOY_BORRAR, "0");
            }
                        
            foreach (var med in medicamentos)
            {
                _cancellationToken.ThrowIfCancellationRequested();

                if (!_farmatic.Articulos.Exists(med.cod_nacional.PadLeft(6, '0')))
                    _fisiotes.Medicamentos.DeleteByCodigoNacional(med.cod_nacional);

                _fisiotes.Configuraciones.Update(FIELD_POR_DONDE_VOY_BORRAR, med.cod_nacional);
            }            
        }
    }
}
