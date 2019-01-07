using MySql.Data.MySqlClient;
using Sisfarma.RestClient;
using Sisfarma.Sincronizador.Fisiotes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisfarma.Sincronizador.Fisiotes.Repositories
{
    public class ClientesRepository : FisiotesRepository
    {
        public ClientesRepository(FisiotesContext ctx) : base(ctx)
        {
        }

        public ClientesRepository(IRestClient restClient, FisiotesConfig config) : base(restClient, config)
        {
        }

        public string GetDniTrackingLast()
        {
            return _restClient
                .Resource(_config.Clientes.GetDniTrackingLast)
                .SendGet<Cliente>().dni;
        }

        public void ResetDniTracking()
        {
            _restClient
                .Resource(_config.Clientes.ResetDniTracking)
                .SendPut();
        }

        public bool AnyWithDni(string dni)
        {
            var cliente = _restClient
                .Resource(_config.Clientes.GetByDni.Replace("{dni}", $"{dni}"))
                .SendGet<Cliente>();
            return cliente != null;
        }

        public void Insert(string trabajador, string tarjeta, string idCliente, string nombre,
            string telefono, string direccion, string movil, string email, decimal puntos, long fechaNacimiento,
            string sexo, string tipo, DateTime? fechaAlta, int baja, int lopd, bool withTrack = false)
        {
            _restClient
                .Resource(_config.Clientes.Insert.Replace("{dni}", $"{idCliente}"))
                .SendPost(new Cliente
                {
                    apellidos = nombre,
                    telefono = telefono,
                    direccion = direccion,
                    movil = movil,
                    email = email,
                    fecha_nacimiento = fechaNacimiento
                });
        }

        public void Update(string trabajador, string tarjeta, string nombre, string telefono, string direccion,
            string movil, string email, decimal puntos, long fechaNacimiento, string sexo, DateTime? fechaAlta, int baja, int lopd, string idCliente,
            bool withTrack = false)
        {
            var clientes = new List<Cliente> { new Cliente
            {
                dni = idCliente,
                nombre = nombre,
                apellidos = nombre,
                telefono = telefono
            } };

            _restClient
                .Resource(_config.Clientes.Update)
                .SendPut(new
                {
                    bulk = clientes
                });
        }

        public void CheckAndCreateFields()
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

        #region SQL Methods

        public void ResetDniTrackingSql()
        {
            var sql = "UPDATE clientes SET dni_tra = 0";
            _ctx.Database.ExecuteSqlCommand(sql);
        }

        public string GetDniTrackingLastSql()
        {
            var sql = @"SELECT dni FROM clientes WHERE dni_tra = 1";
            return _ctx.Database.SqlQuery<string>(sql)
                .FirstOrDefault() ?? "0";
        }

        public bool AnyWithDniSql(string filter)
        {
            var sql = @"SELECT * FROM clientes WHERE dni = @dni";
            return _ctx.Database.SqlQuery<Cliente>(sql,
                new MySqlParameter("dni", filter))
                .Any();
        }

        public void InsertSql(string trabajador, string tarjeta, string idCliente, string nombre,
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
                throw e;
            }
        }

        public void UpdateSql(string trabajador, string tarjeta, string nombre, string telefono, string direccion,
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

        #endregion SQL Methods
    }
}