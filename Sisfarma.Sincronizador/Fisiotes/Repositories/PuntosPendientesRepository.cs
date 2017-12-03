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

        public void CheckAndCreateFields()
        {
            const string table = @"SELECT * from pendiente_puntos LIMIT 0,1;";
            var fields = new[] { "dtoLinea", "dtoVenta", "proveedor", "redencion", "recetaPendiente" };
            var alters = new[]
            {
                @"ALTER TABLE pendiente_puntos ADD dtoLinea FLOAT DEFAULT 0;",
                @"ALTER TABLE pendiente_puntos ADD dtoVenta FLOAT DEFAULT 0;",
                @"ALTER TABLE pendiente_puntos ADD proveedor VARCHAR(255) DEFAULT NULL AFTER laboratorio;",
                @"ALTER TABLE pendiente_puntos ADD redencion FLOAT DEFAULT NULL;",
                @"ALTER TABLE pendiente_puntos ADD recetaPendiente CHAR(2) DEFAULT NULL;"
            };
            CheckAndCreateFieldsTemplate(table, fields, alters);
        }

        public void CheckTipoPagoField(string remote)
        {            
            var sql =
                @"SELECT data_type AS tipo From information_schema.Columns WHERE TABLE_SCHEMA = @baseRemoto AND TABLE_NAME = 'pendiente_puntos' AND COLUMN_NAME = 'tipoPago'";
            var result = _ctx.Database.SqlQuery<string>(sql,
                new MySqlParameter("baseRemoto", remote))
                .ToList();
            if (result.Count != 0)
            {
                if (result[0].Equals("char", StringComparison.CurrentCultureIgnoreCase))
                {
                    sql = @"ALTER TABLE pendiente_puntos MODIFY COLUMN tipoPago CHAR(2);";
                    _ctx.Database.ExecuteSqlCommand(sql);
                }
            }
            else
            {
                sql = @"ALTER TABLE pendiente_puntos ADD tipoPago CHAR(2) DEFAULT NULL AFTER precio;";
                _ctx.Database.ExecuteSqlCommand(sql);
            }            
        }

        public PuntosPendientes GetByItemVenta(int venta, int linea)
        {            
            var sql = @"SELECT * FROM pendiente_puntos WHERE IdVenta = @venta AND Idnlinea = @linea";
            return _ctx.Database.SqlQuery<PuntosPendientes>(sql,
                new MySqlParameter("venta", venta),
                new MySqlParameter("linea", linea))
                .FirstOrDefault();         
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

        public void Insert(int venta, int linea, string codigoBarra, string codigo, string descripcion, string familia, int cantidad, decimal numero,
                string tipoPago, int fecha, string dni, string cargado, string puesto, string trabajador, string codLaboratorio, string laboratorio, string proveedor,
                string receta, DateTime fechaVenta, string superFamlia, float precioMed, float pcoste, float dtoLinea, float dtoVta, float redencion, string recetaPendiente)
        {            
            var sql = "INSERT IGNORE INTO pendiente_puntos " +
                    @"(idventa,idnlinea,cod_barras,cod_nacional,descripcion,familia,cantidad,precio,tipoPago,fecha,dni,cargado,puesto,trabajador," +
                    @"cod_laboratorio,laboratorio,proveedor,receta,fechaVenta,superFamilia, pvp, puc, dtoLinea, dtoVenta, redencion, recetaPendiente) " +
                    @"VALUES(" +
                    @"@venta, @linea, @codigoBarra, @codigo, @descripcion, @familia, @cantidad, @numero, @tipoPago, @fecha, @dni, @cargado, @puesto, @trabajador, " +
                    @"@codLaboratorio, @laboratorio, @proveedor, @receta, @fechaVenta, @superfamilia, @precioMed, @pcoste, @dtoLinea, @dtoVta, @redencion, @recetaPendiente)";
            _ctx.Database.ExecuteSqlCommand(sql,
                new MySqlParameter("venta", venta),
                new MySqlParameter("linea", linea),
                new MySqlParameter("codigoBarra", codigoBarra),
                new MySqlParameter("codigo", codigo),
                new MySqlParameter("descripcion", descripcion),
                new MySqlParameter("familia", familia),
                new MySqlParameter("cantidad", cantidad),
                new MySqlParameter("numero", numero),
                new MySqlParameter("tipoPago", tipoPago),
                new MySqlParameter("fecha", fecha),
                new MySqlParameter("dni", dni),
                new MySqlParameter("cargado", cargado),
                new MySqlParameter("puesto", puesto),
                new MySqlParameter("trabajador", trabajador),
                new MySqlParameter("codLaboratorio", codLaboratorio),
                new MySqlParameter("laboratorio", laboratorio),
                new MySqlParameter("proveedor", proveedor),
                new MySqlParameter("receta", receta),
                new MySqlParameter("fechaVenta", fechaVenta),
                new MySqlParameter("superFamilia", superFamlia),
                new MySqlParameter("precioMed", precioMed),
                new MySqlParameter("pcoste", pcoste),
                new MySqlParameter("dtoLinea", dtoLinea),
                new MySqlParameter("dtoVta", dtoVta),
                new MySqlParameter("redencion", redencion),
                new MySqlParameter("recetaPendiente", recetaPendiente));            
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
