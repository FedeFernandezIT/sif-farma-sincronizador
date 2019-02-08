using Sisfarma.Sincronizador.Consejo;
using Sisfarma.Sincronizador.Extensions;
using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Farmatic.Models;
using Sisfarma.Sincronizador.Fisiotes;
using Sisfarma.Sincronizador.Fisiotes.DTO.Clientes;
using Sisfarma.Sincronizador.Fisiotes.Models;
using Sisfarma.Sincronizador.Helpers;
using Sisfarma.Sincronizador.Sincronizadores.SuperTypes;
using System;
using System.Threading;
using System.Threading.Tasks;
using static Sisfarma.Sincronizador.Fisiotes.Repositories.ConfiguracionesRepository;

namespace Sisfarma.Sincronizador.Sincronizadores
{
    public class PuntoPendienteSincronizador : TaskSincronizador
    {
        const string FIELD_POR_DONDE_VOY_ENTREGAS_CLIENTES = FieldsConfiguracion.FIELD_POR_DONDE_VOY_ENTREGAS_CLIENTES;
        const string YEAR_FOUND = FieldsConfiguracion.FIELD_ANIO_INICIO;
        const string FIELD_PUNTOS_SISFARMA = FieldsConfiguracion.FIELD_PUNTOS_SISFARMA;
        const string FIELD_FECHA_PUNTOS = FieldsConfiguracion.FIELD_FECHA_PUNTOS;
        const string FIELD_CARGAR_PUNTOS = FieldsConfiguracion.FIELD_CARGAR_PUNTOS;
        const string FIELD_SOLO_PUNTOS_CON_TARJETA = FieldsConfiguracion.FIELD_SOLO_PUNTOS_CON_TARJETA;
        const string FIELD_CANJEO_PUNTOS = FieldsConfiguracion.FIELD_CANJEO_PUNTOS;

        const string FAMILIA_DEFAULT = "<Sin Clasificar>";

        private readonly bool _hasSexo;

        private readonly ConsejoService _consejo;

        public PuntoPendienteSincronizador(FarmaticService farmatic, FisiotesService fisiotes, ConsejoService consejo)
            : base(farmatic, fisiotes)
        {            
            _consejo = consejo ?? throw new ArgumentNullException(nameof(consejo));
            _hasSexo = farmatic.Clientes.HasSexoField();
        }        

        public override void Process() => ProcessPuntosPendientes();

        void ProcessPuntosPendientes()
        {            
            var anioInicio = _fisiotes.Configuraciones.GetByCampo(YEAR_FOUND)
                .ToIntegerOrDefault(@default: DateTime.Now.Year - 2);

            var idVenta = _fisiotes.PuntosPendientes.GetUltimaVenta();
            var ventas = _farmatic.Ventas.GetByIdGreaterOrEqual(anioInicio, idVenta);
            foreach (var venta in ventas)
            {
                _cancellationToken.ThrowIfCancellationRequested();
                var dniString = venta.XClie_IdCliente.Strip();
                var dni = dniString.ToIntegerOrDefault();
                var tarjetaDelCliente = string.Empty;

                // TODO: Carga cliente
                if (dni > 0)
                {
                    var cliente = _farmatic.Clientes.GetOneOrDefaulById(dni);
                    tarjetaDelCliente = cliente?.FIS_FAX ?? string.Empty;
                    if (cliente != null)
                        // TODO: dentro de este método se pregunta si deben cargarse los puntos
                        InsertOrUpdateCliente(cliente);
                }


                var vendedor = _farmatic.Vendedores.GetOneOrDefaultById(venta.XVend_IdVendedor)?.NOMBRE ?? "NO";

                var fechaDePuntos = _fisiotes.Configuraciones.GetByCampo(FIELD_FECHA_PUNTOS);
                var cargarPuntos = _fisiotes.Configuraciones.GetByCampo(FIELD_CARGAR_PUNTOS) ?? string.Empty;
                var puntosDeSisfarma = _fisiotes.Configuraciones.GetByCampo(FIELD_PUNTOS_SISFARMA) ?? string.Empty;

                var detalleVenta = _farmatic.Ventas.GetLineasVentaByVenta(venta.IdVenta);

                // TODO: sólo se carga la desc venta una vez
                var descuentoVentaCargado = false;
                foreach (var linea in detalleVenta)
                {
                    var descuentoVenta = 0d;
                    if (!descuentoVentaCargado)
                    {
                        descuentoVentaCargado = !descuentoVentaCargado;
                        descuentoVenta = venta.DescuentoOpera ?? 0;
                    }
                        

                    if (!_fisiotes.PuntosPendientes.Exists(venta.IdVenta, linea.IdNLinea))                    
                        _fisiotes.PuntosPendientes.Insert(
                            // TODO: dentro de este método se calculan puntos sisfarma                            
                            GenerarPuntoPendiente(puntosDeSisfarma, cargarPuntos, dni, tarjetaDelCliente, descuentoVenta, venta, linea, vendedor, _farmatic, _consejo));                                        
                }

                // TODO: actualizacion puntos del cliente
                if (dni != 0 && 
                    puntosDeSisfarma.ToLower() == "si" &&
                    cargarPuntos.ToLower() != "si" &&
                    fechaDePuntos.ToLower() != "no" &&
                    !string.IsNullOrWhiteSpace(fechaDePuntos) &&
                    venta.FechaHora.Date >= fechaDePuntos.ToDateTimeOrDefault("yyyyMMdd"))
                {
                    var puntosDelCliente = Math.Round(_fisiotes.PuntosPendientes.GetPuntosByDni(dni), 2);

                    var puntosCanjeadosDelCliente = Math.Round(_fisiotes.PuntosPendientes.GetPuntosCanjeadosByDni(dni), 2);

                    var puntosCalculados = Math.Round(puntosDelCliente - puntosCanjeadosDelCliente, 2);

                    _fisiotes.Clientes.UpdatePuntos(new UpdatePuntaje { dni = dniString, puntos = puntosCalculados });
                }

                // Recuperamos el detalle de ventas virtuales
                var virtuales = _farmatic.Ventas.GetLineasVirtualesByVenta(venta.IdVenta);
                foreach (var @virtual in virtuales)
                {

                    // Verificamos la entrega del item de venta                        
                    if (!_fisiotes.Entregas.Exists(venta.IdVenta, @virtual.IdNLinea))
                    {                        
                        _fisiotes.Entregas.Insert(
                            GenerarEntregaCliente(venta, @virtual, vendedor));

                        _fisiotes.Configuraciones.Update(FIELD_POR_DONDE_VOY_ENTREGAS_CLIENTES, @virtual.IdVenta.ToString());
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

        private PuntosPendientes GenerarPuntoPendiente(string puntosDeSisfarma, string cargarPuntos, int dni, string tarjetaDelCliente, double descuentoVenta, Venta venta, LineaVenta linea, string vendedor, FarmaticService farmatic, ConsejoService consejo)
        {            
            var redencion = (farmatic.Ventas.GetOneOrDefaultLineaRedencionByKey(venta.IdVenta, linea.IdNLinea)?
                .Redencion) ?? 0;
            var articulo = farmatic.Articulos.GetOneOrDefaultById(linea.Codigo);
            
            var pp = new PuntosPendientes();
            pp.idventa = venta.IdVenta;
            pp.idnlinea = linea.IdNLinea;
            pp.puntos = 0;
            pp.puesto = venta.Maquina;
            pp.tipoPago = linea.TipoLinea;
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
            pp.dtoVenta = Convert.ToSingle(descuentoVenta);
            pp.dtoLinea = Convert.ToSingle(linea.DescuentoLinea ?? 0d);
            pp.precio = Convert.ToDecimal(linea.ImporteNeto);
            pp.cantidad = linea.Cantidad;
            pp.cargado = cargarPuntos.ToLower().Equals("si") ? "si" : "no";

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

                pp.proveedor = GetProveedorFromLocalOrDefault(farmatic, articulo.IdArticu).Strip();
            }


            
            var sonPuntosDeSisfarma = puntosDeSisfarma.ToLower().Equals("si");            
            var fechaDePuntos = _fisiotes.Configuraciones.GetByCampo(FIELD_FECHA_PUNTOS);
            var fechaDeVenta = venta.FechaHora.Date;
            var cargado = cargarPuntos.ToLower().Equals("si");

            if (dni != 0 && 
                sonPuntosDeSisfarma && !cargado &&
                !string.IsNullOrWhiteSpace(fechaDePuntos) &&
                fechaDePuntos.ToLower() != "no" &&
                fechaDeVenta >= fechaDePuntos.ToDateTimeOrDefault("yyyyMMdd"))
            {
                var tipoFamilia = pp.familia != FAMILIA_DEFAULT ? pp.familia : pp.superFamilia;
                var importe = linea.ImporteNeto;
                var articuloDescripcion = articulo?.Descripcion ?? string.Empty;
                var articuloCantidad = linea.Cantidad;

                pp.puntos = (float)CalcularPuntos(tarjetaDelCliente, tipoFamilia, importe, articuloDescripcion, articuloCantidad);
            }
            else if (dni != 0 && fechaDePuntos.ToLower() != "no")
                pp.cargado = "no";


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

        string GetProveedorFromLocalOrDefault(FarmaticService farmaticService, string codigoNacional, string byDefault = "")
        {
            var proveedorDb = farmaticService.Proveedores.GetOneOrDefaultByCodigoNacional(codigoNacional);
            return proveedorDb?.FIS_NOMBRE ?? byDefault;
        }

        private void InsertOrUpdateCliente(Farmatic.Models.Cliente cliente)
        {
            var clienteDTO = Generator.FetchLocalClienteData(_farmatic, cliente, _hasSexo);

            var puntosDeSisfarma = _fisiotes.Configuraciones.GetByCampo(FIELD_PUNTOS_SISFARMA) ?? string.Empty;
            var debeCargarPuntos = puntosDeSisfarma.ToLower().Equals("no") || string.IsNullOrWhiteSpace(puntosDeSisfarma);

            var dniCliente = cliente.PER_NIF.Strip();
            if (_fisiotes.Configuraciones.PerteneceFarmazul())
            {
                var beBlue = _farmatic.Clientes.EsBeBlue(cliente.XTIPO_IDTIPO) ? 1 : 0;
                if (debeCargarPuntos)
                {
                    _fisiotes.Clientes.InsertOrUpdateBeBlue(
                    clienteDTO.Trabajador, clienteDTO.Tarjeta, cliente.IDCLIENTE, dniCliente, clienteDTO.Nombre.Strip(), clienteDTO.Telefono, clienteDTO.Direccion.Strip(),
                    clienteDTO.Movil, clienteDTO.Email, clienteDTO.Puntos, clienteDTO.FechaNacimiento, clienteDTO.Sexo, clienteDTO.FechaAlta, clienteDTO.Baja, clienteDTO.Lopd,
                    beBlue);
                }
                else
                {
                    _fisiotes.Clientes.InsertOrUpdateBeBlue(
                        clienteDTO.Trabajador, clienteDTO.Tarjeta, cliente.IDCLIENTE, dniCliente, clienteDTO.Nombre.Strip(), clienteDTO.Telefono, clienteDTO.Direccion.Strip(),
                        clienteDTO.Movil, clienteDTO.Email, clienteDTO.FechaNacimiento, clienteDTO.Sexo, clienteDTO.FechaAlta, clienteDTO.Baja, clienteDTO.Lopd,
                        beBlue);
                }
            }
            else if (debeCargarPuntos)
            {
                _fisiotes.Clientes.InsertOrUpdate(
                    clienteDTO.Trabajador, clienteDTO.Tarjeta, cliente.IDCLIENTE, dniCliente, clienteDTO.Nombre.Strip(), clienteDTO.Telefono, clienteDTO.Direccion.Strip(),
                    clienteDTO.Movil, clienteDTO.Email, clienteDTO.Puntos, clienteDTO.FechaNacimiento, clienteDTO.Sexo, clienteDTO.FechaAlta, clienteDTO.Baja, clienteDTO.Lopd,
                    withTrack: true);
            }
            else
            {
                _fisiotes.Clientes.InsertOrUpdate(
                    clienteDTO.Trabajador, clienteDTO.Tarjeta, cliente.IDCLIENTE, dniCliente, clienteDTO.Nombre.Strip(), clienteDTO.Telefono, clienteDTO.Direccion.Strip(),
                    clienteDTO.Movil, clienteDTO.Email, clienteDTO.FechaNacimiento, clienteDTO.Sexo, clienteDTO.FechaAlta, clienteDTO.Baja, clienteDTO.Lopd,
                    withTrack: true);
            }
                
        }

        private double CalcularPuntos(string tarjetaDelCliente, string tipoFamilia, double importe, string articuloDescripcion, int articuloCantidad)
        {
            var puntos = importe *
                (double)(_fisiotes.Familias.GetPuntosByFamiliaTipoVerificado(tipoFamilia));

            var soloPuntosConTarjeta = _fisiotes.Configuraciones.GetByCampo(FIELD_SOLO_PUNTOS_CON_TARJETA);
            if (soloPuntosConTarjeta.ToLower() == "si" && string.IsNullOrWhiteSpace(tarjetaDelCliente))
                puntos = 0;

            var canjeoPuntos = _fisiotes.Configuraciones.GetByCampo(FIELD_CANJEO_PUNTOS);
            if (canjeoPuntos.ToLower() != "si" && articuloDescripcion.Contains("FIDELIZACION"))
                puntos = Math.Abs(articuloCantidad) * -1;

            return puntos;
        }
    }
}
