using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Fisiotes;
using System;

namespace Sisfarma.Sincronizador.Sincronizadores.SuperTypes
{
    public abstract class TaskSincronizador : BaseSincronizador
    {
        protected FarmaticService _farmatic;

        public TaskSincronizador(FarmaticService farmatic, FisiotesService fisiotes)
            : base(fisiotes)
        {
            _farmatic = farmatic ?? throw new ArgumentNullException(nameof(farmatic));            
        }
    }
}
