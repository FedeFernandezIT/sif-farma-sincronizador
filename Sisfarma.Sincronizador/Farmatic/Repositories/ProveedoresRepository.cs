using Sisfarma.Sincronizador.Farmatic.Models;
using System;
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
    }
}
