using Sisfarma.Sincronizador.Farmatic.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisfarma.Sincronizador.Farmatic.Repositories
{
    public class FamiliasRepository : FarmaticRepository
    {
        public FamiliasRepository(FarmaticContext ctx) : base(ctx)
        {
        }

        public Familia GetById(short id)
        {
            var sql = @"SELECT * FROM familia WHERE IdFamilia = @id";
            return _ctx.Database.SqlQuery<Familia>(sql,
                    new SqlParameter("id", id))
                .FirstOrDefault();
        }

        public string GetSuperFamiliaDescripcionByFamilia(string familia)
        {
            var sql =
                @"SELECT sf.Descripcion FROM SuperFamilia sf INNER JOIN FamiliaAux fa ON fa.IdSuperFamilia = sf.IdSuperFamilia " +
                @" INNER JOIN Familia f ON f.IdFamilia = fa.IdFamilia WHERE f.Descripcion = @familia";
            return _ctx.Database.SqlQuery<string>(sql,
                new SqlParameter("familia", familia))
                .FirstOrDefault();
        }
    }
}
