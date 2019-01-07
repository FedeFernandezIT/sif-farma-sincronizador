using Sisfarma.Sincronizador.Fisiotes.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisfarma.Sincronizador.Fisiotes.Repositories
{
    public class ListasRepository : FisiotesRepository
    {
        public ListasArticulosRepository DeArticulos { get; set; }

        public ListasRepository(FisiotesContext ctx) : base(ctx)
        {
            DeArticulos = new ListasArticulosRepository(ctx);
        }

        public void CheckAndCreatePorDondeVoyField()
        {
            const string table = @"SELECT * from listas LIMIT 0,1;";
            var fields = new[] { "porDondeVoy" };
            var alters = new[]
            {
                @"ALTER TABLE listas ADD porDondeVoy TINYINT(1) DEFAULT 0;"
            };
            CheckAndCreateFieldsTemplate(table, fields, alters);
        }

        public int GetCodPorDondeVoy()
        {
            var sql = @"SELECT cod FROM listas WHERE porDondeVoy = 1 LIMIT 0,1";
            var result = _ctx.Database.SqlQuery<int>(sql).ToList();
            return result.Any()
                ? result.First()
                : -1;
        }

        public Lista Get(int codigo)
        {
            var sql = @"SELECT * FROM listas WHERE cod = @codigo";
            return _ctx.Database.SqlQuery<Lista>(sql,
                new SqlParameter("codigo", codigo))
                .FirstOrDefault();
        }

        public void UpdateWithPorDondeVoy(int codigo, string lista)
        {
            var sql = @"UPDATE IGNORE listas SET lista = @lista, porDondeVoy = 1 WHERE cod = @codigo";
            _ctx.Database.ExecuteSqlCommand(sql,
                new SqlParameter("codigo", codigo),
                new SqlParameter("lista", lista));
        }

        public void Update(int codigo, string lista)
        {
            var sql = @"UPDATE IGNORE listas SET lista = @lista WHERE cod = @codigo";
            _ctx.Database.ExecuteSqlCommand(sql,
                new SqlParameter("codigo", codigo),
                new SqlParameter("lista", lista));
        }

        public void InsertWithPorDondeVoy(int codigo, string lista)
        {
            var sql = @"INSERT IGNORE INTO listas (cod, lista, porDondeVoy) VALUES(@codigo, @lista, 1)";
            _ctx.Database.ExecuteSqlCommand(sql,
                new SqlParameter("codigo", codigo),
                new SqlParameter("lista", lista));
        }

        public void Insert(int codigo, string lista)
        {
            var sql = @"INSERT IGNORE INTO listas (cod, lista) VALUES(@codigo, @lista)";
            _ctx.Database.ExecuteSqlCommand(sql,
                new SqlParameter("codigo", codigo),
                new SqlParameter("lista", lista));
        }

        public void ResetPorDondeVoy()
        {
            var sql = @"UPDATE IGNORE listas SET porDondeVoy = 0";
            _ctx.Database.ExecuteSqlCommand(sql);
        }
    }
}