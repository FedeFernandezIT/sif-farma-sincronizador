using Sisfarma.RestClient.Exceptions;
using Sisfarma.Sincronizador.Extensions;
using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Fisiotes;
using Sisfarma.Sincronizador.Helpers;
using System;
using System.Threading.Tasks;
using static Sisfarma.Sincronizador.Fisiotes.Repositories.ConfiguracionesRepository;

namespace Sisfarma.Sincronizador.Sincronizadores.SuperTypes
{
    public abstract class BaseSincronizador : ISincronizador
    {
        private const string FIELD_LOG_ERRORS = FieldsConfiguracion.FIELD_LOG_ERRORS;

        protected FarmaticService _farmatic;
        protected FisiotesService _fisiotes;

        public BaseSincronizador(FarmaticService farmatic, FisiotesService fisiotes)
        {
            _farmatic = farmatic ?? throw new ArgumentNullException(nameof(farmatic));
            _fisiotes = fisiotes ?? throw new ArgumentNullException(nameof(fisiotes));
        }


        public abstract void Process();

        public virtual async Task Run()
        {
            while (true)
            {
                try
                {
                    Process();
                }
                catch (RestClientException ex)
                {
                    LogError(ex.ToLogErrorMessage());                                        
                }
                catch (Exception ex)
                {
                    LogError(ex.ToLogErrorMessage());
                }
                finally
                {
                    await Task.Delay(200);
                }
            }
        }

        private void LogError(string message)
        {            
            var hash = Cryptographer.GenerateMd5Hash(message);

            var logsPrevios = _fisiotes.Configuraciones.GetByCampo(FIELD_LOG_ERRORS);            
            if (logsPrevios.Contains(hash))
                return;
            
            var log = $@"$log{{{hash}}}{Environment.NewLine}{message}";
            var logs = $@"{logsPrevios}{Environment.NewLine}{log}";
            _fisiotes.Configuraciones.Update(FIELD_LOG_ERRORS, logs);
            
        }
    }
}
    