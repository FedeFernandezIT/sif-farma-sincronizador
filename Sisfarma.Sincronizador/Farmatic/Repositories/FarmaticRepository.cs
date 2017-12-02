using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisfarma.Sincronizador.Farmatic.Repositories
{
    public abstract class FarmaticRepository
    {
        protected FarmaticContext _ctx;

        public FarmaticRepository(FarmaticContext ctx)
        {
            _ctx = ctx;
        }
    }
}
