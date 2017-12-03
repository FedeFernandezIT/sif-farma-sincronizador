using MySql.Data.MySqlClient;
using Sisfarma.Sincronizador.Fisiotes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisfarma.Sincronizador.Fisiotes.Repositories
{
    public class CategoriasRepository : FisiotesRepository
    {
        public CategoriasRepository(FisiotesContext ctx) : base(ctx)
        {
        }

        public Categoria GetByCategoriaAndPadre(string categoria, string padre)
        {            
            var sql = @"select * from ps_categorias where categoria = @categoria AND padre = @padre";
            return _ctx.Database.SqlQuery<Categoria>(sql,
                new MySqlParameter("categoria", categoria),
                new MySqlParameter("padre", padre))
                .FirstOrDefault();
        }

        public Categoria GetByPadre(string padre)
        {
            var sql = @"select * from ps_categorias where padre = @padre";
            return _ctx.Database.SqlQuery<Categoria>(sql,
                new MySqlParameter("padre", padre))
                .FirstOrDefault();
        }

        public void Insert(string categoria, string padre, int? prestaShop)
        {            
            var sql = @"INSERT IGNORE INTO ps_categorias (categoria, padre, prestashopPadreId) VALUES(" +
                    @"@categoria, @padre, @prestaShop)";
            _ctx.Database.ExecuteSqlCommand(sql,
                new MySqlParameter("categoria", categoria),
                new MySqlParameter("padre", padre),
                new MySqlParameter("prestaShop", prestaShop));         
        }
    }
}
