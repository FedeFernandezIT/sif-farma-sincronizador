using Sisfarma.Sincronizador.Farmatic.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Sisfarma.Sincronizador.Farmatic.Repositories
{
    public class ClientesRepository : FarmaticRepository
    {
        public ClientesRepository(FarmaticContext ctx) : base(ctx)
        {
        }

        public List<Cliente> GetGreatThanId(int id)
        {
            var sql =
                @"SELECT TOP 100 * FROM cliente WHERE Idcliente > @ultimoCliente ORDER BY CAST(Idcliente AS DECIMAL(20)) ASC";
            return _ctx.Database.SqlQuery<Cliente>(sql,
                new SqlParameter("ultimoCliente", id))
                .ToList();            
        }

        public T GetAuxiliarById<T>(string cliente) where T : ClienteAux
        {
            var sql = @"SELECT * FROM ClienteAux WHERE idCliente = @idCliente";
            return _ctx.Database.SqlQuery<T>(sql,
                new SqlParameter("idCliente", cliente))
                .FirstOrDefault();
        }

        public decimal GetTotalPuntosById(string idCliente)
        {
            var sql = @"SELECT ISNULL(SUM(cantidad), 0) AS puntos FROM HistoOferta WHERE IdCliente = @idCliente AND TipoAcumulacion = 'P'";
            return _ctx.Database.SqlQuery<decimal>(sql,
                new SqlParameter("idCliente", idCliente))
                .FirstOrDefault(); // Default = 0
        }

        public bool HasSexoField()
        {
            var existFieldSexo = false;

            // Chekeamos si existen los campos
            var connection = _ctx.Database.Connection;

            var sql = "SELECT TOP 1 * FROM ClienteAux";
            var command = connection.CreateCommand();
            command.CommandText = sql;
            connection.Open();
            var reader = command.ExecuteReader();
            var schemaTable = reader.GetSchemaTable();

            foreach (DataRow row in schemaTable.Rows)
            {
                if (row[schemaTable.Columns["ColumnName"]].ToString()
                    .Equals("sexo", StringComparison.CurrentCultureIgnoreCase))
                {
                    existFieldSexo = true;
                    break;
                }
            }
            connection.Close();
            return existFieldSexo;
        }

        public Cliente GetById(string id)
        {
            var sql = @"SELECT * FROM Cliente WHERE Idcliente = @dni";
            return _ctx.Database.SqlQuery<Cliente>(sql,
                new SqlParameter("dni", id))
                .FirstOrDefault();
        }
    }
}