using MySql.Data.MySqlClient;
using Sisfarma.Sincronizador.Fisiotes.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisfarma.Sincronizador.Fisiotes
{
    public class FisiotesService
    {
        //private string _server, _database;
        private readonly FisiotesContext _ctx;

        public static class FieldsConfiguracion
        {
            public const string FIELD_STOCK_ENTRADA = "fechaActualizacionStockEntrada";
            public const string FIELD_STOCK_SALIDA = "fechaActualizacionStockSalida";
            public const string FIELD_POR_DONDE_VOY_CON_STOCK = "porDondeVoyConStock";
            public const string FIELD_POR_DONDE_VOY_SIN_STOCK = "porDondeVoySinStock";
            public const string FIELD_POR_DONDE_VOY_BORRAR = "porDondeVoyBorrar";
            public const string FIELD_POR_DONDE_VOY_ENTREGAS_CLIENTES = "porDondeEntregasClientes";
        }

        public FisiotesService()
        {            
            _ctx = new FisiotesContext();
        }        

        public void SetCeroClientes()
        {            
            var sql = "UPDATE clientes SET dni_tra = 0";
            _ctx.Database.ExecuteSqlCommand(sql);            
        }

        public bool AnyClienteWithDni(string filter)
        {            
            var sql = @"SELECT * FROM clientes WHERE dni = @dni";
            return _ctx.Database.SqlQuery<Cliente>(sql,
                new MySqlParameter("dni", filter))
                .Any();            
        }

        public string GetDniLastClientAsync()
        {            
            var sql = @"SELECT dni FROM clientes WHERE dni_tra = 1";
            return _ctx.Database.SqlQuery<string>(sql)
                .FirstOrDefault() ?? "0";            
        }

        public void InsertClienteAsync(string trabajador, string tarjeta, string idCliente, string nombre,
            string telefono, string direccion, string movil, string email, decimal puntos, long fechaNacimiento,
            string sexo, string tipo, DateTime? fechaAlta, int baja, int lopd, bool withTrack = false)
        {
            try
            {                      
                var sql = string.Empty;
                if (withTrack)
                    sql = "INSERT IGNORE INTO clientes (" +
                            "dni_tra, nombre_tra, tarjeta, dni, apellidos, telefono, direccion, movil, email, puntos, fecha_nacimiento, sexo, tipo, fechaAlta, baja, lopd) " +
                            "VALUES(" +
                            "'1', @trabajador, @tarjeta, @idCliente, @nombre, @telefono, @direccion, @movil, @email, @puntos, @fechaNacimiento, @sexo, @tipo, @fechaAlta, @baja, @lopd)";
                else
                    sql = "INSERT IGNORE INTO clientes (" +
                            "nombre_tra, tarjeta, dni, apellidos, telefono, direccion, movil, email, puntos, fecha_nacimiento, sexo, tipo, fechaAlta, baja, lopd) " +
                            "VALUES(" +
                            "@trabajador, @tarjeta, @idCliente, @nombre, @telefono, @direccion, @movil, @email, @puntos, @fechaNacimiento, @sexo, @tipo, @fechaAlta, @baja, @lopd)";

                _ctx.Database.ExecuteSqlCommand(sql,
                        new MySqlParameter("trabajador", trabajador),
                        new MySqlParameter("tarjeta", tarjeta),
                        new MySqlParameter("idCliente", idCliente),
                        new MySqlParameter("nombre", nombre),
                        new MySqlParameter("telefono", telefono),
                        new MySqlParameter("direccion", direccion),
                        new MySqlParameter("movil", movil),
                        new MySqlParameter("email", email),
                        new MySqlParameter("puntos", puntos),
                        new MySqlParameter("fechaNacimiento", fechaNacimiento),
                        new MySqlParameter("sexo", sexo),
                        new MySqlParameter("tipo", tipo),
                        new MySqlParameter("fechaAlta", fechaAlta),
                        new MySqlParameter("baja", baja),
                        new MySqlParameter("lopd", lopd));                
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                throw e;
            }
        }

        public void UpdateClienteAsync(string trabajador, string tarjeta, string nombre, string telefono, string direccion,
            string movil, string email, decimal puntos, long fechaNacimiento, string sexo, DateTime? fechaAlta, int baja, int lopd, string idCliente,
            bool withTrack = false)
        {                 
            var sql = string.Empty;
            if (withTrack)
                sql = "UPDATE IGNORE clientes SET dni_tra = '1', nombre_tra = @trabajador, tarjeta = @tarjeta, apellidos = @nombre, " +
                        "telefono = @telefono, direccion = @direccion, movil = @movil, email = @email, puntos = @puntos, fecha_nacimiento = @fechaNacimiento, " +
                        "sexo = @sexo, fechaAlta = @fechaAlta, baja = @baja, lopd = @lopd " +
                        "WHERE dni = @idCliente";
            else
                sql = "UPDATE IGNORE clientes SET nombre_tra = @trabajador, tarjeta = @tarjeta, apellidos = @nombre, " +
                        "telefono = @telefono, direccion = @direccion, movil = @movil, email = @email, puntos = @puntos, fecha_nacimiento = @fechaNacimiento, " +
                        "sexo = @sexo, fechaAlta = @fechaAlta, baja = @baja, lopd = @lopd " +
                        "WHERE dni = @idCliente";

            _ctx.Database.ExecuteSqlCommand(sql,
                    new MySqlParameter("trabajador", trabajador),
                    new MySqlParameter("tarjeta", tarjeta),
                    new MySqlParameter("nombre", nombre),
                    new MySqlParameter("telefono", telefono),
                    new MySqlParameter("direccion", direccion),
                    new MySqlParameter("movil", movil),
                    new MySqlParameter("email", email),
                    new MySqlParameter("puntos", puntos),
                    new MySqlParameter("fechaNacimiento", fechaNacimiento),
                    new MySqlParameter("sexo", sexo),
                    new MySqlParameter("fechaAlta", fechaAlta),
                    new MySqlParameter("baja", baja),
                    new MySqlParameter("lopd", lopd),
                    new MySqlParameter("idCliente", idCliente));        
        }

        public void CheckAndCreateFieldsInClientes()
        {
            const string table = @"SELECT * from clientes LIMIT 0,1;";
            var fields = new[] { "baja", "fechaAlta" };
            var alters = new[]
            {
                @"ALTER TABLE clientes ADD `baja` tinyint(1) DEFAULT 0 AFTER dia_alta;",
                @"ALTER TABLE clientes ADD `fechaAlta` datetime AFTER dia_alta;"
            };

            CheckAndCreateFieldsTemplate(table, fields, alters);
        }

        private void CheckAndCreateFieldsTemplate(string sqlTable, string[] fields, string[] sqlAlter)
        {
            // Por defecto todos false
            var existsFields = new bool[fields.Length];

            using (var ctx = new FisiotesContext())
            {
                // Chekeamos si existen los campos
                var connection = ctx.Database.Connection;
                var sql = sqlTable;
                var command = connection.CreateCommand();
                command.CommandText = sql;
                connection.Open();
                var reader = command.ExecuteReader();
                var schemaTable = reader.GetSchemaTable();

                foreach (DataRow row in schemaTable.Rows)
                {
                    // Verifcamos los campos en el schema
                    for (var i = 0; i < fields.Length; i++)
                    {
                        if (row[schemaTable.Columns["ColumnName"]].ToString().Equals(fields[i]))
                            existsFields[i] = true;
                    }
                    if (existsFields.All(x => x))
                        break;
                }
                connection.Close();

                for (var i = 0; i < existsFields.Length; i++)
                {
                    if (!existsFields[i])
                        ctx.Database.ExecuteSqlCommand(sqlAlter[i]);
                }
            }
        }

        public void CreateClientesHuecos()
        {
            var sql = @"CREATE TABLE IF NOT EXISTS `clientes_huecos` (`hueco` varchar(255) DEFAULT NULL) ENGINE=MyISAM DEFAULT CHARSET=latin1;";
            _ctx.Database.ExecuteSqlCommand(sql);
        }

        public bool AnyHuecoDeCliente(int value)
        {                     
            var sql = @"SELECT * FROM clientes_huecos WHERE hueco = @value";
            return _ctx.Database.SqlQuery<string>(sql,
                new MySqlParameter("value", value))
                .Any();    
        }

        public void InsertHuecoDeCliente(string hueco)
        {                   
            var sql = "INSERT IGNORE INTO clientes_huecos (hueco) VALUES (@hueco)";
            _ctx.Database.ExecuteSqlCommand(sql,
                new MySqlParameter("hueco", hueco));         
        }

        public IEnumerable<string> GetHuecosDeClientesAscAsync()
        {
            var sql = @"SELECT hueco FROM clientes_huecos ORDER BY hueco ASC";
            return _ctx.Database.SqlQuery<string>(sql).ToList();
        }

        public void DeleteHuecoDeCliente(string hueco)
        {
            var sql = @"DELETE FROM clientes_huecos WHERE hueco = @hueco";
            _ctx.Database.ExecuteSqlCommand(sql,
                    new MySqlParameter("hueco", hueco));
        }

        public IEnumerable<PuntosPendientes> GetPuntosPendientesOfRecetas()
        {
            var sql = @"SELECT * FROM pendiente_puntos WHERE (recetaPendiente IS NULL OR recetaPendiente = 'D') " +
                        @"AND YEAR(fechaVenta) >= 2016 GROUP BY idventa, idnlinea ORDER BY idventa ASC LIMIT 0,1000";
            return _ctx.Database.SqlQuery<PuntosPendientes>(sql).ToList();
        }

        public void UpdatePuntosPendientes(string receta, long venta, long linea)
        {
            var sql = @"UPDATE IGNORE pendiente_puntos SET recetaPendiente = @receta WHERE IdVenta = @venta AND Idnlinea = @linea";
            _ctx.Database.ExecuteSqlCommand(sql,
                new MySqlParameter("receta", receta),
                new MySqlParameter("venta", venta),
                new MySqlParameter("linea", linea));
        }

        public void UpdatePuntosPendientes(long venta, long linea)
        {
            var sql = @"UPDATE IGNORE pendiente_puntos SET recetaPendiente = 'C' WHERE IdVenta = @venta AND Idnlinea = @linea";
            _ctx.Database.ExecuteSqlCommand(sql,
                new MySqlParameter("venta", venta),
                new MySqlParameter("linea", linea));
        }

        public Configuracion GetConfiguracionByCampo(string field)
        {            
            var sql = @"SELECT * FROM configuracion WHERE campo = @field";
            return _ctx.Database.SqlQuery<Configuracion>(sql,
                new MySqlParameter("field", field))
                .FirstOrDefault();            
        }

        public void InsertConfiguracion(string field)
        {            
            var sql = string.Empty;
            switch (field)
            {
                case FieldsConfiguracion.FIELD_STOCK_ENTRADA:
                case FieldsConfiguracion.FIELD_STOCK_SALIDA:
                    sql = @"INSERT IGNORE INTO configuracion (campo, valor) VALUES (@field, NULL)";
                    break;
                case FieldsConfiguracion.FIELD_POR_DONDE_VOY_CON_STOCK:
                case FieldsConfiguracion.FIELD_POR_DONDE_VOY_SIN_STOCK:
                case FieldsConfiguracion.FIELD_POR_DONDE_VOY_BORRAR:
                case FieldsConfiguracion.FIELD_POR_DONDE_VOY_ENTREGAS_CLIENTES:
                    sql = @"INSERT IGNORE INTO configuracion (campo, valor) VALUES (@field, '0')";
                    break;
            }
            _ctx.Database.ExecuteSqlCommand(sql,
                new MySqlParameter("field", field));         
        }

        public EntregaCliente GetEntregaCliente()
        {            
            var sql = @"SELECT * FROM entregas_clientes GROUP BY idventa ORDER BY idventa DESC LIMIT 0,1";
            return _ctx.Database.SqlQuery<EntregaCliente>(sql)
                .FirstOrDefault();            
        }

        public PuntosPendientes GetFirstPuntosPendientes()
        {
            var sql = @"SELECT * FROM pendiente_puntos ORDER BY idventa DESC LIMIT 0,1";
            return _ctx.Database.SqlQuery<PuntosPendientes>(sql)
                .FirstOrDefault();                
        }

        public void UpdateConfiguracionByCampo(string field, string value)
        {
            var sql = @"UPDATE IGNORE configuracion SET valor = @value WHERE campo = @field";
            _ctx.Database.ExecuteSqlCommand(sql,
                new MySqlParameter("value", value),
                new MySqlParameter("field", field));            
        }

        public EntregaCliente GetEntregaClienteByKey(int venta, int linea)
        {            
            var sql = @"SELECT * FROM entregas_clientes WHERE IdVenta = @venta AND Idnlinea = @linea";
            return _ctx.Database.SqlQuery<EntregaCliente>(sql,
                new MySqlParameter("venta", venta),
                new MySqlParameter("linea", linea))
                .FirstOrDefault();            
        }

        public void InsertEntregaCliente(int venta, int linea, string codigo, string descripcion, int cantidad, decimal numero, string tipoLinea, int fecha,
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

        public bool ExistFieldWebInMedicamentos()
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

        public List<string> GetCodigosNacionalesFromMedicamentos(string codigo, bool withWeb = false)
        {            
            var sql = withWeb
                ? @"SELECT cod_nacional FROM medicamentos WHERE web = 0 AND cod_nacional >= @codigo ORDER BY cod_nacional ASC LIMIT 0,1000"
                : @"SELECT cod_nacional FROM medicamentos WHERE cod_nacional >= @codigo ORDER BY cod_nacional ASC LIMIT 0,1000";
            return _ctx.Database.SqlQuery<string>(sql,
                new MySqlParameter("codigo", codigo))
                .ToList();         
        }

        public void DeleteMedicamentoByCodigoNacional(string codigo)
        {            
            var sql = @"DELETE FROM medicamentos WHERE cod_nacional = @codigo";
            _ctx.Database.ExecuteSqlCommand(sql, new MySqlParameter("codigo", codigo));         
        }

        public List<PuntosPendientes> GetPuntosPendientesWithoutRedencion()
        {            
            var sql =
                @"SELECT * FROM pendiente_puntos WHERE redencion IS NULL AND YEAR(fechaVenta) >= 2015 GROUP BY idventa ORDER BY idventa ASC LIMIT 0,1000";
            return _ctx.Database.SqlQuery<PuntosPendientes>(sql)
                .ToList();         
        }

        public void UpdatePuntosPendientes(string tipoPago, string proveedor, float? dtoLinea, float? dtoVenta, float redencion, long venta, long linea)
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

        public void UpdatePuntosPendientes(long venta)
        {            
            var sql = @"UPDATE IGNORE pendiente_puntos SET tipoPago = 'C', redencion = 0 WHERE IdVenta = @venta";
            _ctx.Database.ExecuteSqlCommand(sql,
                new MySqlParameter("venta", venta));         
        }

        public void CheckAndAlterFieldsInSinonimos(string remote)
        {            
            var sql = @"SELECT data_type AS tipo From information_schema.Columns WHERE TABLE_SCHEMA = @baseRemoto AND TABLE_NAME = 'sinonimos' AND COLUMN_NAME = 'cod_barras'";
            var type = _ctx.Database.SqlQuery<string>(sql,
                new MySqlParameter("baseRemoto", remote))
                .FirstOrDefault();
            if (type != null)
            {
                if (type.Equals("varchar", StringComparison.CurrentCultureIgnoreCase))
                {
                    sql = @"ALTER TABLE sinonimos MODIFY COLUMN cod_nacional VARCHAR(255);";
                    _ctx.Database.ExecuteSqlCommand(sql);

                    sql = @"ALTER TABLE sinonimos MODIFY COLUMN cod_barras VARCHAR(255);";
                    _ctx.Database.ExecuteSqlCommand(sql);
                }
            }         
        }

        public Sinonimo GetSinomimoSingle()
        {
            var sql = @"SELECT * FROM sinonimos LIMIT 0,1";
            return _ctx.Database.SqlQuery<Sinonimo>(sql)
                .FirstOrDefault();                        
        }

        public void TruncateSinonimos()
        {            
            var sql = @"TRUNCATE sinonimos";
            _ctx.Database.ExecuteSqlCommand(sql);         
        }

        public void InsertSinonimosPerBatch(List<Sinonimo> items)
        {
            var sql = @"INSERT IGNORE INTO sinonimos (cod_barras,cod_nacional) VALUES ";
            foreach (var item in items)
            {
                sql += $@"('{item.cod_barras}', '{item.cod_nacional}'),";
            }
            sql = sql.TrimEnd(',');
            _ctx.Database.ExecuteSqlCommand(sql);            
        }

        public Medicamento GetMedicamentoByCodNacional(string codNacional)
        {            
            var sql = @"select * from medicamentos where cod_nacional = @codNacional";
            return _ctx.Database.SqlQuery<Medicamento>(sql,
                new MySqlParameter("codNacional", codNacional))
                .FirstOrDefault();            
        }

        public void InsertMedicamento(string codigoBarras, string codNacional, string nombre, string superFamilia, string familia,
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

        public void InsertMedicamento(string codigoBarras, string codNacional, string nombre, string superFamilia, string familia,
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

        public void UpdateMedicamento(string codigoBarras, string nombre, string superFamilia, string familia,
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

        public void UpdateMedicamento(string codigoBarras, string nombre, string superFamilia, string familia,
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

        public void UpdateMedicamentoPorDondeVoySinStock()
        {            
            var sql = @"UPDATE IGNORE medicamentos SET porDondeVoySinStock = 0";
            _ctx.Database.ExecuteSqlCommand(sql);            
        }

        public void UpdateMedicamentoPorDondeVoy()
        {        
            var sql = @"UPDATE IGNORE medicamentos SET porDondeVoy = 0";
            _ctx.Database.ExecuteSqlCommand(sql);         
        }

        public void CheckAndCreateFieldsInMedicamentos()
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
