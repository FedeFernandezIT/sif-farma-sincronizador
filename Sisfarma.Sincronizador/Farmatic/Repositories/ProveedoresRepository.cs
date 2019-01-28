using Sisfarma.Sincronizador.Farmatic.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Sisfarma.Sincronizador.Farmatic.Repositories
{
    public class ProveedoresRepository : FarmaticRepository
    {
        public ProveedoresRepository(FarmaticContext ctx) : base(ctx)
        {
        }

        public Proveedor GetById(string id)
        {
            var sql = "SELECT * FROM Proveedor WHERE IDProveedor = @id";
            return _ctx.Database.SqlQuery<Proveedor>(sql,
                new SqlParameter("id", id))
                .FirstOrDefault();
        }

        public IEnumerable<Proveedor> GetAll()
        {
            var sql = @"SELECT * FROM proveedor";
            return _ctx.Database.SqlQuery<Proveedor>(sql)
                .ToList();
        }
    }
}
