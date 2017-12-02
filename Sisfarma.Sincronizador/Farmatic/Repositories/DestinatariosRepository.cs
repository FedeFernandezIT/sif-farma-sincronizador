using Sisfarma.Sincronizador.Farmatic.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Sisfarma.Sincronizador.Farmatic.Repositories
{
    public class DestinatariosRepository : FarmaticRepository
    {
        public DestinatariosRepository(FarmaticContext ctx) : base(ctx)
        {
        }

        public List<Destinatario> GetByCliente(string cliente)
        {
            var sql = @"SELECT * FROM Destinatario WHERE fk_Cliente_1 = @idCliente";
            return _ctx.Database.SqlQuery<Destinatario>(sql,
                new SqlParameter("idCliente", cliente))
                .ToList();            
        }
    }
}
