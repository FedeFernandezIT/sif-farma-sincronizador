using Sisfarma.Sincronizador.Fisiotes.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
                new SqlParameter("encargo", encargo))
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
                new SqlParameter("encargo", encargo),
                new SqlParameter("codNacional", codNacional),
                new SqlParameter("nombre", nombre),
                new SqlParameter("superFamilia", superFamilia),
                new SqlParameter("familia", familia),
                new SqlParameter("codLaboratorio", codLaboratorio),
                new SqlParameter("laboratorio", laboratorio),
                new SqlParameter("proveedor", proveedor),
                new SqlParameter("pvp", pvp),
                new SqlParameter("puc", puc),
                new SqlParameter("dni", dni),
                new SqlParameter("fecha", fecha),
                new SqlParameter("trabajador", trabajador),
                new SqlParameter("unidades", unidades),
                new SqlParameter("fechaEntrega", fechaEntrega),
                new SqlParameter("observaciones", observaciones));
        }
    }
}