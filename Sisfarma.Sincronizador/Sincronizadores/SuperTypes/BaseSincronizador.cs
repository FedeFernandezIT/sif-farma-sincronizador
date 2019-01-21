using Sisfarma.RestClient.Exceptions;
using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Fisiotes;
using System;
using System.Threading.Tasks;

namespace Sisfarma.Sincronizador.Sincronizadores.SuperTypes
{
    public abstract class BaseSincronizador : ISincronizador
    {
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
                    Console.WriteLine(ex.Content);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    await Task.Delay(200);
                }
            }
        }
    }
}
    