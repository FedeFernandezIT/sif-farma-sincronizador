using Sisfarma.Sincronizador.Farmatic.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisfarma.Sincronizador.Farmatic.Repositories
{
    public class EncargosRepository : FarmaticRepository
    {
        public EncargosRepository(FarmaticContext ctx) : base(ctx)
        {
        }

        public IEnumerable<Encargo> GetByContadorGreaterOrEqual(long? contador)
        {            
            var sql = @"SELECT * From Encargo WHERE year(idFecha) >= 2015 AND IdContador >= @contador Order by IdContador ASC";
            return _ctx.Database.SqlQuery<Encargo>(sql,
                new SqlParameter("contador", contador ?? SqlInt64.Null))
                .ToList();         
        }
    }
}
