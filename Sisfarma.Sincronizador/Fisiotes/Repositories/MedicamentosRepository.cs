using MySql.Data.MySqlClient;
using Sisfarma.Sincronizador.Fisiotes.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisfarma.Sincronizador.Fisiotes.Repositories
{
    public class MedicamentosRepository : FisiotesRepository
    {
        public MedicamentosRepository(FisiotesContext ctx) : base(ctx)
        {
        }

        public bool HasWebField()
        {
            var existField = false;

            using (var ctx = new FisiotesContext())
            {
                // Chekeamos si existen los campos
                var connection = ctx.Database.Connection;
                var sql = @"SELECT * from medicamentos LIMIT 0,1;";
                var command = connection.CreateCommand();
                command.CommandText = sql;
                connection.Open();
                var reader = command.ExecuteReader();
                var schemaTable = reader.GetSchemaTable();

                foreach (DataRow row in schemaTable.Rows)
                {
                    if (row[schemaTable.Columns["ColumnName"]].ToString()
                        .Equals("web", StringComparison.CurrentCultureIgnoreCase))
                    {
                        existField = true;
                        break;
                    }
                }
                connection.Close();
            }
            return existField;
        }

        public List<string> GetCodigosNacionalesGreaterOrEqual(string codigo, bool withWeb = false)
        {
            var sql = withWeb
                ? @"SELECT cod_nacional FROM medicamentos WHERE web = 0 AND cod_nacional >= @codigo ORDER BY cod_nacional ASC LIMIT 0,1000"
                : @"SELECT cod_nacional FROM medicamentos WHERE cod_nacional >= @codigo ORDER BY cod_nacional ASC LIMIT 0,1000";
            return _ctx.Database.SqlQuery<string>(sql,
                new MySqlParameter("codigo", codigo))
                .ToList();
        }

        public void DeleteByCodigoNacional(string codigo)
        {
            var sql = @"DELETE FROM medicamentos WHERE cod_nacional = @codigo";
            _ctx.Database.ExecuteSqlCommand(sql, new MySqlParameter("codigo", codigo));
        }

        public Medicamento GetByCodNacional(string codNacional)
        {
            var sql = @"select * from medicamentos where cod_nacional = @codNacional";
            return _ctx.Database.SqlQuery<Medicamento>(sql,
                new MySqlParameter("codNacional", codNacional))
                .FirstOrDefault();
        }

        public void Insert(string codigoBarras, string codNacional, string nombre, string superFamilia, string familia,
            float precio, string descripcion, string laboratorio, string nombreLaboratorio, string proveedor, float pvpSinIva, int iva,
            int stock, float puc, int stockMinimo, int stockMaximo, string presentacion, string descripcionTienda, bool activo, DateTime? caducidad,
            DateTime? ultimaCompra, DateTime? ultimaVenta, bool baja)
        {
            var sql = @"INSERT INTO medicamentos (cod_barras, cod_nacional, nombre, superFamilia, familia,precio, descripcion, laboratorio, nombre_laboratorio, " +
                    @"proveedor, pvpSinIva, iva,stock, puc, stockMinimo, stockMaximo, presentacion, descripcionTienda, activoPrestashop, actualizadoPS, " +
                    @"fechaCaducidad, fechaUltimaCompra, fechaUltimaVenta,baja) " +
                @"VALUES(@codigoBarras, @codNacional, @nombre, @superFamilia, @familia, @precio, @descripcion, @laboratorio, @nombreLaboratorio, " +
                    @"@proveedor, @pvpSinIva, @iva, @stock, @puc, @stockMinimo, @stockMaximo, @presentacion, @descripcionTienda, @activo, 1, " +
                    @"@caducidad, @ultimaCompra, @ultimaVenta, @baja)";
            _ctx.Database.ExecuteSqlCommand(sql,
                new MySqlParameter("codigoBarras", codigoBarras),
                new MySqlParameter("codNacional", codNacional),
                new MySqlParameter("nombre", nombre),
                new MySqlParameter("superFamilia", superFamilia),
                new MySqlParameter("familia", familia),
                new MySqlParameter("precio", precio),
                new MySqlParameter("descripcion", descripcion),
                new MySqlParameter("laboratorio", laboratorio),
                new MySqlParameter("nombreLaboratorio", nombreLaboratorio),
                new MySqlParameter("proveedor", proveedor),
                new MySqlParameter("iva", iva),
                new MySqlParameter("pvpSinIva", pvpSinIva),
                new MySqlParameter("stock", stock),
                new MySqlParameter("puc", puc),
                new MySqlParameter("stockMinimo", stockMinimo),
                new MySqlParameter("stockMaximo", stockMaximo),
                new MySqlParameter("presentacion", presentacion),
                new MySqlParameter("descripcionTienda", descripcionTienda),
                new MySqlParameter("activo", activo),
                new MySqlParameter("caducidad", caducidad),
                new MySqlParameter("ultimaCompra", ultimaCompra),
                new MySqlParameter("ultimaVenta", ultimaVenta),
                new MySqlParameter("baja", baja));
        }

        public void Insert(string codigoBarras, string codNacional, string nombre, string superFamilia, string familia,
            float precio, string descripcion, string laboratorio, string nombreLaboratorio, string proveedor, float pvpSinIva, int iva,
            int stock, float puc, int stockMinimo, int stockMaximo, string presentacion, string descripcionTienda, bool activo, bool baja)
        {
            var sql = @"INSERT IGNORE INTO medicamentos (cod_barras, cod_nacional, nombre, superFamilia, familia,precio, descripcion, laboratorio, nombre_laboratorio, " +
                    @"proveedor, pvpSinIva, iva,stock, puc, stockMinimo, stockMaximo, presentacion, descripcionTienda, activoPrestashop, actualizadoPS, " +
                    @"baja) " +
                @"VALUES(@codigoBarras, @codNacional, @nombre, @superFamilia, @familia, @precio, @descripcion, @laboratorio, @nombreLaboratorio, " +
                    @"@proveedor, @pvpSinIva, @iva, @stock, @puc, @stockMinimo, @stockMaximo, @presentacion, @descripcionTienda, @activo, 1, " +
                    @"@baja)";
            _ctx.Database.ExecuteSqlCommand(sql,
                new MySqlParameter("codigoBarras", codigoBarras),
                new MySqlParameter("codNacional", codNacional),
                new MySqlParameter("nombre", nombre),
                new MySqlParameter("superFamilia", superFamilia),
                new MySqlParameter("familia", familia),
                new MySqlParameter("precio", precio),
                new MySqlParameter("descripcion", descripcion),
                new MySqlParameter("laboratorio", laboratorio),
                new MySqlParameter("nombreLaboratorio", nombreLaboratorio),
                new MySqlParameter("proveedor", proveedor),
                new MySqlParameter("iva", iva),
                new MySqlParameter("pvpSinIva", pvpSinIva),
                new MySqlParameter("stock", stock),
                new MySqlParameter("puc", puc),
                new MySqlParameter("stockMinimo", stockMinimo),
                new MySqlParameter("stockMaximo", stockMaximo),
                new MySqlParameter("presentacion", presentacion),
                new MySqlParameter("descripcionTienda", descripcionTienda),
                new MySqlParameter("activo", activo),
                new MySqlParameter("baja", baja));
        }

        public void Update(string codigoBarras, string nombre, string superFamilia, string familia,
            float precio, string descripcion, string laboratorio, string nombreLaboratorio, string proveedor,
            int iva, float pvpSinIva, int stock, float puc, int stockMinimo, int stockMaximo, string presentacion,
            string descripcionTienda, bool activo, DateTime? caducidad, DateTime? ultimaCompra, DateTime? ultimaVenta,
            bool baja, string codNacional, bool withSqlExtra = false)
        {
            var sqlExtra = withSqlExtra ? string.Empty : " cargadoPS = 0, actualizadoPS = 1, ";
            var sql = @"UPDATE IGNORE medicamentos SET cod_barras = @codigoBarras, nombre = @nombre, superFamilia = @superFamilia, familia = @familia, " +
                    @"precio = @precio, descripcion = @descripcion, laboratorio = @laboratorio, nombre_laboratorio = @nombreLaboratorio, proveedor = @proveedor," +
                    @"iva = @iva, pvpSinIva = @pvpSinIva, stock = @stock, puc = @puc, stockMinimo = @stockMinimo, stockMaximo = @stockMaximo, " +
                    @"presentacion = @presentacion, descripcionTienda = @descripcionTienda, " + sqlExtra +
                    @" activoPrestashop = @activo, fechaCaducidad = @caducidad, fechaUltimaCompra = @ultimaCompra, fechaUltimaVenta = @ultimaVenta, " +
                    @"baja = @baja WHERE cod_nacional = @codNacional";
            _ctx.Database.ExecuteSqlCommand(sql,
                new MySqlParameter("codigoBarras", codigoBarras),
                new MySqlParameter("nombre", nombre),
                new MySqlParameter("superFamilia", superFamilia),
                new MySqlParameter("familia", familia),
                new MySqlParameter("precio", precio),
                new MySqlParameter("descripcion", descripcion),
                new MySqlParameter("laboratorio", laboratorio),
                new MySqlParameter("nombreLaboratorio", nombreLaboratorio),
                new MySqlParameter("proveedor", proveedor),
                new MySqlParameter("iva", iva),
                new MySqlParameter("pvpSinIva", pvpSinIva),
                new MySqlParameter("stock", stock),
                new MySqlParameter("puc", puc),
                new MySqlParameter("stockMinimo", stockMinimo),
                new MySqlParameter("stockMaximo", stockMaximo),
                new MySqlParameter("presentacion", presentacion),
                new MySqlParameter("descripcionTienda", descripcionTienda),
                new MySqlParameter("activo", activo),
                new MySqlParameter("caducidad", caducidad),
                new MySqlParameter("ultimaCompra", ultimaCompra),
                new MySqlParameter("ultimaVenta", ultimaVenta),
                new MySqlParameter("baja", baja),
                new MySqlParameter("codNacional", codNacional));
        }

        public void Update(string codigoBarras, string nombre, string superFamilia, string familia,
            float precio, string descripcion, string laboratorio, string nombreLaboratorio, string proveedor,
            int iva, float pvpSinIva, int stock, float puc, int stockMinimo, int stockMaximo, string presentacion,
            string descripcionTienda, bool activo, bool baja, string codNacional)
        {
            var sql = @"UPDATE IGNORE medicamentos SET cod_barras = @codigoBarras, nombre = @nombre, superFamilia = @superFamilia, familia = @familia, " +
                @"precio = @precio, descripcion = @descripcion, laboratorio = @laboratorio, nombre_laboratorio = @nombreLaboratorio, proveedor = @proveedor," +
                @"iva = @iva, pvpSinIva = @pvpSinIva, stock = @stock, puc = @puc, stockMinimo = @stockMinimo, stockMaximo = @stockMaximo, " +
                @"presentacion = @presentacion, descripcionTienda = @descripcionTienda, cargadoPS = 0, actualizadoPS = 1, " +
                @" activoPrestashop = @activo, baja = @baja WHERE cod_nacional = @codNacional";
            _ctx.Database.ExecuteSqlCommand(sql,
                new MySqlParameter("codigoBarras", codigoBarras),
                new MySqlParameter("nombre", nombre),
                new MySqlParameter("superFamilia", superFamilia),
                new MySqlParameter("familia", familia),
                new MySqlParameter("precio", precio),
                new MySqlParameter("descripcion", descripcion),
                new MySqlParameter("laboratorio", laboratorio),
                new MySqlParameter("nombreLaboratorio", nombreLaboratorio),
                new MySqlParameter("proveedor", proveedor),
                new MySqlParameter("iva", iva),
                new MySqlParameter("pvpSinIva", pvpSinIva),
                new MySqlParameter("stock", stock),
                new MySqlParameter("puc", puc),
                new MySqlParameter("stockMinimo", stockMinimo),
                new MySqlParameter("stockMaximo", stockMaximo),
                new MySqlParameter("presentacion", presentacion),
                new MySqlParameter("descripcionTienda", descripcionTienda),
                new MySqlParameter("activo", activo),
                new MySqlParameter("baja", baja),
                new MySqlParameter("codNacional", codNacional));
        }

        public void ResetPorDondeVoySinStock()
        {
            var sql = @"UPDATE IGNORE medicamentos SET porDondeVoySinStock = 0";
            _ctx.Database.ExecuteSqlCommand(sql);
        }

        public void ResetPorDondeVoy()
        {
            var sql = @"UPDATE IGNORE medicamentos SET porDondeVoy = 0";
            _ctx.Database.ExecuteSqlCommand(sql);
        }

        public void CheckAndCreateFields()
        {
            const string table = @"SELECT * from medicamentos LIMIT 0,1;";
            var fields = new[]
            {
                "laboratorio", "stock", "puc", "stockMinimo", "presentacion", "fechaCaducidad",
                "porDondeVoySinStock", "fechaUltimaCompra", "proveedor", "superFamilia"
            };
            var alters = new[]
            {
                @"ALTER TABLE medicamentos ADD laboratorio VARCHAR(255);",
                @"ALTER TABLE medicamentos ADD (pvpSinIva float, iva int (11), stock int (11));",
                @"ALTER TABLE medicamentos ADD (puc float);",
                @"ALTER TABLE medicamentos ADD (stockMinimo int (11), stockMaximo int (11));",
                @"ALTER TABLE medicamentos ADD `nombre_laboratorio` varchar(255) DEFAULT NULL AFTER laboratorio, " +
                    @"ADD (`presentacion` varchar(50) DEFAULT NULL, `descripcionTienda` text, `prestashopIdPS` int(10) DEFAULT NULL, " +
                    @"`cargadoPS` tinyint(1) DEFAULT '0', `fechaCargadoPS` datetime DEFAULT NULL, `activoPrestashop` tinyint(1) DEFAULT '1', " +
                    @"`actualizadoPS` tinyint(1) DEFAULT '0', `eliminado` tinyint(1) DEFAULT '0', `fechaEliminado` datetime DEFAULT NULL);",
                @"ALTER TABLE medicamentos ADD (fechaCaducidad datetime, porDondeVoy TINYINT(1) DEFAULT 0);",
                @"ALTER TABLE medicamentos ADD (porDondeVoySinStock TINYINT(1) DEFAULT 0);",
                @"ALTER TABLE medicamentos ADD (fechaUltimaCompra DATETIME DEFAULT NULL, fechaUltimaVenta DATETIME DEFAULT NULL);",
                @"ALTER TABLE medicamentos ADD proveedor VARCHAR(255) DEFAULT NULL AFTER nombre_laboratorio, " +
                    @"ADD (baja TINYINT(1) DEFAULT 0);",
                @"ALTER TABLE medicamentos ADD superFamilia VARCHAR(255) DEFAULT NULL AFTER nombre;"

            };
            CheckAndCreateFieldsTemplate(table, fields, alters);
        }
    }
}
