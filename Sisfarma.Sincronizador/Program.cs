using Sisfarma.Sincronizador.Consejo;
using Sisfarma.Sincronizador.Extensions;
using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Farmatic.Models;
using Sisfarma.Sincronizador.Fisiotes;
using Sisfarma.Sincronizador.Models;
using Sisfarma.Sincronizador.Properties;
using Sisfarma.Sincronizador.Sincronizadores;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sisfarma.Sincronizador
{
    internal static class Program
    {        
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        //[STAThread]
        private static void Main()
        {
            string 
                _remoteBase = string.Empty, 
                _remoteServer = string.Empty, 
                _remoteUsername = string.Empty, 
                _remotePassword = string.Empty,
                _localBase = string.Empty,
                _localServer = string.Empty,
                _localUser = string.Empty,
                _localPass = string.Empty;

            int _marketCodeList = 0;

            var notifyIcon = new NotifyIcon();
            notifyIcon.ContextMenuStrip = GetSincronizadorMenuStrip();
            notifyIcon.Icon = Resources.sync;
            notifyIcon.Visible = true;


            LeerFicherosConfiguracion(
            ref _remoteBase,
            ref _remoteServer,
            ref _remoteUsername,
            ref _remotePassword,
            ref _localServer,
            ref _localBase,
            ref _localPass,
            ref _localUser,
            ref _marketCodeList);

            FarmaticService farmaticService = new FarmaticService(_localServer, _localBase, _localUser, _localPass);
            FisiotesService fisiotesService = new FisiotesService(_remoteServer, _remoteUsername, _remoteUsername);
            ConsejoService consejoService = new ConsejoService();


            new PuntosPendientesSincronizador(farmaticService, fisiotesService, consejoService).Run();
            
            //Task.Factory.StartNew(() =>
            //{                
            //    ProcessPuntosPendientes(farmaticService, consejoService, fisiotesService);
            //});
            //Task.Delay(100000).Wait();
            //Task.Factory.StartNew(() => { var i = 0; while (true) Console.WriteLine($"TASK-B{i++}"); });


            Application.ApplicationExit += (sender, @event) => notifyIcon.Visible = false;            
            Application.Run(new SincronizadorApplication());

            void ProcessPuntosPendientes(FarmaticService farmatic, ConsejoService consejo, FisiotesService fisiotes)
            {
                try
                {
                    // Validamos los campos existentes en puntos_pendientes
                    //fisiotes.PuntosPendientes.CheckAndCreateFields();

                    // Creamos la tabla entregas_clientes en la BD remota, si no existe
                    //fisiotes.Entregas.CreateTable(_remoteBase);

                    // Validamos los campos existentes en ClienteAux
                    var existFieldSexo = farmatic.Clientes.HasSexoField();

                    // Verificamos el campo tipoPago en pendientes_puntos en la BD remota
                    //fisiotes.PuntosPendientes.CheckTipoPagoField(_remoteBase);

                    // Recuperamos los puntos pendientes de la última venta y establecemos el ID de venta
                    var idVenta = fisiotes.PuntosPendientes.GetUltimaVenta();

                    // Recuperamos las ventas locales
                    var ventas = farmatic.Ventas.GetByIdGreaterOrEqual(idVenta);
                    foreach (var venta in ventas)
                    {
                        // Establecemos los valores que necesitamos desde la venta extrída de
                        // la BD local.
                        var puesto = venta.Maquina;
                        var tipoPago = venta.TipoVenta;
                        var fechaVenta = venta.FechaHora;
                        var dni = venta.XClie_IdCliente.Strip();
                        if (!string.IsNullOrEmpty(dni))
                        {
                            var cliente = farmatic.Clientes.GetById(dni);
                            // Extraemos los datos necesarios del cliente local para sincronizar con el remoto
                            var clientData = FetchLocalClienteData(farmatic, cliente, existFieldSexo);
                            fisiotes.Clientes.InsertOrUpdate(
                                    clientData.Trabajador, clientData.Tarjeta, cliente.IDCLIENTE, clientData.Nombre.Strip(), clientData.Telefono, clientData.Direccion.Strip(),
                                    clientData.Movil, clientData.Email, clientData.Puntos, clientData.FechaNacimiento, clientData.Sexo, clientData.FechaAlta, clientData.Baja, clientData.Lopd);
                        }

                        // TODO: dni = 0 pero no se usa

                        // Recuramos el vendedor de la venta desde la BD local y establecemos
                        // el trabajador
                        var idVendendor = venta.XVend_IdVendedor;
                        var vendedor = farmatic.Vendedores.GetById(idVendendor);
                        var trabajador = vendedor?.NOMBRE ?? "NO";

                        // Recuperamos la la datos de la venta y el detalle de la misma
                        var fecha = Convert.ToInt32(venta.FechaHora.ToString("yyyyMMdd"));
                        var descuentoVenta = false;
                        var detalleVenta = farmatic.Ventas.GetLineasVentaByVenta(venta.IdVenta);
                        foreach (var linea in detalleVenta)
                        {
                            var recetaPendiente = linea.RecetaPendiente;
                            var receta = linea.TipoAportacion;
                            var precioMed = linea.PVP;

                            var dtoVta = 0d;
                            if (!descuentoVenta)
                            {
                                dtoVta = (venta.DescuentoOpera ?? 0d);
                                descuentoVenta = true;
                            }
                            var dtoLinea = venta.DescuentoLinea ?? 0d;

                            // Establecemos redencion
                            var redencionDb =
                                farmatic.Ventas.GetLineaRedencionByKey(venta.IdVenta, linea.IdNLinea);
                            var redencion = redencionDb?.Redencion ?? 0;

                            var codigoBarra = GetCodidoBarrasFromLocalOrDefault(farmatic, linea.Codigo);

                            // Recuperamos el artículo para establcer los siguientes datos
                            var familia = string.Empty;
                            var superFamilia = string.Empty;
                            var codLaboratorio = string.Empty;
                            var nombreLaboratorio = string.Empty;
                            var proveedor = string.Empty;
                            var pcoste = 0d;

                            var articulo = farmatic.Articulos.GetById(linea.Codigo);
                            if (articulo == null)
                                nombreLaboratorio = "<Sin Laboratorio>";
                            else
                            {
                                pcoste = articulo.Puc;
                                familia = GetFamiliaFromLocalOrDefault(farmatic, articulo.XFam_IdFamilia, "<Sin Clasificar>");

                                codLaboratorio = articulo.Laboratorio ?? string.Empty;

                                superFamilia = familia.Equals("<Sin Clasificar>")
                                    ? superFamilia = familia
                                    : GetSuperFamiliaFromLocalOrDefault(farmatic, familia, "<Sin Clasificar>");

                                proveedor = GetProveedorFromLocalOrDefault(farmatic, articulo.ProveedorHabitual);
                                nombreLaboratorio = GetNombreLaboratorioFromLocalOrDefault(farmatic, consejo, codLaboratorio, "<Sin Laboratorio>");
                            }

                            // Verificamos si hay puntos pendientes desde la base remota, para el item de venta actual
                            var puntos =
                                fisiotes.PuntosPendientes.GetOneOrDefaultByItemVenta(venta.IdVenta, linea.IdNLinea);
                            if (!fisiotes.PuntosPendientes.Exists(venta.IdVenta, linea.IdNLinea))
                            {
                                // Si no hay, lo insertamos
                                var numero = linea.ImporteNeto;
                                var cargado = "no";
                                fisiotes.PuntosPendientes.Insert(venta.IdVenta, linea.IdNLinea, codigoBarra, linea.Codigo,
                                    linea.Descripcion.Strip(), familia.Strip(), linea.Cantidad, Convert.ToDecimal(numero),
                                    tipoPago, fecha, dni, cargado, puesto, trabajador, codLaboratorio.Strip(), nombreLaboratorio.Strip(),
                                    proveedor.Strip(), receta, fechaVenta, superFamilia.Strip(), Convert.ToSingle(precioMed),
                                    Convert.ToSingle(pcoste), Convert.ToSingle(dtoLinea), Convert.ToSingle(dtoVta), Convert.ToSingle(redencion), recetaPendiente);
                            }

                            // Verificamos el idVenta
                            if (venta.IdVenta > idVenta)
                                idVenta = venta.IdVenta - 3;
                        }

                        // Recuperamos el detalle de ventas virtuales
                        var virtuales = farmatic.Ventas.GetLineasVirtualesByVenta(venta.IdVenta);
                        foreach (var lineaVirtual in virtuales)
                        {
                            // Verificamos la entrega del item de venta                        
                            if (!fisiotes.Entregas.Exists(venta.IdVenta, lineaVirtual.IdNLinea))
                            {
                                var numero = lineaVirtual.ImporteNeto;
                                var pvp = lineaVirtual.Pvp;
                                fisiotes.Entregas.Insert(venta.IdVenta, lineaVirtual.IdNLinea, lineaVirtual.Codigo.Strip(),
                                    lineaVirtual.Descripcion.Strip(), lineaVirtual.Cantidad, Convert.ToDecimal(numero), lineaVirtual.TipoLinea, fecha, dni, puesto,
                                    trabajador, fechaVenta, Convert.ToSingle(pvp));
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    //Task.Delay(1500).Wait();
                    Console.WriteLine(e.Message);
                    throw;
                }
            }

            ClienteDto FetchLocalClienteData(FarmaticService farmatic, Farmatic.Models.Cliente cliente, bool hasSexField)
            {
                try
                {
                    var localDestinatarios = farmatic.Destinatarios.GetByCliente(cliente.IDCLIENTE);
                    // Establecemos móvil y email
                    var movil = string.Empty;
                    var email = string.Empty;
                    if (localDestinatarios.Count != 0)
                    {
                        movil = localDestinatarios.First().TlfMovil == null
                                ? string.Empty
                                : localDestinatarios.First().TlfMovil.Trim();

                        email = localDestinatarios.First().Email == null
                                ? string.Empty
                                : localDestinatarios.First().Email.Trim();
                    }
                    else
                    {
                        movil = string.Empty;
                        email = string.Empty;
                    }

                    // Establecemos fecha de nacimiento y sexo
                    var fechaNacimiento = 0L;   // Long
                    var sexo = string.Empty;
                    if (hasSexField)
                    {
                        var localAuxiliar = farmatic.Clientes.GetAuxiliarById<ClienteAuxWithSexo>(cliente.IDCLIENTE);
                        if (localAuxiliar != null)
                        {
                            fechaNacimiento = Convert.ToInt64(localAuxiliar.FechaNac?.ToString("yyyyMMdd"));
                            sexo = localAuxiliar.Sexo == 'V' ? "Hombre"
                                : localAuxiliar.Sexo == 'M' ? "Mujer"
                                : string.Empty;
                        }
                    }
                    else
                    {
                        var localAuxiliar = farmatic.Clientes.GetAuxiliarById<ClienteAux>(cliente.IDCLIENTE);
                        if (localAuxiliar != null)
                            fechaNacimiento = Convert.ToInt64(localAuxiliar.FechaNac?.ToString("yyyyMMdd"));
                    }

                    // Establecemos baja
                    var baja = 0;
                    baja = string.IsNullOrEmpty(cliente.FIS_NIF) || cliente.FIS_NIF.Trim().Equals("No") || cliente.FIS_NIF.Trim().Equals("N")
                            ? 0
                            : 1;

                    // Establecemos lopd
                    var lopd = string.IsNullOrEmpty(cliente.TIPOTARIFA) || cliente.TIPOTARIFA.Trim().Equals("No") || cliente.XCLIE_IDCLIENTEFACT.Trim().Equals("Si")
                            ? 0
                            : 1;

                    // Si sexo aún no tiene valor, le establecemos uno.
                    if (!string.IsNullOrEmpty(sexo))
                        sexo = cliente.FIS_NOMBRE ?? string.Empty;

                    // Establecemos fechaAlta, para ello parseamos la fecha desde FIS_PROVINCIA
                    DateTime? fechaAlta = null;
                    DateTime fechaAux;
                    if (DateTime.TryParse(cliente.FIS_PROVINCIA, out fechaAux))
                        fechaAlta = new DateTime(fechaAux.Year, fechaAux.Month, fechaAux.Day);

                    // Establecemos tarjeta
                    var tarjeta = cliente.FIS_FAX ?? string.Empty;

                    // Establecemos telefono
                    var telefono = cliente.PER_TELEFONO != null
                        ? cliente.PER_TELEFONO.Trim()
                        : cliente.FIS_TELEFONO != null
                        ? cliente.FIS_TELEFONO.Trim()
                        : string.Empty;

                    // Establecemos direccion
                    var direccion = string.Empty;
                    if (!string.IsNullOrEmpty(cliente.PER_DIRECCION) && !string.IsNullOrWhiteSpace(cliente.PER_DIRECCION))
                    {
                        direccion = cliente.PER_DIRECCION.Trim();
                        if (!string.IsNullOrEmpty(cliente.PER_CODPOSTAL) && !string.IsNullOrWhiteSpace(cliente.PER_CODPOSTAL))
                            direccion += $" - {cliente.PER_CODPOSTAL.Trim()}";
                        if (!string.IsNullOrEmpty(cliente.PER_POBLACION) && !string.IsNullOrWhiteSpace(cliente.PER_POBLACION))
                            direccion += $" - {cliente.PER_POBLACION.Trim()}";
                        if (!string.IsNullOrEmpty(cliente.PER_PROVINCIA) && !string.IsNullOrWhiteSpace(cliente.PER_PROVINCIA))
                            direccion += $" ({cliente.PER_PROVINCIA.Trim()})";
                    }

                    // Establecemos nombre
                    var nombre = string.Empty;
                    if (!string.IsNullOrEmpty(cliente.PER_NOMBRE) && !string.IsNullOrWhiteSpace(cliente.PER_NOMBRE))
                        // eliminamos caracacteres inválidos
                        nombre = cliente.PER_NOMBRE.Trim().Strip();

                    // Buscamos el vendedor y lo establecemos como trabajador
                    var trabajador = GetNombreVendedorOrDefault(farmatic, cliente.XVEND_IDVENDEDOR);

                    // Recuperamos los puntos acumulados del cliente
                    var puntos = farmatic.Clientes.GetTotalPuntosById(cliente.IDCLIENTE);

                    return new ClienteDto
                    {
                        FechaNacimiento = fechaNacimiento,
                        FechaAlta = fechaAlta,
                        Email = email,
                        Movil = movil,
                        Direccion = direccion,
                        Nombre = nombre,
                        Sexo = sexo,
                        Telefono = telefono,
                        Tarjeta = tarjeta,
                        Trabajador = trabajador,
                        Puntos = puntos,
                        Baja = baja,
                        Lopd = lopd
                    };
                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            string GetCodidoBarrasFromLocalOrDefault(FarmaticService farmatic, string articulo)
            {
                var sinonimo = farmatic.Sinonimos.GetByArticulo(articulo);
                return sinonimo?.Sinonimo ?? "847000".PadLeft(6, '0');
            }

            string GetFamiliaFromLocalOrDefault(FarmaticService farmatic, short id, string byDefault = "")
            {
                var familiaDb = farmatic.Familias.GetById(id);
                return !string.IsNullOrEmpty(familiaDb?.Descripcion)
                    ? familiaDb.Descripcion
                    : byDefault;
            }

            string GetSuperFamiliaFromLocalOrDefault(FarmaticService farmatic, string familia, string byDefault = "")
            {
                return farmatic.Familias.GetSuperFamiliaDescripcionByFamilia(familia) ?? byDefault;
            }

            string GetNombreLaboratorioFromLocalOrDefault(FarmaticService farmatic, ConsejoService consejo, string codigo, string byDefault = "")
            {
                var nombreLaboratorio = byDefault;
                if (!string.IsNullOrEmpty(codigo?.Trim()) && !string.IsNullOrWhiteSpace(codigo))
                {
                    var laboratorioDb = default(Consejo.Models.Labor); //consejoService.Laboratorios.Get(codigo);
                    if (laboratorioDb == null)
                    {
                        var laboratorioLocal =
                            farmatic.Laboratorios.GetById(codigo);
                        nombreLaboratorio = laboratorioLocal?.Nombre ?? byDefault;
                    }
                    else nombreLaboratorio = laboratorioDb.NOMBRE;
                }
                else nombreLaboratorio = byDefault;
                return nombreLaboratorio;
            }

            string GetProveedorFromLocalOrDefault(FarmaticService farmatic, string proveedor, string byDefault = "")
            {
                var proveedorDb = farmatic.Proveedores.GetById(proveedor);
                return proveedorDb?.FIS_NOMBRE ?? byDefault;
            }

            string GetNombreVendedorOrDefault(FarmaticService farmatic, int? vendedor, string byDefault = "")
            {
                var vendedorDb = farmatic.Vendedores.GetById(Convert.ToInt16(vendedor));
                return vendedorDb?.NOMBRE ?? byDefault;
            }
        }

        private static ContextMenuStrip GetSincronizadorMenuStrip()
        {
            var cms = new ContextMenuStrip();
            cms.Items.Add("Salir", null, (sender, @event) => Application.Exit());
            return cms;
        }

        private static void LeerFicherosConfiguracion(
            ref string _remoteBase,
            ref string _remoteServer,
            ref string _remoteUsername,
            ref string _remotePassword,
            ref string _localServer,
            ref string _localBase,
            ref string _localPass,
            ref string _localUser,
            ref int _marketCodeList)
        {
            try
            {
                var dir = ConfigurationManager.AppSettings["Directory.Setup"];

                // Configuramos el acceso a el servidor remoto de MySql
                var path = ConfigurationManager.AppSettings["File.Remote.Base"];
                _remoteBase = new StreamReader(Path.Combine(dir, path)).ReadLine();

                path = ConfigurationManager.AppSettings["File.Remote.Server"];
                var stream = new StreamReader(Path.Combine(dir, path));
                _remoteServer = stream.ReadLine();
                _remoteUsername = stream.ReadLine();
                _remotePassword = stream.ReadLine();

                // Servidor Local
                path = ConfigurationManager.AppSettings["File.Local.Server"];
                _localServer = new StreamReader(Path.Combine(dir, path)).ReadLine();
                path = ConfigurationManager.AppSettings["File.Local.Base"];
                _localBase = new StreamReader(Path.Combine(dir, path)).ReadLine();
                path = ConfigurationManager.AppSettings["File.Local.User"];
                _localUser = new StreamReader(Path.Combine(dir, path)).ReadLine();
                path = ConfigurationManager.AppSettings["File.Local.Pass"];
                _localPass = new StreamReader(Path.Combine(dir, path)).ReadLine();

                // Único archivo que puede no existir
                path = ConfigurationManager.AppSettings["File.Market.Code.List"];
                _marketCodeList = File.Exists(Path.Combine(dir, path))
                    ? Convert.ToInt32(new StreamReader(Path.Combine(dir, path)).ReadLine())
                    : -1;
            }
            catch (IOException)
            {
                throw new IOException("Ha habido un error en la lectura de algún fichero de configuración. Compruebe que existen dichos ficheros de configuración.");
            }
        }
    }
}