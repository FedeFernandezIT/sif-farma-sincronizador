using Sisfarma.Sincronizador.Fisiotes.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisfarma.Sincronizador.Fisiotes.Repositories
{
    public class ListasArticulosRepository : FisiotesRepository
    {
        public ListasArticulosRepository(FisiotesContext ctx) : base(ctx)
        {
        }

        public void Insert(int lista, int articulo)
        {
            var sql = @"INSERT IGNORE INTO listas_articulos (cod_lista,cod_articulo) VALUES (@lista, @articulo)";
            _ctx.Database.ExecuteSqlCommand(sql,
                new SqlParameter("lista", lista),
                new SqlParameter("articulo", articulo));
        }

        public void Insert(List<ListaArticulo> items)
        {
            var sql = @"INSERT IGNORE INTO listas_articulos (cod_lista,cod_articulo) VALUES ";
            foreach (var item in items)
            {
                sql += $@"('{item.cod_lista}', '{item.cod_articulo}'),";
            }
            sql = sql.TrimEnd(',');
            _ctx.Database.ExecuteSqlCommand(sql);
        }

        public void Delete(int codigo)
        {
            var sql = @"DELETE FROM listas_articulos WHERE cod_lista = @codigo";
            _ctx.Database.ExecuteSqlCommand(sql,
                new SqlParameter("codigo", codigo));
        }
    }
}