using Sisfarma.Sincronizador.Farmatic.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisfarma.Sincronizador.Farmatic.Repositories
{
    public class VentasRepository : FarmaticRepository
    {
        public VentasRepository(FarmaticContext ctx) : base(ctx)
        {
        }

        public LineaVenta GetLineaVentaByKey(long venta, long linea)
        {
            var sql = @"SELECT * FROM lineaventa WHERE IdVenta = @venta AND IdNLinea = @linea";
            return _ctx.Database.SqlQuery<LineaVenta>(sql,
                new SqlParameter("venta", venta),
                new SqlParameter("linea", linea))
                .FirstOrDefault();
        }

        public List<Venta> GetByIdGreaterOrEqual(long value)
        {            
            var sql = @"SELECT TOP 1000 * FROM venta WHERE ejercicio >= 1 AND IdVenta >= @value ORDER BY IdVenta ASC";
            return _ctx.Database.SqlQuery<Venta>(sql,
                new SqlParameter("value", value))
                .ToList();            
        }

        public List<Venta> GetVirtualesLessThanId(long venta)
        {
            var sql = @"SELECT v.* FROM venta v INNER JOIN lineaventavirtual lvv ON lvv.idventa = v.idventa AND (lvv.codigo = 'Pago' OR lvv.codigo = 'A Cuenta') " +
                    @"WHERE v.ejercicio >= 2015 AND v.IdVenta < @venta ORDER BY v.IdVenta DESC";
            return _ctx.Database.SqlQuery<Venta>(sql,
                new SqlParameter("venta", venta))
                .ToList();
        }

        public List<LineaVentaVirtual> GetLineasVirtualesByVenta(int venta)
        {
            var sql =
                @"SELECT * FROM lineaventavirtual WHERE IdVenta = @venta AND (codigo = 'Pago' OR codigo = 'A Cuenta')";
            return _ctx.Database.SqlQuery<LineaVentaVirtual>(sql,
                new SqlParameter("venta", venta))
                .ToList();
        }

        public Venta GetById(long venta)
        {
            var sql = @"SELECT * FROM venta WHERE IdVenta = @venta ORDER BY IdVenta ASC";
            return _ctx.Database.SqlQuery<Venta>(sql,
                new SqlParameter("venta", venta))
                .FirstOrDefault();
        }

        public List<LineaVenta> GetLineasVentaByVenta(int venta)
        {
            var sql = @"SELECT * FROM lineaventa WHERE IdVenta = @idVenta";
            return _ctx.Database.SqlQuery<LineaVenta>(sql,
                new SqlParameter("idVenta", venta))
                .ToList();
        }

        public LineaVentaRedencion GetLineaRedencionByKey(int venta, int linea)
        {
            var sql = @"SELECT * FROM LineaVentaReden WHERE IdVenta = @venta AND IdNLinea = @linea";
            return _ctx.Database.SqlQuery<LineaVentaRedencion>(sql,
                    new SqlParameter("venta", venta),
                    new SqlParameter("linea", linea))
                .FirstOrDefault();
        }
    }
}
