using MySql.Data.MySqlClient;
using Sisfarma.Sincronizador.Fisiotes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisfarma.Sincronizador.Fisiotes.Repositories
{
    public class PedidosRepository : FisiotesRepository
    {
        public PedidosRepository(FisiotesContext ctx) : base(ctx)
        {
        }

        public void CreateTable(string remote)
        {            
            var sql =
                @"SELECT TABLE_NAME AS tipo From information_schema.TABLES WHERE TABLE_SCHEMA = @baseRemoto AND TABLE_NAME = 'pedidos'";
            var result = _ctx.Database.SqlQuery<string>(sql,
                new MySqlParameter("baseRemoto", remote))
                .ToList();
            if (result.Count == 0)
            {
                sql = "CREATE TABLE IF NOT EXISTS `lineas_pedidos` (" +
                        "`id` bigint(255) unsigned NOT NULL AUTO_INCREMENT," +
                        "`fechaPedido` datetime DEFAULT NULL," +
                        "`idPedido` bigint(255) DEFAULT NULL," +
                        "`idLinea` bigint(255) DEFAULT NULL," +
                        "`cod_nacional` bigint(255) DEFAULT NULL," +
                        "`descripcion` varchar(255) DEFAULT NULL," +
                        "`familia` varchar(255) DEFAULT NULL," +
                        "`superFamilia` varchar(255) DEFAULT NULL," +
                        "`cantidad` int(11) DEFAULT NULL," +
                        "`pvp` float DEFAULT NULL," +
                        "`puc` float DEFAULT NULL," +
                        "`cod_laboratorio` varchar(50) DEFAULT NULL," +
                        "`laboratorio` varchar(255) DEFAULT NULL," +
                        "PRIMARY KEY (`id`)" +
                        ") ENGINE=MyISAM DEFAULT CHARSET=latin1;";
                _ctx.Database.ExecuteSqlCommand(sql);

                sql = "CREATE TABLE IF NOT EXISTS `pedidos` (" +
                        "`id` bigint(255) unsigned NOT NULL AUTO_INCREMENT," +
                        "`idPedido` bigint(255) DEFAULT NULL," +
                        "`fechaPedido` datetime DEFAULT NULL," +
                        "`hora` datetime DEFAULT NULL," +
                        "`numLineas` int(11) DEFAULT NULL," +
                        "`importePvp` float DEFAULT NULL," +
                        "`importePuc` float DEFAULT NULL," +
                        "`idProveedor` varchar(50) DEFAULT NULL," +
                        "`proveedor` varchar(255) DEFAULT NULL," +
                        "`trabajador` varchar(255) DEFAULT NULL," +
                        "`sistema` varchar(50) DEFAULT NULL," +
                        "PRIMARY KEY (`id`)" +
                        ") ENGINE=MyISAM DEFAULT CHARSET=latin1;";
                _ctx.Database.ExecuteSqlCommand(sql);
            }            
        }

        public void CheckAndCreateFechaPedidoField()
        {
            const string table = @"SELECT * from lineas_pedidos LIMIT 0,1;";
            var fields = new[] { "fechaPedido" };
            var alters = new[]
            {
                @"ALTER TABLE lineas_pedidos ADD fechaPedido DATETIME AFTER id;"
            };
            CheckAndCreateFieldsTemplate(table, fields, alters);
        }

        public Pedido Last()
        {            
            var sql = @"select * from pedidos order by idPedido Desc Limit 0,1";
            return _ctx.Database.SqlQuery<Pedido>(sql)
                .FirstOrDefault();            
        }

        public Pedido Get(int pedido)
        {            
            var sql = @"select * from pedidos where idPedido = @pedido";
            return _ctx.Database.SqlQuery<Pedido>(sql,
                new MySqlParameter("pedido", pedido))
                .FirstOrDefault();            
        }

        public LineaPedido GetLineaByKey(int pedido, int linea)
        {            
            var sql = @"select * from lineas_pedidos where idPedido = @pedido AND idLinea= @linea";
            return _ctx.Database.SqlQuery<LineaPedido>(sql,
                new MySqlParameter("pedido", pedido),
                new MySqlParameter("linea", linea))
                .FirstOrDefault();            
        }

        public void Insert(int idPedido, DateTime fechaPedido, DateTime hora, int numLineas, float importePvp, float importePuc, string idProveedor, string proveedor, string trabajador)
        {            
            var sql = @"INSERT IGNORE INTO pedidos (idPedido,fechaPedido,hora,numLineas,importePvp,importePuc,idProveedor,proveedor,trabajador) VALUES(" +
                            @"@idPedido, @fechaPedido, @hora, @numLineas, @importePvp, @importePuc, @idProveedor, @proveedor, @trabajador)";
            _ctx.Database.ExecuteSqlCommand(sql,
                new MySqlParameter("idPedido", idPedido),
                new MySqlParameter("fechaPedido", fechaPedido),
                new MySqlParameter("hora", hora),
                new MySqlParameter("numLineas", numLineas),
                new MySqlParameter("importePvp", importePvp),
                new MySqlParameter("importePuc", importePuc),
                new MySqlParameter("idProveedor", idProveedor),
                new MySqlParameter("proveedor", proveedor),
                new MySqlParameter("trabajador", trabajador));         
        }

        public void InsertLinea(DateTime fechaPedido, int idPedido, int idLinea, string codNacional, string descripcion, string familia,
            string superFamilia, int cantidad, float pvp, float puc, string codLaboratorio, string laboratorio)
        {            
            var sql = @"INSERT IGNORE INTO lineas_pedidos (fechaPedido, idPedido, idLinea, cod_nacional, descripcion, familia, superFamilia, cantidad, pvp, puc, cod_laboratorio, laboratorio) VALUES(" +
                    "@fechaPedido, @idPedido, @idLinea, @codNacional, @descripcion, @familia, @superFamilia, @cantidad, @pvp, @puc, @codLaboratorio, @laboratorio)";
            _ctx.Database.ExecuteSqlCommand(sql,
                new MySqlParameter("fechaPedido", fechaPedido),
                new MySqlParameter("idPedido", idPedido),
                new MySqlParameter("idLinea", idLinea),
                new MySqlParameter("codNacional", codNacional),
                new MySqlParameter("descripcion", descripcion),
                new MySqlParameter("familia", familia),
                new MySqlParameter("superFamilia", superFamilia),
                new MySqlParameter("cantidad", cantidad),
                new MySqlParameter("pvp", pvp),
                new MySqlParameter("puc", puc),
                new MySqlParameter("codLaboratorio", codLaboratorio),
                new MySqlParameter("laboratorio", laboratorio));        
        }
    }
}
