using Sisfarma.Sincronizador.Consejo;
using Sisfarma.Sincronizador.Extensions;
using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Farmatic.Models;
using Sisfarma.Sincronizador.Fisiotes;
using Sisfarma.Sincronizador.Fisiotes.Models;
using Sisfarma.Sincronizador.Sincronizadores.SuperTypes;
using System;
using static Sisfarma.Sincronizador.Fisiotes.Repositories.ConfiguracionesRepository;

namespace Sisfarma.Sincronizador.Sincronizadores
{
    public class PuntoPendienteSincronizador : BaseSincronizador
    {
        const string FIELD_POR_DONDE_VOY_ENTREGAS_CLIENTES = FieldsConfiguracion.FIELD_POR_DONDE_VOY_ENTREGAS_CLIENTES;
        const int YEAR_FOUND = 1;

        private readonly bool _hasSexo;

        private ConsejoService _consejo;

        public PuntoPendienteSincronizador(FarmaticService farmatic, FisiotesService fisiotes, ConsejoService consejo)
            : base(farmatic, fisiotes)
        {            
            _consejo = consejo ?? throw new ArgumentNullException(nameof(consejo));
            _hasSexo = farmatic.Clientes.HasSexoField();
        }        

        public override void Process() => ProcessPuntosPendientes(_farmatic, _fisiotes, _consejo);

        void ProcessPuntosPendientes(FarmaticService farmatic, FisiotesService fisiotes, ConsejoService consejo)
        {                        
            var idVenta = fisiotes.PuntosPendientes.GetUltimaVenta();
            var ventas = farmatic.Ventas.GetByIdGreaterOrEqual(YEAR_FOUND, idVenta);
            foreach (var venta in ventas)
            {
                var vendedor = farmatic.Vendedores.GetOneOrDefaultById(venta.XVend_IdVendedor)?.NOMBRE ?? "NO";
                var detalleVenta = farmatic.Ventas.GetLineasVentaByVenta(venta.IdVenta);

                foreach (var linea in detalleVenta)
                {
                    if (!fisiotes.PuntosPendientes.Exists(venta.IdVenta, linea.IdNLinea))                    
                        fisiotes.PuntosPendientes.Insert(
                            GenerarPuntoPendiente(venta, linea, vendedor, farmatic, consejo));                                        
                }

                // Recuperamos el detalle de ventas virtuales
                var virtuales = farmatic.Ventas.GetLineasVirtualesByVenta(venta.IdVenta);
                foreach (var @virtual in virtuales)
                {

                    // Verificamos la entrega del item de venta                        
                    if (!fisiotes.Entregas.Exists(venta.IdVenta, @virtual.IdNLinea))
                    {                        
                        fisiotes.Entregas.Insert(
                            GenerarEntregaCliente(venta, @virtual, vendedor));

                        fisiotes.Configuraciones.Update(FIELD_POR_DONDE_VOY_ENTREGAS_CLIENTES, @virtual.IdVenta.ToString());
                    }
                        
                }
            }
        }

        private EntregaCliente GenerarEntregaCliente(Venta venta, LineaVentaVirtual lineaVirtual, string vendedor)
        {        
            var ec = new EntregaCliente();

            ec.idventa = venta.IdVenta;
            ec.idnlinea = lineaVirtual.IdNLinea;
            ec.codigo = lineaVirtual.Codigo;
            ec.descripcion = lineaVirtual.Descripcion;
            ec.precio = Convert.ToDecimal(lineaVirtual.ImporteNeto);
            ec.tipo = lineaVirtual.TipoLinea;
            ec.fecha = Convert.ToInt32(venta.FechaHora.ToString("yyyyMMdd"));
            ec.dni = venta.XClie_IdCliente.Strip();
            ec.puesto = venta.Maquina;
            ec.trabajador = vendedor;
            ec.pvp = Convert.ToSingle(lineaVirtual.Pvp);
            ec.fechaEntrega = venta.FechaHora;

            return ec;
        }

        private PuntosPendientes GenerarPuntoPendiente(Venta venta, LineaVenta linea, string vendedor, FarmaticService farmatic, ConsejoService consejo)
        {                        
            var redencion = (farmatic.Ventas.GetOneOrDefaultLineaRedencionByKey(venta.IdVenta, linea.IdNLinea)?
                .Redencion) ?? 0;
            var articulo = farmatic.Articulos.GetOneOrDefaultById(linea.Codigo);

            var pp = new PuntosPendientes();
            pp.idventa = venta.IdVenta;
            pp.idnlinea = linea.IdNLinea;
            pp.cargado = "no";
            pp.puesto = venta.Maquina;
            pp.tipoPago = venta.TipoVenta;
            pp.fechaVenta = venta.FechaHora;
            pp.dni = venta.XClie_IdCliente.Strip();
            pp.trabajador = vendedor;
            pp.fecha = Convert.ToInt32(venta.FechaHora.ToString("yyyyMMdd"));            
            pp.recetaPendiente = linea.RecetaPendiente;
            pp.receta = linea.TipoAportacion;
            pp.redencion = Convert.ToSingle(redencion);
            pp.cod_nacional = linea.Codigo;
            pp.cod_barras = GetCodidoBarrasFromLocalOrDefault(farmatic, linea.Codigo);
            pp.descripcion = linea.Descripcion.Strip();
            pp.pvp = Convert.ToSingle(linea.PVP);            
            pp.dtoVenta = Convert.ToSingle(linea.DescuentoOpera ?? 0);
            pp.dtoLinea = Convert.ToSingle(venta.DescuentoLinea ?? 0d);
            pp.precio = Convert.ToDecimal(linea.ImporteNeto);
            pp.cantidad = linea.Cantidad;            

            if (articulo == null)
            {
                pp.laboratorio = "<Sin Laboratorio>";
                pp.cod_laboratorio = string.Empty;
                pp.familia = string.Empty;
                pp.superFamilia = string.Empty;                
                pp.proveedor = string.Empty;
                pp.puc = 0;                
            }                
            else
            {
                pp.cod_laboratorio = articulo.Laboratorio.Strip() ?? string.Empty;
                pp.laboratorio = GetNombreLaboratorioFromLocalOrDefault(farmatic, consejo, pp.cod_laboratorio, "<Sin Laboratorio>");
                pp.puc = Convert.ToSingle(articulo.Puc);
                pp.familia = GetFamiliaFromLocalOrDefault(farmatic, articulo.XFam_IdFamilia, "<Sin Clasificar>").Strip();
                pp.superFamilia = !pp.familia.Equals("<Sin Clasificar>")
                    ? GetSuperFamiliaFromLocalOrDefault(farmatic, pp.familia, "<Sin Clasificar>").Strip()
                    : pp.familia;

                pp.proveedor = GetProveedorFromLocalOrDefault(farmatic, articulo.ProveedorHabitual).Strip();                
            }

            return pp;
        }
        

        string GetCodidoBarrasFromLocalOrDefault(FarmaticService farmaticService, string articulo)
        {
            var sinonimo = farmaticService.Sinonimos.GetOneOrDefaultByArticulo(articulo);
            return sinonimo?.Sinonimo ?? "847000".PadLeft(6, '0');
        }

        string GetFamiliaFromLocalOrDefault(FarmaticService farmatic, short id, string byDefault = "")
        {
            var familiaDb = farmatic.Familias.GetById(id);
            return !string.IsNullOrEmpty(familiaDb?.Descripcion)
                ? familiaDb.Descripcion
                : byDefault;
        }

        string GetSuperFamiliaFromLocalOrDefault(FarmaticService farmaticService, string familia, string byDefault = "")
        {
            return farmaticService.Familias.GetSuperFamiliaDescripcionByFamilia(familia) ?? byDefault;
        }

        string GetNombreLaboratorioFromLocalOrDefault(FarmaticService farmaticService, ConsejoService consejoService, string codigo, string byDefault = "")
        {
            var nombreLaboratorio = byDefault;
            if (!string.IsNullOrEmpty(codigo?.Trim()) && !string.IsNullOrWhiteSpace(codigo))
            {
                var laboratorioDb = default(Consejo.Models.Labor); //consejoService.Laboratorios.Get(codigo);
                if (laboratorioDb == null)
                {
                    var laboratorioLocal =
                        farmaticService.Laboratorios.GetById(codigo);
                    nombreLaboratorio = laboratorioLocal?.Nombre ?? byDefault;
                }
                else nombreLaboratorio = laboratorioDb.NOMBRE;
            }
            else nombreLaboratorio = byDefault;
            return nombreLaboratorio;
        }

        string GetProveedorFromLocalOrDefault(FarmaticService farmaticService, string proveedor, string byDefault = "")
        {
            var proveedorDb = farmaticService.Proveedores.GetById(proveedor);
            return proveedorDb?.FIS_NOMBRE ?? byDefault;
        }                
    }
}
