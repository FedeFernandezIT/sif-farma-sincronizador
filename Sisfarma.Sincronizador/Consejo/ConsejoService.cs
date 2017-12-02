using Sisfarma.Sincronizador.Consejo.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisfarma.Sincronizador.Consejo
{
    public class ConsejoService
    {
        private readonly ConsejoContext _ctx;

        public ConsejoService()
        {
            _ctx = new ConsejoContext();
        }

        public Labor GetLaboratorioByCodigo(string codigo)
        {            
            var sql = @"SELECT * FROM LABOR WHERE CODIGO = @codigo";
            return _ctx.Database.SqlQuery<Labor>(sql,
                new SqlParameter("codigo", codigo))
                .FirstOrDefault();                
        }

        public Esperara GetEspeparaByCodigo(string codigo)
        {            
            var sql = @"SELECT * FROM ESPEPARA WHERE CODIGO = @codigo";
            return _ctx.Database.SqlQuery<Esperara>(sql, 
                new SqlParameter("codigo", codigo))
                .FirstOrDefault();            
        }

        public List<string> GetTextosByCodigoEspepara(string codigo)
        {            
            var sql = @"SELECT t.TEXTO FROM TEXTOS t INNER JOIN TEXTOSESPE te ON te.CODIGOTEXTO = t.CODIGOTEXTO WHERE te.CODIGOESPEPARA = @codigo " +
                "ORDER BY te.CODIGOEPIGRAFE";
            return _ctx.Database.SqlQuery<string>(sql,
                new SqlParameter("codigo", codigo))
                .ToList();         
        }
    }
}
