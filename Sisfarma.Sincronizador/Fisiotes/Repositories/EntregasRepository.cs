using MySql.Data.MySqlClient;
using Sisfarma.Sincronizador.Fisiotes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisfarma.Sincronizador.Fisiotes.Repositories
{
    public class EntregasRepository : FisiotesRepository
    {
        public EntregasRepository(FisiotesContext ctx) : base(ctx)
        {
        }

        public void CreateTable(string remote)
        {            
            var sql =
                @"SELECT TABLE_NAME AS tipo From information_schema.TABLES WHERE TABLE_SCHEMA = @baseRemoto AND TABLE_NAME = 'entregas_clientes'";
            var result =_ctx.Database.SqlQuery<string>(sql,
                new MySqlParameter("baseRemoto", remote))
                .ToList();
            if (result.Count == 0)
            {
                // Creamos la tabla entregas_clientes
                sql = @"CREATE TABLE IF NOT EXISTS `entregas_clientes` (" +
                        "`cod` bigint(255) NOT NULL AUTO_INCREMENT," +
                        "`idventa` bigint(255) NOT NULL," +
                        "`idnlinea` bigint(255) NOT NULL," +
                        "`codigo` varchar(255) NOT NULL," +
                        "`descripcion` varchar(255) NOT NULL," +
                        "`cantidad` int(255) NOT NULL," +
                        "`precio` decimal(20,2) NOT NULL," +
                        "`tipo` char(2) DEFAULT NULL," +
                        "`fecha` int(255) NOT NULL," +
                        "`dni` varchar(255) NOT NULL," +
                        "`hora` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP," +
                        "`puesto` varchar(100) NOT NULL," +
                        "`trabajador` varchar(100) NOT NULL," +
                        "`fechaEntrega` datetime DEFAULT NULL," +
                        "`pvp` float DEFAULT NULL," +
                        "PRIMARY KEY (`cod`)," +
                        "UNIQUE KEY `unico` (`idventa`,`idnlinea`)," +
                        "KEY `tx_codigo` (`codigo`)," +
                        "KEY `tx_fecha` (`fecha`)," +
                        "KEY `tx_fecha_entrega` (`fechaEntrega`)," +
                        "KEY `tx_venta` (`idventa`,`idnlinea`)" +
                        ") ENGINE=MyISAM DEFAULT CHARSET=latin1;";
                _ctx.Database.ExecuteSqlCommand(sql);            
            }
        }

        public EntregaCliente Last()
        {
            var sql = @"SELECT * FROM entregas_clientes GROUP BY idventa ORDER BY idventa DESC LIMIT 0,1";
            return _ctx.Database.SqlQuery<EntregaCliente>(sql)
                .FirstOrDefault();
        }
        
        public EntregaCliente GetByKey(int venta, int linea)
        {
            var sql = @"SELECT * FROM entregas_clientes WHERE IdVenta = @venta AND Idnlinea = @linea";
            return _ctx.Database.SqlQuery<EntregaCliente>(sql,
                new MySqlParameter("venta", venta),
                new MySqlParameter("linea", linea))
                .FirstOrDefault();
        }

        public void Insert(int venta, int linea, string codigo, string descripcion, int cantidad, decimal numero, string tipoLinea, int fecha,
                string dni, string puesto, string trabajador, DateTime fechaVenta, float? pvp)
        {
            var sql =
                @"INSERT IGNORE INTO entregas_clientes (idventa,idnlinea,codigo,descripcion,cantidad,precio,tipo,fecha,dni,puesto,trabajador,fechaEntrega,pvp) VALUES(" +
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
    }
}
