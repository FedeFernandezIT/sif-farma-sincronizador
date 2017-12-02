using MySql.Data.MySqlClient;
using Sisfarma.Sincronizador.Fisiotes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisfarma.Sincronizador.Fisiotes.Repositories
{
    public class EntregasRepository : FisiotesRepository
    {
        public EntregasRepository(FisiotesContext ctx) : base(ctx)
        {
        }

        public EntregaCliente Last()
        {
            var sql = @"SELECT * FROM entregas_clientes GROUP BY idventa ORDER BY idventa DESC LIMIT 0,1";
            return _ctx.Database.SqlQuery<EntregaCliente>(sql)
                .FirstOrDefault();
        }
        
        public EntregaCliente GetByKey(int venta, int linea)
        {
            var sql = @"SELECT * FROM entregas_clientes WHERE IdVenta = @venta AND Idnlinea = @linea";
            return _ctx.Database.SqlQuery<EntregaCliente>(sql,
                new MySqlParameter("venta", venta),
                new MySqlParameter("linea", linea))
                .FirstOrDefault();
        }

        public void Insert(int venta, int linea, string codigo, string descripcion, int cantidad, decimal numero, string tipoLinea, int fecha,
                string dni, string puesto, string trabajador, DateTime fechaVenta, float? pvp)
        {
            var sql =
                @"INSERT INTO IGNORE entregas_clientes (idventa,idnlinea,codigo,descripcion,cantidad,precio,tipo,fecha,dni,puesto,trabajador,fechaEntrega,pvp) VALUES(" +
                @"@venta, @linea, @codigo, @descripcion, @cantidad, @numero, @tipoLinea, @fecha, @dni, @puesto, @trabajador, @fechaVenta, @pvp)";
            _ctx.Database.ExecuteSqlCommand(sql,
                new MySqlParameter("venta", venta),
                new MySqlParameter("linea", linea),
                new MySqlParameter("codigo", codigo),
                new MySqlParameter("descripcion", descripcion),
                new MySqlParameter("cantidad", cantidad),
                new MySqlParameter("numero", numero),
                new MySqlParameter("tipoLinea", tipoLinea),
                new MySqlParameter("fecha", fecha),
                new MySqlParameter("dni", dni),
                new MySqlParameter("puesto", puesto),
                new MySqlParameter("trabajador", trabajador),
                new MySqlParameter("fechaVenta", fechaVenta),
                new MySqlParameter("pvp", pvp));
        }
    }
}
