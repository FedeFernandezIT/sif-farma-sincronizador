using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sisfarma.Sincronizador.Sincronizadores.SuperTypes
{
    public abstract class Sincronizador : ISincronizador
    {
        protected CancellationToken _cancellationToken;

        public abstract void Process();

        public virtual async Task Run(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            while (true)
            {
                try
                {
                    _cancellationToken.ThrowIfCancellationRequested();
                    Process();
                }
                catch (Exception ex)
                {
                    await Task.Delay(200);
                }
                finally
                {
                    await Task.Delay(200);
                }
            }
        }
    }
}
