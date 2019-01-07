using Sisfarma.Sincronizador.Fisiotes.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisfarma.Sincronizador.Fisiotes.Repositories
{
    public class ConfiguracionesRepository : FisiotesRepository
    {
        public ConfiguracionesRepository(FisiotesContext ctx) : base(ctx)
        {
        }

        public Configuracion GetByCampo(string field)
        {
            var sql = @"SELECT * FROM configuracion WHERE campo = @field";
            return _ctx.Database.SqlQuery<Configuracion>(sql,
                new SqlParameter("field", field))
                .FirstOrDefault();
        }

        public void Insert(string field)
        {
            var sql = string.Empty;
            switch (field)
            {
                case FieldsConfiguracion.FIELD_STOCK_ENTRADA:
                case FieldsConfiguracion.FIELD_STOCK_SALIDA:
                    sql = @"INSERT IGNORE INTO configuracion (campo, valor) VALUES (@field, NULL)";
                    break;

                case FieldsConfiguracion.FIELD_POR_DONDE_VOY_CON_STOCK:
                case FieldsConfiguracion.FIELD_POR_DONDE_VOY_SIN_STOCK:
                case FieldsConfiguracion.FIELD_POR_DONDE_VOY_BORRAR:
                case FieldsConfiguracion.FIELD_POR_DONDE_VOY_ENTREGAS_CLIENTES:
                    sql = @"INSERT IGNORE INTO configuracion (campo, valor) VALUES (@field, '0')";
                    break;
            }
            _ctx.Database.ExecuteSqlCommand(sql,
                new SqlParameter("field", field));
        }

        public void Update(string field, string value)
        {
            var sql = @"UPDATE IGNORE configuracion SET valor = @value WHERE campo = @field";
            _ctx.Database.ExecuteSqlCommand(sql,
                new SqlParameter("value", value),
                new SqlParameter("field", field));
        }

        public static class FieldsConfiguracion
        {
            public const string FIELD_STOCK_ENTRADA = "fechaActualizacionStockEntrada";
            public const string FIELD_STOCK_SALIDA = "fechaActualizacionStockSalida";
            public const string FIELD_POR_DONDE_VOY_CON_STOCK = "porDondeVoyConStock";
            public const string FIELD_POR_DONDE_VOY_SIN_STOCK = "porDondeVoySinStock";
            public const string FIELD_POR_DONDE_VOY_BORRAR = "porDondeVoyBorrar";
            public const string FIELD_POR_DONDE_VOY_ENTREGAS_CLIENTES = "porDondeEntregasClientes";
        }
    }
}