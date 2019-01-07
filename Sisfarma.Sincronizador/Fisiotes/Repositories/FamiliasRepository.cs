using Sisfarma.Sincronizador.Fisiotes.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisfarma.Sincronizador.Fisiotes.Repositories
{
    public class FamiliasRepository : FisiotesRepository
    {
        public FamiliasRepository(FisiotesContext ctx) : base(ctx)
        {
        }

        public Familia GetByFamilia(string familia)
        {
            var sql = @"select * from familia where familia = @familia";
            return _ctx.Database.SqlQuery<Familia>(sql,
                new SqlParameter("familia", familia))
                .FirstOrDefault();
        }

        public void Insert(string familia)
        {
            var sql = @"INSERT IGNORE INTO familia (familia) VALUES(@familia)";
            _ctx.Database.ExecuteSqlCommand(sql,
                new SqlParameter("familia", familia));
        }
    }
}