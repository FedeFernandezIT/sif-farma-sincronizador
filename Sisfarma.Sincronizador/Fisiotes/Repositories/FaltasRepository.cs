using MySql.Data.MySqlClient;
using Sisfarma.Sincronizador.Fisiotes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisfarma.Sincronizador.Fisiotes.Repositories
{
    public class FaltasRepository : FisiotesRepository
    {
        public FaltasRepository(FisiotesContext ctx) : base(ctx)
        {
        }

        public void CheckAndCreateProveedorField()
        {
            const string table = @"SELECT * from faltas LIMIT 0,1;";
            var fields = new[] { "proveedor" };
            var alters = new[]
            {
                "ALTER TABLE faltas ADD proveedor VARCHAR(255) DEFAULT NULL AFTER laboratorio;"
            };
            CheckAndCreateFieldsTemplate(table, fields, alters);
        }

        public Falta Last()
        {            
            var sql = "select * from faltas order by idPedido Desc Limit 0,1";
            return _ctx.Database.SqlQuery<Falta>(sql)
                .FirstOrDefault();
        }

        public Falta GetByLineaPedido(int pedido, int linea)
        {            
            var sql = @"select * from faltas where idPedido = @pedido AND idLinea= @linea";
            return _ctx.Database.SqlQuery<Falta>(sql,
                new MySqlParameter("pedido", pedido),
                new MySqlParameter("linea", linea))
                .FirstOrDefault();
        }

        public void Insert(int pedido, int linea, string codNacional, string descripcion, string familia, string superFamilia, int cantidad,
            DateTime fechaFalta, string codLaboratorio, string nombreLaboratorio, string proveedor, DateTime? fechaPedido, float pvp, float puc)
        {            
            var sql = @"INSERT IGNORE INTO faltas (idPedido, idLinea, cod_nacional, descripcion, familia, superFamilia, cantidadPedida, fechaFalta, " +
                        "cod_laboratorio, laboratorio, proveedor, fechaPedido, pvp, puc) VALUES(" +
                        "@pedido, @linea, @codNacional, @descripcion, @familia, @superFamilia, @cantidad, @fechaFalta, @codLaboratorio, " +
                        "@nombreLaboratorio, @proveedor, @fechaPedido, @pvp, @puc)";
            _ctx.Database.ExecuteSqlCommand(sql,
                new MySqlParameter("pedido", pedido),
                new MySqlParameter("linea", linea),
                new MySqlParameter("codNacional", codNacional),
                new MySqlParameter("descripcion", descripcion),
                new MySqlParameter("familia", familia),
                new MySqlParameter("superFamilia", superFamilia),
                new MySqlParameter("cantidad", cantidad),
                new MySqlParameter("fechaFalta", fechaFalta),
                new MySqlParameter("codLaboratorio", codLaboratorio),
                new MySqlParameter("nombreLaboratorio", nombreLaboratorio),
                new MySqlParameter("proveedor", proveedor),
                new MySqlParameter("fechaPedido", fechaPedido),
                new MySqlParameter("pvp", pvp),
                new MySqlParameter("puc", puc));        
        }
    }
}
