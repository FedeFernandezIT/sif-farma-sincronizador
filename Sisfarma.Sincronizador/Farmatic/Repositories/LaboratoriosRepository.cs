using Sisfarma.Sincronizador.Farmatic.Models;
using System.Data.SqlClient;
using System.Linq;

namespace Sisfarma.Sincronizador.Farmatic.Repositories
{
    public class LaboratoriosRepository : FarmaticRepository
    {
        public LaboratoriosRepository(FarmaticContext ctx) : base(ctx)
        {
        }

        public Laboratorio GetById(string codigo)
        {
            var sql = @"SELECT * FROM laboratorio WHERE codigo = @codigo";
            return _ctx.Database.SqlQuery<Laboratorio>(sql,
                new SqlParameter("codigo", codigo))
                .FirstOrDefault();
        }
    }
}
