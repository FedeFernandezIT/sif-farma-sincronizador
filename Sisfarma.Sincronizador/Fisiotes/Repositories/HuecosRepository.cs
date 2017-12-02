using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisfarma.Sincronizador.Fisiotes.Repositories
{
    public class HuecosRepository : FisiotesRepository
    {
        public HuecosRepository(FisiotesContext ctx) : base(ctx)
        {
        }

        public void CreateTable()
        {
            var sql = @"CREATE TABLE IF NOT EXISTS `clientes_huecos` (`hueco` varchar(255) DEFAULT NULL) ENGINE=MyISAM DEFAULT CHARSET=latin1;";
            _ctx.Database.ExecuteSqlCommand(sql);
        }

        public bool Any(int value)
        {
            var sql = @"SELECT * FROM clientes_huecos WHERE hueco = @value";
            return _ctx.Database.SqlQuery<string>(sql,
                new MySqlParameter("value", value))
                .Any();
        }

        public void Insert(string hueco)
        {
            var sql = "INSERT IGNORE INTO clientes_huecos (hueco) VALUES (@hueco)";
            _ctx.Database.ExecuteSqlCommand(sql,
                new MySqlParameter("hueco", hueco));
        }

        public IEnumerable<string> GetByOrderAsc()
        {
            var sql = @"SELECT hueco FROM clientes_huecos ORDER BY hueco ASC";
            return _ctx.Database.SqlQuery<string>(sql).ToList();
        }

        public void Delete(string hueco)
        {
            var sql = @"DELETE FROM clientes_huecos WHERE hueco = @hueco";
            _ctx.Database.ExecuteSqlCommand(sql,
                    new MySqlParameter("hueco", hueco));
        }
    }
}
