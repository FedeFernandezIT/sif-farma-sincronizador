using Sisfarma.Sincronizador.Fisiotes.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisfarma.Sincronizador.Fisiotes.Repositories
{
    public class SinonimosRepository : FisiotesRepository
    {
        public SinonimosRepository(FisiotesContext ctx) : base(ctx)
        {
        }

        public void CheckAndAlterFields(string remote)
        {
            var sql = @"SELECT data_type AS tipo From information_schema.Columns WHERE TABLE_SCHEMA = @baseRemoto AND TABLE_NAME = 'sinonimos' AND COLUMN_NAME = 'cod_barras'";
            var type = _ctx.Database.SqlQuery<string>(sql,
                new SqlParameter("baseRemoto", remote))
                .FirstOrDefault();
            if (type != null)
            {
                if (type.Equals("varchar", StringComparison.CurrentCultureIgnoreCase))
                {
                    sql = @"ALTER TABLE sinonimos MODIFY COLUMN cod_nacional VARCHAR(255);";
                    _ctx.Database.ExecuteSqlCommand(sql);

                    sql = @"ALTER TABLE sinonimos MODIFY COLUMN cod_barras VARCHAR(255);";
                    _ctx.Database.ExecuteSqlCommand(sql);
                }
            }
        }

        public Sinonimo First()
        {
            var sql = @"SELECT * FROM sinonimos LIMIT 0,1";
            return _ctx.Database.SqlQuery<Sinonimo>(sql)
                .FirstOrDefault();
        }

        public void Truncate()
        {
            var sql = @"TRUNCATE sinonimos";
            _ctx.Database.ExecuteSqlCommand(sql);
        }

        public void Insert(List<Sinonimo> items)
        {
            var sql = @"INSERT IGNORE INTO sinonimos (cod_barras,cod_nacional) VALUES ";
            foreach (var item in items)
            {
                sql += $@"('{item.cod_barras}', '{item.cod_nacional}'),";
            }
            sql = sql.TrimEnd(',');
            _ctx.Database.ExecuteSqlCommand(sql);
        }
    }
}