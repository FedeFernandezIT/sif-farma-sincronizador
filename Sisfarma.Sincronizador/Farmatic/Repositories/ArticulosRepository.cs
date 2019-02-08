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
    public class ArticulosRepository : FarmaticRepository
    {
        public ArticulosRepository(FarmaticContext ctx) : base(ctx)
        {
        }

        public bool Exists(string codigo) => this.GetOneOrDefaultById(codigo) != null;

        public Articulo GetOneOrDefaultById(string codigo)
        {
            var sql = @"SELECT * FROM articu WHERE IdArticu = @codigo";
            return _ctx.Database.SqlQuery<Articulo>(sql,
                new SqlParameter("codigo", codigo))
                .FirstOrDefault();
        }

        

        public List<ArticuloWithIva> GetWithoutStockByIdGreaterOrEqual(string codArticulo)
        {
            var sql = @"select top 1000 a.*, t.Piva AS iva from articu a INNER JOIN Tablaiva t ON t.IdTipoArt = a.XGrup_IdGrupoIva AND t.IdTipoPro = '05' " +
                @" WHERE a.Descripcion <> 'PENDIENTE DE ASIGNACIÓN' AND a.Descripcion <> 'VENTAS VARIAS' AND a.Descripcion <> '   BASE DE DATOS  3/03/2014' " +
                @" AND a.IdArticu >= @codArticulo AND a.StockActual <= 0 ORDER BY a.IdArticu ASC";
            return _ctx.Database.SqlQuery<ArticuloWithIva>(sql,
                new SqlParameter("codArticulo", codArticulo))
                .ToList();
        }

        public List<ArticuloWithIva> GetWithStockByIdGreaterOrEqual(string codArticulo)
        {
            var sql = @"select top 1000 a.*, t.Piva AS iva from articu a INNER JOIN Tablaiva t ON t.IdTipoArt = a.XGrup_IdGrupoIva AND t.IdTipoPro = '05' " +
                @" WHERE a.Descripcion <> 'PENDIENTE DE ASIGNACIÓN' AND a.Descripcion <> 'VENTAS VARIAS' AND a.Descripcion <> '   BASE DE DATOS  3/03/2014' " +
                @" AND a.IdArticu >= @codArticulo AND a.StockActual > 0 ORDER BY a.IdArticu ASC";
            return _ctx.Database.SqlQuery<ArticuloWithIva>(sql,
                new SqlParameter("codArticulo", codArticulo))
                .ToList();
        }

        public List<ArticuloWithIva> GetByFechaUltimaSalidaGreaterOrEqual(DateTime? fechaActualizacionStock)
        {
            var sql = @"select a.*, t.Piva AS iva from articu a INNER JOIN Tablaiva t ON t.IdTipoArt = a.XGrup_IdGrupoIva AND t.IdTipoPro = '05' " +
                @"WHERE a.Descripcion <> 'PENDIENTE DE ASIGNACIÓN' AND a.Descripcion <> 'VENTAS VARIAS' AND a.Descripcion <> '   BASE DE DATOS  3/03/2014' " +
                @"AND FechaUltimaSalida >= @fechaActualizacion ORDER BY FechaUltimaSalida ASC";
            return _ctx.Database.SqlQuery<ArticuloWithIva>(sql,
                new SqlParameter("fechaActualizacion", fechaActualizacionStock ?? SqlDateTime.Null))
                .ToList();
        }        

        public ArticuloWithIva GetControlArticuloFisrtOrDefault(string articulo)
        {
            var sql = @"select TOP 1 idArticu from articu " +
                  " WHERE Descripcion <> 'PENDIENTE DE ASIGNACIÓN' AND Descripcion <> 'VENTAS VARIAS' AND Descripcion <> '   BASE DE DATOS  3/03/2014' " +
                  " AND IdArticu > @articulo AND StockActual > 0 ORDER BY IdArticu ASC";

            return _ctx.Database.SqlQuery<ArticuloWithIva>(sql,
                new SqlParameter("articulo", articulo))
                .FirstOrDefault();            
        }

        public List<ArticuloWithIva> GetByFechaUltimaEntradaGreaterOrEqual(DateTime? fechaActualizacionStock)
        {
            var sql = @"select a.*, t.Piva AS iva from articu a INNER JOIN Tablaiva t ON t.IdTipoArt = a.XGrup_IdGrupoIva AND t.IdTipoPro = '05' " +
                @"WHERE a.Descripcion <> 'PENDIENTE DE ASIGNACIÓN' AND a.Descripcion <> 'VENTAS VARIAS' AND a.Descripcion <> '   BASE DE DATOS  3/03/2014' " +
                @"AND FechaUltimaEntrada >= @fechaActualizacion ORDER BY FechaUltimaEntrada ASC";
            return _ctx.Database.SqlQuery<ArticuloWithIva>(sql,
                new SqlParameter("fechaActualizacion", fechaActualizacionStock ?? SqlDateTime.Null))
                .ToList();
        }
    }
}
