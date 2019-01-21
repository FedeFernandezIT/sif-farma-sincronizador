using Sisfarma.Sincronizador.Farmatic.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Sisfarma.Sincronizador.Farmatic.Repositories
{
    public class ListasArticulosRepository : FarmaticRepository
    {
        public ListasArticulosRepository(FarmaticContext ctx) : base(ctx)
        {
        }

        public ListaArticulo GetOneOrDefault(int lista)
        {            
            var sql = @"SELECT * FROM ListaArticu WHERE fecha >= DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())) AND idLista = @lista";
            return _ctx.Database.SqlQuery<ListaArticulo>(sql,
                new SqlParameter("lista", lista))
                .FirstOrDefault();            
        }

        public ArticuloWithIva GetArticuloWithIva(int lista, string articulo)
        {            
            var sql = @"select a.*, t.Piva AS iva from articu a INNER JOIN Tablaiva t ON t.IdTipoArt = a.XGrup_IdGrupoIva AND t.IdTipoPro = '05' " +
                    @"INNER JOIN ItemListaArticu li ON li.XItem_IdArticu = a.idArticu AND li.XItem_IdLista = @lista " +
                    @"WHERE a.idArticu = @articulo";
            return _ctx.Database.SqlQuery<ArticuloWithIva>(sql,
                    new SqlParameter("lista", lista),
                    new SqlParameter("articulo", articulo))
                    .FirstOrDefault();            
        }

        public IEnumerable<ListaArticulo> GetByIdGreaterThan(int lista)
        {            
            var sql = @"SELECT * FROM ListaArticu WHERE idLista > @lista";
            return _ctx.Database.SqlQuery<ListaArticulo>(sql,
                new SqlParameter("lista", lista))
                .ToList();         
        }

        public List<ListaArticulo> GetByFechaExceptList(int lista)
        {            
                var sql = @"SELECT * FROM ListaArticu WHERE fecha >= DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())) AND idLista <> @lista";
                return _ctx.Database.SqlQuery<ListaArticulo>(sql,
                    new SqlParameter("lista", lista))
                    .ToList();         
        }

        public List<ItemListaArticulo> GetArticulosByLista(int lista)
        {            
            var sql = @"SELECT * FROM ItemListaArticu WHERE XItem_IdLista = @lista GROUP BY XItem_IdLista, XItem_IdArticu";
            return _ctx.Database.SqlQuery<ItemListaArticulo>(sql,
                new SqlParameter("lista", lista))
                .ToList();         
        }

        public void Update(int lista)
        {            
            var sql = @"UPDATE ListaArticu SET fecha = DATEADD(dd, -1, DATEDIFF(dd, 0, GETDATE())) WHERE idLista = @lista";
            _ctx.Database.ExecuteSqlCommand(sql,
                new SqlParameter("lista", @lista));         
        }
    }
}
