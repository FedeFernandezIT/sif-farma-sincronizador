using MySql.Data.MySqlClient;
using Sisfarma.Sincronizador.Fisiotes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisfarma.Sincronizador.Fisiotes.Repositories
{
    public class EncargosRepository : FisiotesRepository
    {
        public EncargosRepository(FisiotesContext ctx) : base(ctx)
        {
        }

        public void CheckAndCreateProveedorField()
        {
            const string table = @"SELECT * from encargos LIMIT 0,1;";
            var fields = new[] { "proveedor" };
            var alters = new[]
            {
                @"ALTER TABLE encargos ADD proveedor VARCHAR(255) DEFAULT NULL AFTER laboratorio;"
            };
            CheckAndCreateFieldsTemplate(table, fields, alters);
        }

        public Encargo Last()
        {            
            var sql = @"select * from encargos order by idEncargo Desc Limit 0,1";
            return _ctx.Database.SqlQuery<Encargo>(sql)
                .FirstOrDefault();            
        }

        public Encargo Get(int encargo)
        {            
            var sql = @"select * from encargos where IdEncargo = @encargo";
            return _ctx.Database.SqlQuery<Encargo>(sql,
                new MySqlParameter("encargo", encargo))
                .FirstOrDefault();
        }

        public void Insert(long? encargo, string codNacional, string nombre, string superFamilia, string familia, string codLaboratorio,
            string laboratorio, string proveedor, float? pvp, float? puc, string dni, DateTime? fecha, string trabajador, int? unidades, DateTime? fechaEntrega,
            string observaciones)
        {            
            var sql = @"INSERT IGNORE INTO encargos (idEncargo, cod_nacional, nombre, superFamilia, familia, cod_laboratorio, laboratorio, proveedor, pvp, puc, dni, " +
                        @"fecha, trabajador, unidades, fechaEntrega, observaciones) VALUES(" +
                        @"@encargo, @codNacional, @nombre, @superFamilia, @familia, @codLaboratorio, @laboratorio, @proveedor, @pvp, @puc, @dni, " +
                        @"@fecha, @trabajador, @unidades, @fechaEntrega, @observaciones)";
            _ctx.Database.ExecuteSqlCommand(sql,
                new MySqlParameter("encargo", encargo),
                new MySqlParameter("codNacional", codNacional),
                new MySqlParameter("nombre", nombre),
                new MySqlParameter("superFamilia", superFamilia),
                new MySqlParameter("familia", familia),
                new MySqlParameter("codLaboratorio", codLaboratorio),
                new MySqlParameter("laboratorio", laboratorio),
                new MySqlParameter("proveedor", proveedor),
                new MySqlParameter("pvp", pvp),
                new MySqlParameter("puc", puc),
                new MySqlParameter("dni", dni),
                new MySqlParameter("fecha", fecha),
                new MySqlParameter("trabajador", trabajador),
                new MySqlParameter("unidades", unidades),
                new MySqlParameter("fechaEntrega", fechaEntrega),
                new MySqlParameter("observaciones", observaciones));        
        }
    }
}
