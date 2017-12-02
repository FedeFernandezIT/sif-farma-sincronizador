using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisfarma.Sincronizador.Farmatic
{
    public class FarmaticContext : DbContext
    {
        public FarmaticContext()
            : base("FarmaticContext")
        {
        }
    }
}
