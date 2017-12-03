using Sisfarma.Sincronizador.Farmatic.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisfarma.Sincronizador.Farmatic.Repositories
{
    public class RecepcionesRepository : FarmaticRepository
    {
        public RecepcionesRepository(FarmaticContext ctx) : base(ctx)
        {
        }

        public IEnumerable<Recepcion> GetByYear()
        {            
            var sql = @"SELECT * From Recep WHERE YEAR(Fecha) >= 2015 Order by IdRecepcion ASC";
            return _ctx.Database.SqlQuery<Recepcion>(sql)
                .ToList();         
        }

        public IEnumerable<Recepcion> GetByIdAndYear(long? pedido)
        {            
            var sql =
                @"SELECT * From Recep WHERE IdRecepcion >= @pedido AND YEAR(Fecha) >= 2015 Order by IdRecepcion ASC";
            return _ctx.Database.SqlQuery<Recepcion>(sql,
                new SqlParameter("pedido", pedido ?? SqlInt64.Null))
                .ToList();            
        }

        public RecepcionResume GetResumeById(int recepcion)
        {            
            var sql = @"SELECT ISNULL(COUNT(IdNLinea),0) AS numLineas, ISNULL(SUM(recibidas*ImportePvp),0) AS importePvp, ISNULL(SUM(importe),0) AS importePuc " +
                @"FROM LINEARECEP WHERE IdRecepcion = @recepcion AND Recibidas <> 0";
            return  _ctx.Database.SqlQuery<RecepcionResume>(sql,
                new SqlParameter("recepcion", recepcion))
                .Single();         
        }

        public IEnumerable<LineaRecepcion> GetLineasById(int recepcion)
        {            
            var sql = @"select * from LINEARECEP where IdRecepcion = @recepcion AND Recibidas <> 0";
            return _ctx.Database.SqlQuery<LineaRecepcion>(sql,
                new SqlParameter("recepcion", recepcion))
                .ToList();         
        }
    }
}
