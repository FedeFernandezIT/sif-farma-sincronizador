using Sisfarma.Sincronizador.Farmatic.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisfarma.Sincronizador.Farmatic.Repositories
{
    public class PedidosRepository : FarmaticRepository
    {
        public PedidosRepository(FarmaticContext ctx) : base(ctx)
        {
        }

        public IEnumerable<Pedido> GetByIdGreaterOrEqual(long? pedido)
        {            
            var sql = @"SELECT * From pedido WHERE IdPedido >= @pedido Order by IdPedido ASC";
            return _ctx.Database.SqlQuery<Pedido>(sql,
                new SqlParameter("pedido", pedido ?? SqlInt64.Null))
                .ToList();            
        }

        public IEnumerable<Pedido> GetByFechaGreaterOrEqual(DateTime fecha)
        {            
            var sql = @"SELECT * From pedido WHERE Fecha >= @fecha Order by IdPedido ASC";
            return _ctx.Database.SqlQuery<Pedido>(sql,
                new SqlParameter("fecha", fecha))
                .ToList();         
        }

        public IEnumerable<LineaPedido> GetLineasByPedido(int pedido)
        {            
            var sql = @"select * from lineaPedido where IdPedido = @pedido";
            return _ctx.Database.SqlQuery<LineaPedido>(sql,
                new SqlParameter("pedido", pedido))
                .ToList();         
        }
    }
}
