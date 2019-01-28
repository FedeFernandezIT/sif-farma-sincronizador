using Sisfarma.Sincronizador.Farmatic.Models;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;

namespace Sisfarma.Sincronizador.Farmatic.Repositories
{
    public class VendedoresRepository : FarmaticRepository
    {
        public VendedoresRepository(FarmaticContext ctx) : base(ctx)
        {
        }

        public Vendedor GetOneOrDefaultById(short? idVendedor)
        {
            var sql = @"SELECT * FROM vendedor WHERE IdVendedor = @idVendedor";
            return _ctx.Database.SqlQuery<Vendedor>(sql,
                new SqlParameter("idVendedor", idVendedor ?? SqlInt16.Null))
                .FirstOrDefault();
        }
    }
}