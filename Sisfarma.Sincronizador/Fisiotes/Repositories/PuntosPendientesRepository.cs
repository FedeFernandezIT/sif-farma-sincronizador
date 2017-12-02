using MySql.Data.MySqlClient;
using Sisfarma.Sincronizador.Fisiotes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisfarma.Sincronizador.Fisiotes.Repositories
{
    public class PuntosPendientesRepository : FisiotesRepository
    {
        public PuntosPendientesRepository(FisiotesContext ctx) : base(ctx)
        {
        }

        public IEnumerable<PuntosPendientes> GetOfRecetasPendientes()
        {
            var sql = @"SELECT * FROM pendiente_puntos WHERE (recetaPendiente IS NULL OR recetaPendiente = 'D') " +
                        @"AND YEAR(fechaVenta) >= 2016 GROUP BY idventa, idnlinea ORDER BY idventa ASC LIMIT 0,1000";
            return _ctx.Database.SqlQuery<PuntosPendientes>(sql).ToList();
        }

        public IEnumerable<PuntosPendientes> GetWithoutRedencion()
        {
            var sql =
                @"SELECT * FROM pendiente_puntos WHERE redencion IS NULL AND YEAR(fechaVenta) >= 2015 GROUP BY idventa ORDER BY idventa ASC LIMIT 0,1000";
            return _ctx.Database.SqlQuery<PuntosPendientes>(sql)
                .ToList();
        }

        public PuntosPendientes Last()
        {
            var sql = @"SELECT * FROM pendiente_puntos ORDER BY idventa DESC LIMIT 0,1";
            return _ctx.Database.SqlQuery<PuntosPendientes>(sql)
                .FirstOrDefault();
        }

        public void Update(long venta)
        {
            var sql = @"UPDATE IGNORE pendiente_puntos SET tipoPago = 'C', redencion = 0 WHERE IdVenta = @venta";
            _ctx.Database.ExecuteSqlCommand(sql,
                new MySqlParameter("venta", venta));
        }

        public void Update(string receta, long venta, long linea)
        {
            var sql = @"UPDATE IGNORE pendiente_puntos SET recetaPendiente = @receta WHERE IdVenta = @venta AND Idnlinea = @linea";
            _ctx.Database.ExecuteSqlCommand(sql,
                new MySqlParameter("receta", receta),
                new MySqlParameter("venta", venta),
                new MySqlParameter("linea", linea));
        }

        public void Update(long venta, long linea)
        {
            var sql = @"UPDATE IGNORE pendiente_puntos SET recetaPendiente = 'C' WHERE IdVenta = @venta AND Idnlinea = @linea";
            _ctx.Database.ExecuteSqlCommand(sql,
                new MySqlParameter("venta", venta),
                new MySqlParameter("linea", linea));
        }        

        public void Update(string tipoPago, string proveedor, float? dtoLinea, float? dtoVenta, float redencion, long venta, long linea)
        {
            var sql =
                @"UPDATE IGNORE pendiente_puntos SET tipoPago = @tipoPago, proveedor = @proveedor, dtoLinea = @dtoLinea, dtoVenta = @dtoVenta, redencion = @redencion " +
                @"WHERE IdVenta = @venta AND Idnlinea= @linea";
            _ctx.Database.ExecuteSqlCommand(sql,
                new MySqlParameter("tipoPago", tipoPago),
                new MySqlParameter("proveedor", proveedor),
                new MySqlParameter("dtoLinea", dtoLinea),
                new MySqlParameter("dtoVenta", dtoVenta),
                new MySqlParameter("redencion", redencion),
                new MySqlParameter("venta", venta),
                new MySqlParameter("linea", linea));
        }
        
    }
}
