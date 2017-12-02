using Sisfarma.Sincronizador.Farmatic.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisfarma.Sincronizador.Farmatic.Repositories
{
    public class SinonimosRepository : FarmaticRepository
    {
        public SinonimosRepository(FarmaticContext ctx) : base(ctx)
        {
        }

        public List<Sinonimos> Get()
        {
            var sql = @"SELECT * FROM Sinonimo";
            return _ctx.Database.SqlQuery<Sinonimos>(sql)
                .ToList();
        }

        public Sinonimos GetByArticulo(string codigo)
        {
            var sql = @"SELECT * FROM sinonimo WHERE IdArticu = @codigo";
            return _ctx.Database.SqlQuery<Sinonimos>(sql,
                new SqlParameter("codigo", codigo))
                .FirstOrDefault();
        }
    }
}
