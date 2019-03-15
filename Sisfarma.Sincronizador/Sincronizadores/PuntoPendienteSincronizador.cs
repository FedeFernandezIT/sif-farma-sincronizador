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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using static Sisfarma.Sincronizador.Fisiotes.Repositories.ConfiguracionesRepository;

namespace Sisfarma.Sincronizador.Sincronizadores
{
    public class PuntoPendienteSincronizador : TaskSincronizador
    {
        private const string FIELD_POR_DONDE_VOY_ENTREGAS_CLIENTES = FieldsConfiguracion.FIELD_POR_DONDE_VOY_ENTREGAS_CLIENTES;
        private const string YEAR_FOUND = FieldsConfiguracion.FIELD_ANIO_INICIO;
        private const string FIELD_PUNTOS_SISFARMA = FieldsConfiguracion.FIELD_PUNTOS_SISFARMA;
        private const string FIELD_FECHA_PUNTOS = FieldsConfiguracion.FIELD_FECHA_PUNTOS;
        private const string FIELD_CARGAR_PUNTOS = FieldsConfiguracion.FIELD_CARGAR_PUNTOS;
        private const string FIELD_SOLO_PUNTOS_CON_TARJETA = FieldsConfiguracion.FIELD_SOLO_PUNTOS_CON_TARJETA;
        private const string FIELD_CANJEO_PUNTOS = FieldsConfiguracion.FIELD_CANJEO_PUNTOS;

        private const string FAMILIA_DEFAULT = "<Sin Clasificar>";

        private readonly bool _hasSexo;
        private readonly string _fileLogs;

        private readonly ConsejoService _consejo;

        private bool _perteneceFarmazul;
        private string _puntosDeSisfarma;
        private string _cargarPuntos;
        private string _fechaDePuntos;
        private string _soloPuntosConTarjeta;
        private string _canjeoPuntos;

        public PuntoPendienteSincronizador(FarmaticService farmatic, FisiotesService fisiotes, ConsejoService consejo)
            : base(farmatic, fisiotes)
        {
            _consejo = consejo ?? throw new ArgumentNullException(nameof(consejo));
            _hasSexo = farmatic.Clientes.HasSexoField();
            _fileLogs = System.Configuration.ConfigurationManager.AppSettings["Directory.Setup"] + @"PuntoPendienteSincronizador.logs";
        }

        public override void Process() => ProcessPuntosPendientes();

        private void ProcessPuntosPendientes()
        {
            var initDateTime = DateTime.UtcNow;
            var puntosInsertados = 0;
            File.AppendAllLines(_fileLogs, new[] { initDateTime.ToString("o") + " Init process v.123" });
            var anioInicio = _fisiotes.Configuraciones.GetByCampo(YEAR_FOUND)
                .ToIntegerOrDefault(@default: DateTime.Now.Year - 2);

            File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Fisiotes Recuperando última venta ..." });
            var idVenta = _fisiotes.PuntosPendientes.GetUltimaVenta();
            File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + $" Fisiotes Última venta {idVenta}" });

            File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Farmatic Recuperando ventas ..." });
            var ventas = _farmatic.Ventas.GetByIdGreaterOrEqual(anioInicio, idVenta);
            File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + $" Farmatic ventas recuperadas {ventas.Count}" });

            _fechaDePuntos = _fisiotes.Configuraciones.GetByCampo(FIELD_FECHA_PUNTOS);
            _cargarPuntos = _fisiotes.Configuraciones.GetByCampo(FIELD_CARGAR_PUNTOS) ?? string.Empty;
            _puntosDeSisfarma = _fisiotes.Configuraciones.GetByCampo(FIELD_PUNTOS_SISFARMA) ?? string.Empty;
            _soloPuntosConTarjeta = _fisiotes.Configuraciones.GetByCampo(FIELD_SOLO_PUNTOS_CON_TARJETA);
            _canjeoPuntos = _fisiotes.Configuraciones.GetByCampo(FIELD_CANJEO_PUNTOS);
            _perteneceFarmazul = _fisiotes.Configuraciones.PerteneceFarmazul();

            foreach (var venta in ventas)
            {
                _cancellationToken.ThrowIfCancellationRequested();
                var dniString = venta.XClie_IdCliente.Strip();
                var dni = dniString.ToIntegerOrDefault();
                var tarjetaDelCliente = string.Empty;

                // TODO: Carga cliente
                if (dni > 0)
                {
                    File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Farmatic Recuperando cliente ..." });
                    var cliente = _farmatic.Clientes.GetOneOrDefaulById(dni);
                    tarjetaDelCliente = cliente?.FIS_FAX ?? string.Empty;
                    if (cliente != null)
                        // TODO: dentro de este método se pregunta si deben cargarse los puntos
                        InsertOrUpdateCliente(cliente);
                }

                var vendedor = _farmatic.Vendedores.GetOneOrDefaultById(venta.XVend_IdVendedor)?.NOMBRE ?? "NO";

                File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Farmatic recuperando detalle venta ..." });
                var detalleVenta = _farmatic.Ventas.GetLineasVentaByVenta(venta.IdVenta);
                File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + $" Farmatic detalle venta recuperado {detalleVenta.Count}" });

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

                    File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Fisiotes verificando existencia puntos pendientes ..." });
                    var existe = _fisiotes.PuntosPendientes.Exists(venta.IdVenta, linea.IdNLinea);
                    File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + $" Fisiotes puntos pendientes existe {existe}" });
                    if (!existe)
                    {
                        var puntoPendienteGenerado = GenerarPuntoPendiente(_puntosDeSisfarma, _cargarPuntos, dni, tarjetaDelCliente, descuentoVenta, venta, linea, vendedor, _farmatic, _consejo);
                        File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Fisiotes insert puntos pendientes ..." });
                        _fisiotes.PuntosPendientes.Insert(
                            // TODO: dentro de este método se calculan puntos sisfarma
                            puntoPendienteGenerado, _fileLogs);
                        File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Fisiotes puntos pendientes insertados" });
                        File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + $" PUNTOS INSERTADOS {++puntosInsertados} en {(DateTime.UtcNow - initDateTime).TotalSeconds} segundos." });
                    }
                }

                // TODO: actualizacion puntos del cliente
                if (dni != 0 &&
                    _puntosDeSisfarma.ToLower() == "si" &&
                    _cargarPuntos.ToLower() != "si" &&
                    _fechaDePuntos.ToLower() != "no" &&
                    !string.IsNullOrWhiteSpace(_fechaDePuntos) &&
                    venta.FechaHora.Date >= _fechaDePuntos.ToDateTimeOrDefault("yyyyMMdd"))
                {
                    File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Fisiotes actualizando puntos del cliente ..." });
                    var puntosDelCliente = Math.Round(_fisiotes.PuntosPendientes.GetPuntosByDni(dni), 2);

                    var puntosCanjeadosDelCliente = Math.Round(_fisiotes.PuntosPendientes.GetPuntosCanjeadosByDni(dni), 2);

                    var puntosCalculados = Math.Round(puntosDelCliente - puntosCanjeadosDelCliente, 2);

                    _fisiotes.Clientes.UpdatePuntos(new UpdatePuntaje { dni = dniString, puntos = puntosCalculados });
                    File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Fisiotes puntos del cliente actualizados" });
                }

                // Recuperamos el detalle de ventas virtuales
                File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Farmatic recuperando lineas virtuales ..." });
                var virtuales = _farmatic.Ventas.GetLineasVirtualesByVenta(venta.IdVenta);
                File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + $" Farmatic lineas virtuales {virtuales.Count}" });

                foreach (var @virtual in virtuales)
                {
                    // Verificamos la entrega del item de venta
                    if (!_fisiotes.Entregas.Exists(venta.IdVenta, @virtual.IdNLinea))
                    {
                        File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Fisiotes insert entregas ..." });
                        _fisiotes.Entregas.Insert(
                            GenerarEntregaCliente(venta, @virtual, vendedor));

                        _fisiotes.Configuraciones.Update(FIELD_POR_DONDE_VOY_ENTREGAS_CLIENTES, @virtual.IdVenta.ToString());
                        File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Fisiotes entrega insertada" });
                    }
                }
            }
        }

        private EntregaCliente GenerarEntregaCliente(Venta venta, LineaVentaVirtual lineaVirtual, string vendedor)
        {
            File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Farmatic generando entrega ..." });
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

            File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Farmatic entrega generada" });
            return ec;
        }

        private PuntosPendientes GenerarPuntoPendiente(string puntosDeSisfarma, string cargarPuntos, int dni, string tarjetaDelCliente, double descuentoVenta, Venta venta, LineaVenta linea, string vendedor, FarmaticService farmatic, ConsejoService consejo)
        {
            File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Farmatic generando puntos ..." });

            File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Farmatic recuperando redencion ..." });
            var redencion = (farmatic.Ventas.GetOneOrDefaultLineaRedencionByKey(venta.IdVenta, linea.IdNLinea)?
                .Redencion) ?? 0;
            File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Farmatic redencion recuperada" });

            File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Farmatic recuperando articulo ..." });
            var articulo = farmatic.Articulos.GetOneOrDefaultById(linea.Codigo);
            File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Farmatic articulo recuperado" });

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
                pp.familia = FAMILIA_DEFAULT;
                pp.superFamilia = FAMILIA_DEFAULT;
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

                File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Farmatic recuperando proveedor ..." });
                pp.proveedor = GetProveedorFromLocalOrDefault(farmatic, articulo.IdArticu).Strip();
                File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Farmatic proveedor recuperado" });
            }

            var sonPuntosDeSisfarma = puntosDeSisfarma.ToLower().Equals("si");
            var fechaDePuntos = _fechaDePuntos;
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

                File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Fisiotes calculado puntos ..." });
                pp.puntos = (float)CalcularPuntos(tarjetaDelCliente, tipoFamilia, importe, articuloDescripcion, articuloCantidad);
                File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Fisiotes puntos calculados" });
            }
            else if (dni != 0 && fechaDePuntos.ToLower() != "no")
                pp.cargado = "no";

            File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Farmatic puntos generados" });
            return pp;
        }

        private string GetCodidoBarrasFromLocalOrDefault(FarmaticService farmaticService, string articulo)
        {
            File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Farmatic recuperando sinonimos ..." });
            var sinonimo = farmaticService.Sinonimos.GetOneOrDefaultByArticulo(articulo);
            File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Farmatic sinonimo recuperado" });
            return sinonimo?.Sinonimo ?? "847000".PadLeft(6, '0');
        }

        private string GetFamiliaFromLocalOrDefault(FarmaticService farmatic, short id, string byDefault = "")
        {
            File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Farmatic recuperando familia ..." });
            var familiaDb = farmatic.Familias.GetById(id);
            File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Farmatic familia recuperada" });
            return !string.IsNullOrEmpty(familiaDb?.Descripcion)
                ? familiaDb.Descripcion
                : byDefault;
        }

        private string GetSuperFamiliaFromLocalOrDefault(FarmaticService farmaticService, string familia, string byDefault = "")
        {
            File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Farmatic recuperando super familia ..." });
            var superfamilia = farmaticService.Familias.GetSuperFamiliaDescripcionByFamilia(familia) ?? byDefault;
            File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Farmatic super familia recuperada" });
            return superfamilia;
        }

        private string GetNombreLaboratorioFromLocalOrDefault(FarmaticService farmaticService, ConsejoService consejoService, string codigo, string byDefault = "")
        {
            File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Farmatic recuperando laboratorio ..." });
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
            File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Farmatic laboratorio recuperado" });
            return nombreLaboratorio;
        }

        private string GetProveedorFromLocalOrDefault(FarmaticService farmaticService, string codigoNacional, string byDefault = "")
        {
            var proveedorDb = farmaticService.Proveedores.GetOneOrDefaultByCodigoNacional(codigoNacional);
            return proveedorDb?.FIS_NOMBRE ?? byDefault;
        }

        private void InsertOrUpdateCliente(Farmatic.Models.Cliente cliente)
        {
            File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Farmatic generando cliente ..." });
            var clienteDTO = Generator.FetchLocalClienteData(_farmatic, cliente, _hasSexo);
            File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Farmatic cliente generado" });

            var puntosDeSisfarma = _puntosDeSisfarma;
            var debeCargarPuntos = puntosDeSisfarma.ToLower().Equals("no") || string.IsNullOrWhiteSpace(puntosDeSisfarma);

            var dniCliente = cliente.PER_NIF.Strip();
            if (_perteneceFarmazul)
            {
                var beBlue = _farmatic.Clientes.EsBeBlue(cliente.XTIPO_IDTIPO) ? 1 : 0;

                if (debeCargarPuntos)
                {
                    File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Fisiotes insert cliente ..." });
                    _fisiotes.Clientes.InsertOrUpdateBeBlue(
                    clienteDTO.Trabajador, clienteDTO.Tarjeta, cliente.IDCLIENTE, dniCliente, clienteDTO.Nombre.Strip(), clienteDTO.Telefono, clienteDTO.Direccion.Strip(),
                    clienteDTO.Movil, clienteDTO.Email, clienteDTO.Puntos, clienteDTO.FechaNacimiento, clienteDTO.Sexo, clienteDTO.FechaAlta, clienteDTO.Baja, clienteDTO.Lopd,
                    beBlue);
                    File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Fisiotes cliente insertado" });
                }
                else
                {
                    File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Fisiotes insert cliente ..." });
                    _fisiotes.Clientes.InsertOrUpdateBeBlue(
                        clienteDTO.Trabajador, clienteDTO.Tarjeta, cliente.IDCLIENTE, dniCliente, clienteDTO.Nombre.Strip(), clienteDTO.Telefono, clienteDTO.Direccion.Strip(),
                        clienteDTO.Movil, clienteDTO.Email, clienteDTO.FechaNacimiento, clienteDTO.Sexo, clienteDTO.FechaAlta, clienteDTO.Baja, clienteDTO.Lopd,
                        beBlue);
                    File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Fisiotes cliente insertado" });
                }
            }
            else if (debeCargarPuntos)
            {
                File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Fisiotes insert cliente ..." });
                _fisiotes.Clientes.InsertOrUpdate(
                    clienteDTO.Trabajador, clienteDTO.Tarjeta, cliente.IDCLIENTE, dniCliente, clienteDTO.Nombre.Strip(), clienteDTO.Telefono, clienteDTO.Direccion.Strip(),
                    clienteDTO.Movil, clienteDTO.Email, clienteDTO.Puntos, clienteDTO.FechaNacimiento, clienteDTO.Sexo, clienteDTO.FechaAlta, clienteDTO.Baja, clienteDTO.Lopd,
                    withTrack: true);
                File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Fisiotes cliente insertado" });
            }
            else
            {
                File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Fisiotes insert cliente ..." });
                _fisiotes.Clientes.InsertOrUpdate(
                    clienteDTO.Trabajador, clienteDTO.Tarjeta, cliente.IDCLIENTE, dniCliente, clienteDTO.Nombre.Strip(), clienteDTO.Telefono, clienteDTO.Direccion.Strip(),
                    clienteDTO.Movil, clienteDTO.Email, clienteDTO.FechaNacimiento, clienteDTO.Sexo, clienteDTO.FechaAlta, clienteDTO.Baja, clienteDTO.Lopd,
                    withTrack: true);
                File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Fisiotes cliente insertado" });
            }
        }

        private double CalcularPuntos(string tarjetaDelCliente, string tipoFamilia, double importe, string articuloDescripcion, int articuloCantidad)
        {
            var puntos = importe *
                (double)(_fisiotes.Familias.GetPuntosByFamiliaTipoVerificado(tipoFamilia));

            if (_soloPuntosConTarjeta.ToLower() == "si" && string.IsNullOrWhiteSpace(tarjetaDelCliente))
                puntos = 0;

            if (_canjeoPuntos.ToLower() != "si" && articuloDescripcion.Contains("FIDELIZACION"))
                puntos = Math.Abs(articuloCantidad) * -1;

            return puntos;
        }
    }
}