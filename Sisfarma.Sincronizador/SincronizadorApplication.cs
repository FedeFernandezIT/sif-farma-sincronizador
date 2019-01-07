using Sisfarma.RestClient;
using Sisfarma.Sincronizador.Consejo;
using Sisfarma.Sincronizador.Extensions;
using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Farmatic.Models;
using Sisfarma.Sincronizador.Fisiotes;
using Sisfarma.Sincronizador.Fisiotes.Models;
using Sisfarma.Sincronizador.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Sisfarma.Sincronizador.Fisiotes.Repositories.ConfiguracionesRepository;

namespace Sisfarma.Sincronizador
{
    public class SincronizadorApplication : ApplicationContext
    {
        private string _remoteBase, _remoteServer, _remoteUsername, _remotePassword;
        private string _localBase, _localServer, _localUser, _localPass;
        private int _marketCodeList;

        private System.Timers.Timer timerClientes;
        private System.Timers.Timer timerClientesHuecos;
        //private System.Timers.Timer timerActualizarRecetasPendientes;
        //private System.Timers.Timer timerActualizarEntregasClientes;
        //private System.Timers.Timer timerActualizarProductosBorrados;
        //private System.Timers.Timer timerActualizarPuntosPendientes;
        //private System.Timers.Timer timerSinonimos;
        //private System.Timers.Timer timerControlSinStockInicial;
        //private System.Timers.Timer timerControlStockFechasSalida;
        //private System.Timers.Timer timerControlStockInicial;
        //private System.Timers.Timer timerControlStockFechasEntrada;
        //private System.Timers.Timer timerPuntosPendientes;
        //private System.Timers.Timer timerPedidos;
        //private System.Timers.Timer timerListasTiendas;
        //private System.Timers.Timer timerCategorias;
        //private System.Timers.Timer timerListasFechas;
        //private System.Timers.Timer timerListas;
        //private System.Timers.Timer timerEncargos;
        //private System.Timers.Timer timerFamilias;
        //private System.Timers.Timer timerProductosCriticos;

        public SincronizadorApplication()
        {
            LeerFicherosConfiguracion();
            InitializeTimer();
        }

        private void InitializeTimer()
        {
            timerClientes = new System.Timers.Timer(2500);
            timerClientes.Elapsed += (sender, @event) =>
            {
                //timerClientes.Stop();
                FarmaticService farmatic = new FarmaticService();
                FisiotesService fisiotes = new FisiotesService(_remoteServer, _remoteUsername, _remoteUsername);
                ProcessClientes(farmatic, fisiotes);
                //timerClientes.Start();
            };

            timerClientesHuecos = new System.Timers.Timer(63000);
            timerClientesHuecos.Elapsed += (sender, @event) =>
            {
                timerClientesHuecos.Stop();
                FarmaticService farmatic = new FarmaticService();
                FisiotesService fisiotes = new FisiotesService(_remoteServer, _remoteUsername, _remoteUsername);
                ProcessClientesHuecos(farmatic, fisiotes);
                timerClientesHuecos.Start();
            };

            //timerActualizarRecetasPendientes = new System.Timers.Timer(5000);
            //timerActualizarRecetasPendientes.Elapsed += (sender, @event) =>
            //{
            //    timerActualizarRecetasPendientes.Stop();
            //    FarmaticService farmatic = new FarmaticService();
            //    FisiotesService fisiotes = new FisiotesService();
            //    ProcessClientesHuecos(farmatic, fisiotes);
            //    timerActualizarRecetasPendientes.Start();
            //};

            //timerActualizarEntregasClientes = new System.Timers.Timer(30000);
            //timerActualizarEntregasClientes.Elapsed += (sender, @event) =>
            //{
            //    timerActualizarEntregasClientes.Stop();
            //    FarmaticService farmatic = new FarmaticService();
            //    FisiotesService fisiotes = new FisiotesService();
            //    ProcessUpdateEntregasClientes(farmatic, fisiotes);
            //    timerActualizarEntregasClientes.Start();
            //};

            //timerActualizarProductosBorrados = new System.Timers.Timer(30000);
            //timerActualizarProductosBorrados.Elapsed += (sender, @event) =>
            //{
            //    timerActualizarProductosBorrados.Stop();
            //    FarmaticService farmatic = new FarmaticService();
            //    FisiotesService fisiotes = new FisiotesService();
            //    ProcessUpdateProductosBorrados(farmatic, fisiotes);
            //    timerActualizarProductosBorrados.Start();
            //};

            //timerActualizarPuntosPendientes = new System.Timers.Timer(5000);
            //timerActualizarPuntosPendientes.Elapsed += (sender, @event) =>
            //{
            //    timerActualizarPuntosPendientes.Stop();
            //    FarmaticService farmatic = new FarmaticService();
            //    FisiotesService fisiotes = new FisiotesService();
            //    ProcessUpdatePuntosPendientes(farmatic, fisiotes);
            //    timerActualizarPuntosPendientes.Start();
            //};

            //timerSinonimos = new System.Timers.Timer(50000);
            //timerSinonimos.Elapsed += (sender, @event) =>
            //{
            //    timerSinonimos.Stop();
            //    FarmaticService farmatic = new FarmaticService();
            //    FisiotesService fisiotes = new FisiotesService();
            //    ProcessSinonimos(farmatic, fisiotes);
            //    timerSinonimos.Start();
            //};

            //timerControlSinStockInicial = new System.Timers.Timer(10000);
            //timerControlSinStockInicial.Elapsed += (sender, @event) =>
            //{
            //    timerControlSinStockInicial.Stop();
            //    FarmaticService farmatic = new FarmaticService();
            //    FisiotesService fisiotes = new FisiotesService();
            //    ConsejoService consejo = new ConsejoService();
            //    ProcessControlSinStockInicial(farmatic, consejo, fisiotes);
            //    timerControlSinStockInicial.Start();
            //};

            //timerControlStockFechasSalida = new System.Timers.Timer(60000);
            //timerControlStockFechasSalida.Elapsed += (sender, @event) =>
            //{
            //    timerControlStockFechasSalida.Stop();
            //    FarmaticService farmatic = new FarmaticService();
            //    FisiotesService fisiotes = new FisiotesService();
            //    ConsejoService consejo = new ConsejoService();
            //    ProcessControlStockFechasSalida(farmatic, consejo, fisiotes);
            //    timerControlStockFechasSalida.Start();
            //};

            //timerControlStockInicial = new System.Timers.Timer(5000);
            //timerControlStockInicial.Elapsed += (sender, @event) =>
            //{
            //    timerControlStockInicial.Stop();
            //    FarmaticService farmatic = new FarmaticService();
            //    FisiotesService fisiotes = new FisiotesService();
            //    ConsejoService consejo = new ConsejoService();
            //    ProcessControlStockInicial(farmatic, consejo, fisiotes);
            //    timerControlStockInicial.Start();
            //};

            //timerControlStockFechasEntrada = new System.Timers.Timer(60000);
            //timerControlStockFechasEntrada.Elapsed += (sender, @event) =>
            //{
            //    timerControlStockFechasEntrada.Stop();
            //    FarmaticService farmatic = new FarmaticService();
            //    FisiotesService fisiotes = new FisiotesService();
            //    ConsejoService consejo = new ConsejoService();
            //    ProcessControlStockFechasEntrada(farmatic, consejo, fisiotes);
            //    timerControlStockFechasEntrada.Start();
            //};

            //timerPuntosPendientes = new System.Timers.Timer(3000);
            //timerPuntosPendientes.Elapsed += (sender, @event) =>
            //{
            //    timerPuntosPendientes.Stop();
            //    FarmaticService farmatic = new FarmaticService();
            //    FisiotesService fisiotes = new FisiotesService();
            //    ConsejoService consejo = new ConsejoService();
            //    ProcessPuntosPendientes(farmatic, consejo, fisiotes);
            //    timerPuntosPendientes.Start();
            //};

            //timerPedidos = new System.Timers.Timer(3100);
            //timerPedidos.Elapsed += (sender, @event) =>
            //{
            //    timerPedidos.Stop();
            //    FarmaticService farmatic = new FarmaticService();
            //    FisiotesService fisiotes = new FisiotesService();
            //    ConsejoService consejo = new ConsejoService();
            //    ProcessPedidos(farmatic, consejo, fisiotes);
            //    timerPedidos.Start();
            //};

            //timerListasTiendas = new System.Timers.Timer(4500);
            //timerListasTiendas.Elapsed += (sender, @event) =>
            //{
            //    timerListasTiendas.Stop();
            //    FarmaticService farmatic = new FarmaticService();
            //    FisiotesService fisiotes = new FisiotesService();
            //    ConsejoService consejo = new ConsejoService();
            //    ProcessListaTienda(farmatic, consejo, fisiotes);
            //    timerListasTiendas.Start();
            //};

            //timerCategorias = new System.Timers.Timer(64000);
            //timerCategorias.Elapsed += (sender, @event) =>
            //{
            //    timerCategorias.Stop();
            //    FarmaticService farmatic = new FarmaticService();
            //    FisiotesService fisiotes = new FisiotesService();
            //    ProcessCategorias(farmatic, fisiotes);
            //    timerCategorias.Start();
            //};

            //timerListasFechas = new System.Timers.Timer(62000);
            //timerListasFechas.Elapsed += (sender, @event) =>
            //{
            //    timerListasFechas.Stop();
            //    FarmaticService farmatic = new FarmaticService();
            //    FisiotesService fisiotes = new FisiotesService();
            //    ProcessListasFechas(farmatic, fisiotes);
            //    timerListasFechas.Start();
            //};

            //timerListas = new System.Timers.Timer(61000);
            //timerListas.Elapsed += (sender, @event) =>
            //{
            //    timerListas.Stop();
            //    FarmaticService farmatic = new FarmaticService();
            //    FisiotesService fisiotes = new FisiotesService();
            //    ProcessListas(farmatic, fisiotes);
            //    timerListas.Start();
            //};

            //timerEncargos = new System.Timers.Timer(4000);
            //timerEncargos.Elapsed += (sender, @event) =>
            //{
            //    timerEncargos.Stop();
            //    FarmaticService farmatic = new FarmaticService();
            //    FisiotesService fisiotes = new FisiotesService();
            //    ConsejoService consejo = new ConsejoService();
            //    ProcessEncargos(farmatic, consejo, fisiotes);
            //    timerEncargos.Start();
            //};

            //timerFamilias = new System.Timers.Timer(65000);
            //timerFamilias.Elapsed += (sender, @event) =>
            //{
            //    timerFamilias.Stop();
            //    FarmaticService farmatic = new FarmaticService();
            //    FisiotesService fisiotes = new FisiotesService();
            //    ProcessFamilia(farmatic, fisiotes);
            //    timerFamilias.Start();
            //};

            //timerProductosCriticos = new System.Timers.Timer(3500);
            //timerProductosCriticos.Elapsed += (sender, @event) =>
            //{
            //    timerProductosCriticos.Stop();
            //    FarmaticService farmatic = new FarmaticService();
            //    FisiotesService fisiotes = new FisiotesService();
            //    ConsejoService consejo = new ConsejoService();
            //    ProcessProductosCrticos(farmatic, consejo, fisiotes);
            //    timerProductosCriticos.Start();
            //};

            timerClientes.Start();
            //timerClientesHuecos.Start();
            //timerActualizarRecetasPendientes.Start();
            //timerActualizarEntregasClientes.Start();
            //timerActualizarProductosBorrados.Start();
            //timerActualizarPuntosPendientes.Start();
            //timerSinonimos.Start();
            //timerControlSinStockInicial.Start();
            //timerControlStockFechasSalida.Start();
            //timerControlStockInicial.Start();
            //timerControlStockFechasEntrada.Start();
            //timerPuntosPendientes.Start();
            //timerPedidos.Start();
            //timerListasTiendas.Start();
            //timerCategorias.Start();
            //timerListasFechas.Start();
            //timerListas.Start();
            //timerEncargos.Start();
            //timerFamilias.Start();
            //timerProductosCriticos.Start();
        }

        private void LeerFicherosConfiguracion()
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

        public void ProcessClientes(FarmaticService farmaticService, FisiotesService fisiotesService)
        {
            try
            {
                // Validamos los campos existentes en ClienteAux
                var existFieldSexo = farmaticService.Clientes.HasSexoField();

                //Recuperamos el DNI del último cliente, lo utilizamos para recuperar luego
                // un cliente remoto.
                var lastCliente = string.Empty;
                var formatTime = DateTime.Now.ToString("HHmm");

                lastCliente = "1500".Equals(formatTime) || "2300".Equals(formatTime)
                        ? "0"
                        : fisiotesService.Clientes.GetDniTrackingLast();

                // Recuperamos los clientes locales mayores al DNI del último cliente remoto
                var localClientes = new List<Farmatic.Models.Cliente>();
                localClientes = farmaticService.Clientes.GetGreatThanId(Convert.ToInt32(lastCliente));

                // Sincronizamos los clientes locales con la BD remota
                var contadorHuecos = -1;
                foreach (var cliente in localClientes)
                {
                    if (contadorHuecos == -1)
                        //Guardamos el Id del cliente local
                        contadorHuecos = Convert.ToInt32(cliente.IDCLIENTE);

                    //Extraemos los datos necesarios del cliente local para sincronizar con el remoto
                    var clientData = FetchLocalClienteData(farmaticService, cliente, false);

                    //Recuperamos el último cliente remoto
                    fisiotesService.Clientes.ResetDniTracking();
                    if (!fisiotesService.Clientes.AnyWithDni(lastCliente))
                    {
                        var tipo = "cliente";
                        fisiotesService.Clientes.Insert(
                                clientData.Trabajador, clientData.Tarjeta, cliente.IDCLIENTE, clientData.Nombre.Strip(), clientData.Telefono, clientData.Direccion.Strip(),
                                clientData.Movil, clientData.Email, clientData.Puntos, clientData.FechaNacimiento, clientData.Sexo, tipo, clientData.FechaAlta, clientData.Baja, clientData.Lopd,
                                withTrack: true);
                    }
                    else
                    {
                        fisiotesService.Clientes.Update(
                                clientData.Trabajador, clientData.Tarjeta, clientData.Nombre.Strip(), clientData.Telefono, clientData.Direccion.Strip(),
                                clientData.Movil, clientData.Email, clientData.Puntos, clientData.FechaNacimiento, clientData.Sexo, clientData.FechaAlta, clientData.Baja, clientData.Lopd,
                                cliente.IDCLIENTE, withTrack: true);
                    }

                    //Almacenamos todos los huecos de clientes que hayan.
                    var intIdCliente = Convert.ToInt32(cliente.IDCLIENTE);
                    if (intIdCliente != contadorHuecos)
                    {
                        for (int i = contadorHuecos; i < intIdCliente; i++)
                        {
                            if (!fisiotesService.Huecos.Any(i))
                            {
                                fisiotesService.Huecos.Insert(i.ToString());
                            }
                        }
                        contadorHuecos = intIdCliente;
                    }
                    contadorHuecos++;
                }
            }
            catch (Exception e)
            {
                //Task.Delay(1500).Wait();
                Console.WriteLine(e.Message);
            }
        }

        private ClienteDto FetchLocalClienteData(FarmaticService farmaticService, Farmatic.Models.Cliente cliente, bool hasSexField)
        {
            try
            {
                var localDestinatarios = farmaticService.Destinatarios.GetByCliente(cliente.IDCLIENTE);
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
                    var localAuxiliar = farmaticService.Clientes.GetAuxiliarById<ClienteAuxWithSexo>(cliente.IDCLIENTE);
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
                    var localAuxiliar = farmaticService.Clientes.GetAuxiliarById<ClienteAux>(cliente.IDCLIENTE);
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
                var trabajador = GetNombreVendedorOrDefault(farmaticService, cliente.XVEND_IDVENDEDOR);

                // Recuperamos los puntos acumulados del cliente
                var puntos = farmaticService.Clientes.GetTotalPuntosById(cliente.IDCLIENTE);

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

        private string GetNombreVendedorOrDefault(FarmaticService farmaticService, int? vendedor, string byDefault = "")
        {
            var vendedorDb = farmaticService.Vendedores.GetById(Convert.ToInt16(vendedor));
            return vendedorDb?.NOMBRE ?? byDefault;
        }

        private void ProcessClientesHuecos(FarmaticService farmaticService, FisiotesService fisiotesService)
        {
            try
            {
                // Validamos los campos existentes en ClienteAux
                var existFieldSexo = farmaticService.Clientes.HasSexoField();

                // Recuperamos los huecos de clientes en forma ascendente
                var remoteHuecos = fisiotesService.Huecos.GetByOrderAsc();

                // Sincronizamos los clientes con huecos con la BD remota
                foreach (var hueco in remoteHuecos)
                {
                    var cliente = farmaticService.Clientes.GetById(hueco);
                    if (cliente != null)
                    {
                        // Extraemos los datos necesarios del cliente local para sincronizar con el remoto
                        var clientData = FetchLocalClienteData(farmaticService, cliente, existFieldSexo);

                        // Recuperamos el cliente remoto que coincida con el cliente local
                        if (!fisiotesService.Clientes.AnyWithDni(cliente.IDCLIENTE))
                        {
                            var tipo = "cliente";
                            fisiotesService.Clientes.Insert(
                                clientData.Trabajador, clientData.Tarjeta, cliente.IDCLIENTE, clientData.Nombre.Strip(), clientData.Telefono, clientData.Direccion.Strip(),
                                clientData.Movil, clientData.Email, clientData.Puntos, clientData.FechaNacimiento, clientData.Sexo, tipo, clientData.FechaAlta, clientData.Baja, clientData.Lopd);
                        }
                        else
                        {
                            fisiotesService.Clientes.Update(
                                clientData.Trabajador, clientData.Tarjeta, clientData.Nombre.Strip(), clientData.Telefono, clientData.Direccion.Strip(),
                                clientData.Movil, clientData.Email, clientData.Puntos, clientData.FechaNacimiento, clientData.Sexo, clientData.FechaAlta, clientData.Baja, clientData.Lopd, cliente.IDCLIENTE);
                        }

                        // Eliminamos el hueco del cliente.
                        fisiotesService.Huecos.Delete(hueco);
                    }
                }
            }
            catch (Exception e)
            {
                //Task.Delay(1500).Wait();
                Console.WriteLine(e.Message);
            }
        }

        //public void ProcessUpdateRecetasPendientes(FarmaticService farmaticService, FisiotesService fisiotesService)
        //{
        //    try
        //    {
        //        var puntos = fisiotesService.PuntosPendientes.GetOfRecetasPendientes();
        //        foreach (var punto in puntos)
        //        {
        //            var lineaVenta = farmaticService.Ventas.GetLineaVentaByKey(punto.idventa, punto.idnlinea);

        //            if (lineaVenta != null && (punto.recetaPendiente == null || lineaVenta.RecetaPendiente != "D"))
        //                fisiotesService.PuntosPendientes.Update(lineaVenta.RecetaPendiente, punto.idventa, punto.idnlinea);
        //            else if (punto.recetaPendiente == null)
        //                fisiotesService.PuntosPendientes.Update(punto.idventa, punto.idnlinea);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Task.Delay(1500).Wait();
        //    }
        //}

        //public void ProcessUpdateEntregasClientes(FarmaticService farmaticService, FisiotesService fisiotesService)
        //{
        //    const string FIELD_POR_DONDE_VOY_ENTREGAS_CLIENTES =
        //        FieldsConfiguracion.FIELD_POR_DONDE_VOY_ENTREGAS_CLIENTES;
        //    try
        //    {
        //        var cfg =
        //            fisiotesService.Configuraciones.GetByCampo(FIELD_POR_DONDE_VOY_ENTREGAS_CLIENTES);
        //        var venta = 0L;
        //        if (cfg == null)
        //        {
        //            fisiotesService.Configuraciones.Insert(FIELD_POR_DONDE_VOY_ENTREGAS_CLIENTES);
        //            var entregaAny = fisiotesService.Entregas.Last();
        //            if (entregaAny == null)
        //            {
        //                var pendiente = fisiotesService.PuntosPendientes.Last();
        //                venta = pendiente?.idventa ?? 0;
        //            }
        //            else venta = entregaAny.idventa;

        //            fisiotesService.Configuraciones.Update(FIELD_POR_DONDE_VOY_ENTREGAS_CLIENTES, venta.ToString());
        //        }
        //        else venta = Convert.ToInt64(cfg.valor);

        //        var ventasVirtuales = farmaticService.Ventas.GetVirtualesLessThanId(venta);
        //        foreach (var vtaVirtual in ventasVirtuales)
        //        {
        //            fisiotesService.Configuraciones.Update(FIELD_POR_DONDE_VOY_ENTREGAS_CLIENTES, vtaVirtual.IdVenta.ToString());
        //            var dni = vtaVirtual.XClie_IdCliente.Strip();
        //            if (string.IsNullOrEmpty(dni))
        //                dni = "0";

        //            var trabajador = GetNombreVendedorOrDefault(farmaticService, vtaVirtual.XVend_IdVendedor, "NO");
        //            var lineas = farmaticService.Ventas.GetLineasVirtualesByVenta(vtaVirtual.IdVenta);
        //            foreach (var linea in lineas)
        //            {
        //                var entrega = fisiotesService.Entregas.GetByKey(linea.IdVenta, linea.IdNLinea);
        //                if (entrega == null)
        //                    fisiotesService.Entregas.Insert(linea.IdVenta, linea.IdNLinea, linea.Codigo.Strip(),
        //                        linea.Descripcion.Strip(), linea.Cantidad, Convert.ToDecimal(linea.ImporteNeto), linea.TipoLinea,
        //                        Convert.ToInt32(vtaVirtual.FechaHora.ToString("yyyyMMdd")), dni, vtaVirtual.Maquina, trabajador,
        //                        vtaVirtual.FechaHora, Convert.ToSingle(linea.Pvp));
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Task.Delay(1500).Wait();
        //    }
        //}

        //public void ProcessUpdateProductosBorrados(FarmaticService farmaticService, FisiotesService fisiotesService)
        //{
        //    const string FIELD_POR_DONDE_VOY_BORRAR =
        //        FieldsConfiguracion.FIELD_POR_DONDE_VOY_BORRAR;
        //    try
        //    {
        //        var cfg = fisiotesService.Configuraciones.GetByCampo(FIELD_POR_DONDE_VOY_BORRAR);
        //        var codArticulo = "0";
        //        if (cfg == null)
        //            fisiotesService.Configuraciones.Insert(FIELD_POR_DONDE_VOY_BORRAR);
        //        else
        //            codArticulo = cfg.valor;

        //        var exsitWebInMedicamentos = fisiotesService.Medicamentos.HasWebField();
        //        var medicamentos =
        //            fisiotesService.Medicamentos.GetCodigosNacionalesGreaterOrEqual(codArticulo, exsitWebInMedicamentos);
        //        switch (medicamentos.Count)
        //        {
        //            case 0:
        //                fisiotesService.Configuraciones.Update(FIELD_POR_DONDE_VOY_BORRAR, "0");
        //                break;

        //            case 1:
        //                var articulo =
        //                    farmaticService.Articulos.GetById(medicamentos.First().PadLeft(6, '0'));
        //                if (articulo == null)
        //                    fisiotesService.Medicamentos.DeleteByCodigoNacional(medicamentos.First());
        //                fisiotesService.Configuraciones.Update(FIELD_POR_DONDE_VOY_BORRAR, "0");
        //                break;
        //        }

        //        foreach (var codNac in medicamentos)
        //        {
        //            var articulo = farmaticService.Articulos.GetById(codNac.PadLeft(6, '0'));
        //            if (articulo == null)
        //                fisiotesService.Medicamentos.DeleteByCodigoNacional(codNac);
        //            fisiotesService.Configuraciones.Update(FIELD_POR_DONDE_VOY_BORRAR, codNac);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Task.Delay(1500).Wait();
        //    }
        //}

        //public void ProcessUpdatePuntosPendientes(FarmaticService farmaticService, FisiotesService fisiotesService)
        //{
        //    try
        //    {
        //        var puntos = fisiotesService.PuntosPendientes.GetWithoutRedencion();
        //        foreach (var pto in puntos)
        //        {
        //            var venta = farmaticService.Ventas.GetById(pto.idventa);
        //            if (venta != null)
        //            {
        //                var lineas = farmaticService.Ventas.GetLineasVentaByVenta(venta.IdVenta);
        //                foreach (var linea in lineas)
        //                {
        //                    var lineaRedencion =
        //                        farmaticService.Ventas.GetLineaRedencionByKey(linea.IdVenta, linea.IdNLinea);
        //                    var redencion = lineaRedencion?.Redencion ?? 0;
        //                    var articulo = farmaticService.Articulos.GetById(linea.Codigo);
        //                    var proveedor = articulo != null
        //                        ? GetProveedorFromLocalOrDefault(farmaticService, articulo.ProveedorHabitual)
        //                        : string.Empty;
        //                    fisiotesService.PuntosPendientes.Update(venta.TipoVenta, proveedor,
        //                        Convert.ToSingle(linea.DescuentoLinea), Convert.ToSingle(venta.DescuentoOpera),
        //                        Convert.ToSingle(redencion), linea.IdVenta, linea.IdNLinea);
        //                }
        //            }
        //            else
        //            {
        //                fisiotesService.PuntosPendientes.Update(pto.idventa);
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Task.Delay(1500).Wait();
        //    }
        //}

        //private string GetProveedorFromLocalOrDefault(FarmaticService farmaticService, string proveedor, string byDefault = "")
        //{
        //    var proveedorDb = farmaticService.Proveedores.GetById(proveedor);
        //    return proveedorDb?.FIS_NOMBRE ?? byDefault;
        //}

        //public void ProcessSinonimos(FarmaticService farmaticService, FisiotesService fisiotesService)
        //{
        //    try
        //    {
        //        fisiotesService.Sinonimos.CheckAndAlterFields(_remoteBase);
        //        var sinonimo = fisiotesService.Sinonimos.First();
        //        var insertar = sinonimo == null || Array.Exists(new[] { "1230", "1730", "1930" }, x => x.Equals(DateTime.Now.ToString("HHmm")))
        //                ? true
        //                : false;

        //        if (insertar)
        //        {
        //            fisiotesService.Sinonimos.Truncate();
        //            var sinonimos = farmaticService.Sinonimos.Get();

        //            var take = 1000;
        //            for (int i = 0; i < sinonimos.Count; i += take)
        //            {
        //                var items = sinonimos.Skip(i).Take(1000).Select(x => new Sinonimo
        //                {
        //                    cod_barras = x.Sinonimo.Strip(),
        //                    cod_nacional = x.IdArticu.Strip()
        //                }).ToList();
        //                fisiotesService.Sinonimos.Insert(items);
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Task.Delay(1500).Wait();
        //    }
        //}

        //public void ProcessControlSinStockInicial(FarmaticService farmaticService, ConsejoService consejoService, FisiotesService fisiotesService)
        //{
        //    const string FIELD_POR_DONDE_VOY_SIN_STOCK = FieldsConfiguracion.FIELD_POR_DONDE_VOY_SIN_STOCK;
        //    try
        //    {
        //        var configuracion = fisiotesService.Configuraciones.GetByCampo(FIELD_POR_DONDE_VOY_SIN_STOCK);
        //        if (configuracion == null)
        //            fisiotesService.Configuraciones.Insert(FIELD_POR_DONDE_VOY_SIN_STOCK);
        //        var codArticulo = configuracion != null ? configuracion.valor : "0";
        //        var articulos = farmaticService.Articulos.GetWithoutStockByIdGreaterOrEqual(codArticulo);
        //        foreach (var articulo in articulos)
        //        {
        //            SyncUpArticuloWithIva(farmaticService, consejoService, fisiotesService, articulo, FIELD_POR_DONDE_VOY_SIN_STOCK, articulo.IdArticu);
        //        }
        //        fisiotesService.Configuraciones.Update(FIELD_POR_DONDE_VOY_SIN_STOCK, "0");
        //        fisiotesService.Medicamentos.ResetPorDondeVoySinStock();
        //    }
        //    catch (Exception e)
        //    {
        //        Task.Delay(1500).Wait();
        //    }
        //}

        //private void SyncUpArticuloWithIva(FarmaticService farmaticService, ConsejoService consejoService, FisiotesService fisiotesService, ArticuloWithIva articulo, string updatefield, string updateValue)
        //{
        //    fisiotesService.Configuraciones.Update(updatefield, updateValue);

        //    var familila = farmaticService.Familias.GetById(articulo.XFam_IdFamilia);
        //    var fechaUltimaCompra = articulo.FechaUltimaEntrada;
        //    var fechaUltimaVenta = articulo.FechaUltimaSalida;
        //    var precio = articulo.Pvp;
        //    var pcoste = articulo.Puc;
        //    var pvpsIva = Math.Round(articulo.Pvp * 100 / (articulo.Iva + 100), 2);
        //    var stock = articulo.StockActual;
        //    var stockMinimo = articulo.StockMinimo;
        //    var stockMaximo = articulo.StockMaximo;
        //    var desc = articulo.Descripcion.Strip();

        //    var superFamilia = !string.IsNullOrEmpty(familila?.Descripcion)
        //            ? GetSuperFamiliaFromLocalOrDefault(farmaticService, familila.Descripcion)
        //            : string.Empty;

        //    var codigoBarra = GetCodidoBarrasFromLocalOrDefault(farmaticService, articulo.IdArticu);
        //    var proveedor = GetProveedorFromLocalOrDefault(farmaticService, articulo.ProveedorHabitual);
        //    var nombreLaboratorio = GetNombreLaboratorioFromLocalOrDefault(farmaticService, consejoService, articulo.Laboratorio, "<Sin Laboratorio>");

        //    var espepara = consejoService.Esperas.Get(articulo.IdArticu);
        //    var present = espepara?.PRESENTACION ?? string.Empty;

        //    var descripcionHtml = string.Empty;
        //    var textos = consejoService.Esperas.GetTextos(articulo.IdArticu);
        //    foreach (var texto in textos)
        //    {
        //        if (string.IsNullOrEmpty(descripcionHtml))
        //            descripcionHtml = texto;
        //        descripcionHtml += $@" <br> {texto}";
        //    }
        //    descripcionHtml = descripcionHtml.Length < 30000
        //        ? descripcionHtml.Replace(Environment.NewLine, "<br>").Replace("\0", string.Empty).Strip()
        //        : string.Empty;

        //    var baja = articulo.Baja;
        //    var activo = !baja;

        //    var fechaCaducidad = articulo.FechaCaducidad;

        //    var medicamento = fisiotesService.Medicamentos.GetByCodNacional(articulo.IdArticu);
        //    if (medicamento == null)
        //        fisiotesService.Medicamentos.Insert(codigoBarra.Strip(), articulo.IdArticu.Strip(), desc.Strip(), superFamilia.Strip(),
        //                familila?.Descripcion.Strip(), Convert.ToSingle(precio), desc.Strip(), articulo.Laboratorio.Strip(), nombreLaboratorio.Strip(),
        //                proveedor.Strip(), Convert.ToSingle(pvpsIva), Convert.ToInt32(articulo.Iva), stock, Convert.ToSingle(pcoste), stockMinimo, stockMaximo, present.Strip(),
        //                descripcionHtml.Strip(), activo, fechaCaducidad, fechaUltimaCompra, fechaUltimaVenta, baja);
        //    else
        //    {
        //        if (desc.Strip() != medicamento.nombre || precio != medicamento.precio ||
        //            articulo.Laboratorio != medicamento.laboratorio || articulo.Iva != medicamento.iva ||
        //            stock != medicamento.stock || present.Strip() != medicamento.presentacion ||
        //            descripcionHtml != medicamento.descripcion)
        //            fisiotesService.Medicamentos.Update(codigoBarra.Strip(), desc.Strip(), superFamilia.Strip(),
        //                familila?.Descripcion.Strip(), Convert.ToSingle(precio), desc.Strip(), articulo.Laboratorio.Strip(), nombreLaboratorio.Strip(),
        //                proveedor.Strip(), Convert.ToInt32(articulo.Iva), Convert.ToSingle(pvpsIva), stock, Convert.ToSingle(pcoste), stockMinimo, stockMaximo, present.Strip(),
        //                descripcionHtml.Strip(), activo, fechaCaducidad, fechaUltimaCompra, fechaUltimaVenta, baja, articulo.IdArticu,
        //                withSqlExtra: true);
        //        else
        //            fisiotesService.Medicamentos.Update(codigoBarra.Strip(), desc.Strip(), superFamilia.Strip(),
        //                familila?.Descripcion.Strip(), Convert.ToSingle(precio), desc.Strip(), articulo.Laboratorio.Strip(), nombreLaboratorio.Strip(),
        //                proveedor.Strip(), Convert.ToInt32(articulo.Iva), Convert.ToSingle(pvpsIva), stock, Convert.ToSingle(pcoste), stockMinimo, stockMaximo, present.Strip(),
        //                descripcionHtml.Strip(), activo, fechaCaducidad, fechaUltimaCompra, fechaUltimaVenta, baja, articulo.IdArticu);
        //    }
        //}

        //private string GetSuperFamiliaFromLocalOrDefault(FarmaticService farmaticService, string familia, string byDefault = "")
        //{
        //    return farmaticService.Familias.GetSuperFamiliaDescripcionByFamilia(familia) ?? byDefault;
        //}

        //private string GetCodidoBarrasFromLocalOrDefault(FarmaticService farmaticService, string articulo)
        //{
        //    var sinonimo = farmaticService.Sinonimos.GetByArticulo(articulo);
        //    return sinonimo?.Sinonimo ?? "847000".PadLeft(6, '0');
        //}

        //private string GetNombreLaboratorioFromLocalOrDefault(FarmaticService farmaticService, ConsejoService consejoService, string codigo, string byDefault = "")
        //{
        //    var nombreLaboratorio = byDefault;
        //    if (!string.IsNullOrEmpty(codigo?.Trim()) && !string.IsNullOrWhiteSpace(codigo))
        //    {
        //        var laboratorioDb = consejoService.Laboratorios.Get(codigo);
        //        if (laboratorioDb == null)
        //        {
        //            var laboratorioLocal =
        //                farmaticService.Laboratorios.GetById(codigo);
        //            nombreLaboratorio = laboratorioLocal?.Nombre ?? byDefault;
        //        }
        //        else nombreLaboratorio = laboratorioDb.NOMBRE;
        //    }
        //    else nombreLaboratorio = byDefault;
        //    return nombreLaboratorio;
        //}

        //public void ProcessControlStockFechasSalida(FarmaticService farmaticService, ConsejoService consejoService, FisiotesService fisiotesService)
        //{
        //    const string FIELD_NAME = FieldsConfiguracion.FIELD_STOCK_SALIDA;
        //    try
        //    {
        //        DateTime? fechaActualizacionStock = null;
        //        var configuracion = fisiotesService.Configuraciones.GetByCampo(FIELD_NAME);
        //        if (configuracion == null)
        //            fisiotesService.Configuraciones.Insert(FIELD_NAME);
        //        else
        //            fechaActualizacionStock = CalculateFechaActualizacion(configuracion.valor);

        //        // Recuperamos artículos de la base local junto al iva
        //        var articulosWithIva = farmaticService.Articulos.GetByFechaUltimaSalidaGreaterOrEqual(fechaActualizacionStock);
        //        foreach (var articulo in articulosWithIva)
        //        {
        //            var strFecha = articulo.FechaUltimaSalida?.ToString("yyyy-MM-dd");
        //            SyncUpArticuloWithIva(farmaticService, consejoService, fisiotesService, articulo, FIELD_NAME, strFecha);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Task.Delay(1500).Wait();
        //    }
        //}

        //private static DateTime CalculateFechaActualizacion(string fecha)
        //{
        //    try
        //    {
        //        return string.IsNullOrEmpty(fecha) || string.IsNullOrWhiteSpace(fecha)
        //            ? DateTime.Now.AddDays(-7)
        //            : (DateTime.Now - DateTime.ParseExact(fecha, "yyyy-dd-MM", CultureInfo.InvariantCulture)).TotalDays > 7
        //            ? DateTime.Now.AddDays(-7)
        //            : DateTime.ParseExact(fecha, "yyyy-dd-MM", CultureInfo.InvariantCulture);
        //    }
        //    catch (Exception)
        //    {
        //        return DateTime.ParseExact(fecha, "yyyy-dd-MM", CultureInfo.InvariantCulture);
        //    }
        //}

        //public void ProcessControlStockInicial(FarmaticService farmaticService, ConsejoService consejoService, FisiotesService fisiotesService)
        //{
        //    const string FIELD_STOCK_ENTRADA = FieldsConfiguracion.FIELD_STOCK_ENTRADA;
        //    const string FIELD_STOCK_SALIDA = FieldsConfiguracion.FIELD_STOCK_SALIDA;
        //    const string FIELD_POR_DONDE_VOY_CON_STOCK = FieldsConfiguracion.FIELD_POR_DONDE_VOY_CON_STOCK;
        //    const string FIELD_POR_DONDE_VOY_SIN_STOCK = FieldsConfiguracion.FIELD_POR_DONDE_VOY_SIN_STOCK;
        //    try
        //    {
        //        fisiotesService.Medicamentos.CheckAndCreateFields();
        //        var configuracion = fisiotesService.Configuraciones.GetByCampo(FIELD_STOCK_ENTRADA);
        //        if (configuracion == null)
        //            fisiotesService.Configuraciones.Insert(FIELD_STOCK_ENTRADA);
        //        configuracion = fisiotesService.Configuraciones.GetByCampo(FIELD_STOCK_SALIDA);
        //        if (configuracion == null)
        //            fisiotesService.Configuraciones.Insert(FIELD_STOCK_SALIDA);
        //        configuracion = fisiotesService.Configuraciones.GetByCampo(FIELD_POR_DONDE_VOY_CON_STOCK);
        //        if (configuracion == null)
        //        {
        //            fisiotesService.Configuraciones.Insert(FIELD_POR_DONDE_VOY_CON_STOCK);
        //            fisiotesService.Configuraciones.Insert(FIELD_POR_DONDE_VOY_SIN_STOCK);
        //        }
        //        var codArticulo = configuracion != null ? configuracion.valor : "0";
        //        var articulos = farmaticService.Articulos.GetWithStockByIdGreaterOrEqual(codArticulo);
        //        foreach (var articulo in articulos)
        //        {
        //            SyncUpArticuloWithIva(farmaticService, consejoService, fisiotesService, articulo, FIELD_POR_DONDE_VOY_CON_STOCK, articulo.IdArticu);
        //        }
        //        fisiotesService.Configuraciones.Update(FIELD_POR_DONDE_VOY_CON_STOCK, "0");
        //        fisiotesService.Medicamentos.ResetPorDondeVoy();
        //    }
        //    catch (Exception e)
        //    {
        //        Task.Delay(1500).Wait();
        //    }
        //}

        //public void ProcessControlStockFechasEntrada(FarmaticService farmaticService, ConsejoService consejoService, FisiotesService fisiotesService)
        //{
        //    const string FIELD_NAME = FieldsConfiguracion.FIELD_STOCK_ENTRADA;
        //    try
        //    {
        //        // Estblecemos la fecha de actualización del stock
        //        DateTime? fechaActualizacionStock = null;
        //        var configuracion = fisiotesService.Configuraciones.GetByCampo(FIELD_NAME);
        //        if (configuracion == null)
        //            fisiotesService.Configuraciones.Insert(FIELD_NAME);
        //        else
        //            fechaActualizacionStock = CalculateFechaActualizacion(configuracion.valor);

        //        // Recuperamos artículos de la base local junto al iva
        //        var articulosWithIva = farmaticService.Articulos.GetByFechaUltimaEntradaGreaterOrEqual(fechaActualizacionStock);
        //        foreach (var articulo in articulosWithIva)
        //        {
        //            var strFecha = articulo.FechaUltimaEntrada?.ToString("yyyy-MM-dd");
        //            SyncUpArticuloWithIva(farmaticService, consejoService, fisiotesService, articulo, FIELD_NAME, strFecha);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Task.Delay(1500).Wait();
        //    }
        //}

        //public void ProcessPuntosPendientes(FarmaticService farmatic, ConsejoService consejo, FisiotesService fisiotes)
        //{
        //    try
        //    {
        //        // Validamos los campos existentes en puntos_pendientes
        //        fisiotes.PuntosPendientes.CheckAndCreateFields();

        //        // Creamos la tabla entregas_clientes en la BD remota, si no existe
        //        fisiotes.Entregas.CreateTable(_remoteBase);

        //        // Validamos los campos existentes en ClienteAux
        //        var existFieldSexo = farmatic.Clientes.HasSexoField();

        //        // Verificamos el campo tipoPago en pendientes_puntos en la BD remota
        //        fisiotes.PuntosPendientes.CheckTipoPagoField(_remoteBase);

        //        // Recuperamos los puntos pendientes de la última venta y establecemos el ID de venta
        //        var puntosPendientes = fisiotes.PuntosPendientes.Last();
        //        var idVenta = puntosPendientes?.idventa ?? 1L;

        //        // Recuperamos las ventas locales
        //        var ventas = farmatic.Ventas.GetByIdGreaterOrEqual(idVenta);
        //        foreach (var venta in ventas)
        //        {
        //            // Establecemos los valores que necesitamos desde la venta extrída de
        //            // la BD local.
        //            var puesto = venta.Maquina;
        //            var tipoPago = venta.TipoVenta;
        //            var fechaVenta = venta.FechaHora;
        //            var dni = venta.XClie_IdCliente.Strip();
        //            if (!string.IsNullOrEmpty(dni))
        //            {
        //                var cliente = farmatic.Clientes.GetById(dni);
        //                // Extraemos los datos necesarios del cliente local para sincronizar con el remoto
        //                var clientData = FetchLocalClienteData(farmatic, cliente, existFieldSexo);

        //                // Recuperamos el cliente remoto que coincida con el cliente local
        //                if (!fisiotes.Clientes.AnyWithDni(cliente.IDCLIENTE))
        //                {
        //                    var tipo = "cliente";
        //                    fisiotes.Clientes.Insert(
        //                        clientData.Trabajador, clientData.Tarjeta, cliente.IDCLIENTE, clientData.Nombre.Strip(), clientData.Telefono, clientData.Direccion.Strip(),
        //                        clientData.Movil, clientData.Email, clientData.Puntos, clientData.FechaNacimiento, clientData.Sexo, tipo, clientData.FechaAlta, clientData.Baja, clientData.Lopd);
        //                }
        //                else
        //                {
        //                    fisiotes.Clientes.Update(
        //                        clientData.Trabajador, clientData.Tarjeta, clientData.Nombre.Strip(), clientData.Telefono, clientData.Direccion.Strip(),
        //                        clientData.Movil, clientData.Email, clientData.Puntos, clientData.FechaNacimiento, clientData.Sexo, clientData.FechaAlta, clientData.Baja, clientData.Lopd, cliente.IDCLIENTE);
        //                }
        //            }
        //            // TODO: dni = 0 pero no se usa

        //            // Recuramos el vendedor de la venta desde la BD local y establecemos
        //            // el trabajador
        //            var idVendendor = venta.XVend_IdVendedor;
        //            var vendedor = farmatic.Vendedores.GetById(idVendendor);
        //            var trabajador = vendedor?.NOMBRE ?? "NO";

        //            // Recuperamos la la datos de la venta y el detalle de la misma
        //            var fecha = Convert.ToInt32(venta.FechaHora.ToString("yyyyMMdd"));
        //            var descuentoVenta = false;
        //            var detalleVenta = farmatic.Ventas.GetLineasVentaByVenta(venta.IdVenta);
        //            foreach (var linea in detalleVenta)
        //            {
        //                var recetaPendiente = linea.RecetaPendiente;
        //                var receta = linea.TipoAportacion;
        //                var precioMed = linea.PVP;

        //                var dtoVta = 0d;
        //                if (!descuentoVenta)
        //                {
        //                    dtoVta = (venta.DescuentoOpera ?? 0d);
        //                    descuentoVenta = true;
        //                }
        //                var dtoLinea = venta.DescuentoLinea ?? 0d;

        //                // Establecemos redencion
        //                var redencionDb =
        //                    farmatic.Ventas.GetLineaRedencionByKey(venta.IdVenta, linea.IdNLinea);
        //                var redencion = redencionDb?.Redencion ?? 0;

        //                var codigoBarra = GetCodidoBarrasFromLocalOrDefault(farmatic, linea.Codigo);

        //                // Recuperamos el artículo para establcer los siguientes datos
        //                var familia = string.Empty;
        //                var superFamilia = string.Empty;
        //                var codLaboratorio = string.Empty;
        //                var nombreLaboratorio = string.Empty;
        //                var proveedor = string.Empty;
        //                var pcoste = 0d;

        //                var articulo = farmatic.Articulos.GetById(linea.Codigo);
        //                if (articulo == null)
        //                    nombreLaboratorio = "<Sin Laboratorio>";
        //                else
        //                {
        //                    pcoste = articulo.Puc;
        //                    familia = GetFamiliaFromLocalOrDefault(farmatic, articulo.XFam_IdFamilia, "<Sin Clasificar>");

        //                    codLaboratorio = articulo.Laboratorio ?? string.Empty;

        //                    superFamilia = familia.Equals("<Sin Clasificar>")
        //                        ? superFamilia = familia
        //                        : GetSuperFamiliaFromLocalOrDefault(farmatic, familia, "<Sin Clasificar>");

        //                    proveedor = GetProveedorFromLocalOrDefault(farmatic, articulo.ProveedorHabitual);
        //                    nombreLaboratorio = GetNombreLaboratorioFromLocalOrDefault(farmatic, consejo, codLaboratorio, "<Sin Laboratorio>");
        //                }

        //                // Verificamos si hay puntos pendientes desde la base remota, para el item de venta actual
        //                var puntos =
        //                    fisiotes.PuntosPendientes.GetByItemVenta(venta.IdVenta, linea.IdNLinea);
        //                if (puntos == null)
        //                {
        //                    // Si no hay, lo insertamos
        //                    var numero = linea.ImporteNeto;
        //                    var cargado = "no";
        //                    fisiotes.PuntosPendientes.Insert(venta.IdVenta, linea.IdNLinea, codigoBarra, linea.Codigo,
        //                        linea.Descripcion.Strip(), familia.Strip(), linea.Cantidad, Convert.ToDecimal(numero),
        //                        tipoPago, fecha, dni, cargado, puesto, trabajador, codLaboratorio.Strip(), nombreLaboratorio.Strip(),
        //                        proveedor.Strip(), receta, fechaVenta, superFamilia.Strip(), Convert.ToSingle(precioMed),
        //                        Convert.ToSingle(pcoste), Convert.ToSingle(dtoLinea), Convert.ToSingle(dtoVta), Convert.ToSingle(redencion), recetaPendiente);
        //                }

        //                // Verificamos el idVenta
        //                if (venta.IdVenta > idVenta)
        //                    idVenta = venta.IdVenta - 3;
        //            }

        //            // Recuperamos el detalle de ventas virtuales
        //            var virtuales = farmatic.Ventas.GetLineasVirtualesByVenta(venta.IdVenta);
        //            foreach (var lineaVirtual in virtuales)
        //            {
        //                // Verificamos la entrega del item de venta
        //                var entrega = fisiotes.Entregas.GetByKey(venta.IdVenta, lineaVirtual.IdNLinea);
        //                if (entrega == null)
        //                {
        //                    var numero = lineaVirtual.ImporteNeto;
        //                    var pvp = lineaVirtual.Pvp;
        //                    fisiotes.Entregas.Insert(venta.IdVenta, lineaVirtual.IdNLinea, lineaVirtual.Codigo.Strip(),
        //                        lineaVirtual.Descripcion.Strip(), lineaVirtual.Cantidad, Convert.ToDecimal(numero), lineaVirtual.TipoLinea, fecha, dni, puesto,
        //                        trabajador, fechaVenta, Convert.ToSingle(pvp));
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Task.Delay(1500).Wait();
        //    }
        //}

        //private string GetFamiliaFromLocalOrDefault(FarmaticService farmatic, short id, string byDefault = "")
        //{
        //    var familiaDb = farmatic.Familias.GetById(id);
        //    return !string.IsNullOrEmpty(familiaDb?.Descripcion)
        //        ? familiaDb.Descripcion
        //        : byDefault;
        //}

        //public void ProcessPedidos(FarmaticService farmatic, ConsejoService consejo, FisiotesService fisiotes)
        //{
        //    try
        //    {
        //        fisiotes.Pedidos.CreateTable(_remoteBase);
        //        fisiotes.Pedidos.CheckAndCreateFechaPedidoField();

        //        var anyPedido = fisiotes.Pedidos.Last();
        //        var recepciones = anyPedido == null
        //            ? farmatic.Recepciones.GetByYear()
        //            : farmatic.Recepciones.GetByIdAndYear(anyPedido.idPedido);

        //        foreach (var recepcion in recepciones)
        //        {
        //            var proveedor = GetProveedorFromLocalOrDefault(farmatic, recepcion.XProv_IdProveedor);
        //            var trabajador = GetNombreVendedorOrDefault(farmatic, recepcion.XVend_IdVendedor);

        //            var resume = farmatic.Recepciones.GetResumeById(recepcion.IdRecepcion);
        //            var pedido = fisiotes.Pedidos.Get(recepcion.IdRecepcion);

        //            if (pedido == null && resume.numLineas > 0)
        //                fisiotes.Pedidos.Insert(recepcion.IdRecepcion, recepcion.Hora, DateTime.Now,
        //                    resume.numLineas, Convert.ToSingle(resume.importePvp), Convert.ToSingle(resume.importePuc),
        //                    recepcion.XProv_IdProveedor, proveedor, trabajador);

        //            if (resume.numLineas > 0)
        //            {
        //                var lineas = farmatic.Recepciones.GetLineasById(recepcion.IdRecepcion);
        //                foreach (var linea in lineas)
        //                {
        //                    if (!string.IsNullOrEmpty(linea.XArt_IdArticu?.Trim()))
        //                    {
        //                        var familia = string.Empty;
        //                        var superFamilia = string.Empty;
        //                        var codLaboratorio = string.Empty;
        //                        var nombreLaboratorio = string.Empty;
        //                        var pvp = 0d;
        //                        var puc = 0d;

        //                        var articulo = farmatic.Articulos.GetById(linea.XArt_IdArticu);
        //                        if (articulo == null)
        //                            nombreLaboratorio = "<Sin Laboratorio>";
        //                        else
        //                        {
        //                            puc = articulo.Puc;
        //                            pvp = articulo.Pvp;

        //                            familia = GetFamiliaFromLocalOrDefault(farmatic, articulo.XFam_IdFamilia, "<Sin Clasificar>");
        //                            superFamilia = familia.Equals("<Sin Clasificar>")
        //                                ? superFamilia = familia
        //                                : GetSuperFamiliaFromLocalOrDefault(farmatic, familia, "<Sin Clasificar>");

        //                            codLaboratorio = articulo.Laboratorio ?? string.Empty;
        //                            nombreLaboratorio = GetNombreLaboratorioFromLocalOrDefault(farmatic, consejo, codLaboratorio, "<Sin Laboratorio>");
        //                        }

        //                        var lineaPedido = fisiotes.Pedidos.GetLineaByKey(linea.IdRecepcion, linea.IdNLinea);
        //                        if (lineaPedido == null && articulo != null)
        //                            fisiotes.Pedidos.InsertLinea(recepcion.Hora, linea.IdRecepcion, linea.IdNLinea,
        //                                articulo.IdArticu.Strip(), articulo.Descripcion.Strip(), familia.Strip(),
        //                                superFamilia.Strip(), linea.Recibidas, Convert.ToSingle(pvp), Convert.ToSingle(puc),
        //                                codLaboratorio.Strip(), nombreLaboratorio.Strip());
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Task.Delay(1500).Wait();
        //    }
        //}

        //public void ProcessListaTienda(FarmaticService farmatic, ConsejoService consejo, FisiotesService fisiotes)
        //{
        //    try
        //    {
        //        if (_marketCodeList > 0)
        //        {
        //            var lista = farmatic.ListasArticulos.Get(_marketCodeList);
        //            if (lista != null)
        //            {
        //                var listaRemote = fisiotes.Listas.Get(lista.IdLista);
        //                if (listaRemote == null)
        //                    fisiotes.Listas.Insert(lista.IdLista, lista.Descripcion.Strip());
        //                else
        //                    fisiotes.Listas.Update(lista.IdLista, lista.Descripcion.Strip());

        //                fisiotes.Listas.DeArticulos.Delete(lista.IdLista);
        //                var articulos = farmatic.ListasArticulos.GetArticulosByLista(lista.IdLista);
        //                foreach (var articulo in articulos)
        //                {
        //                    fisiotes.Listas.DeArticulos.Insert(articulo.XItem_IdLista, Convert.ToInt32(articulo.XItem_IdArticu));
        //                    var awi = farmatic.ListasArticulos.GetArticuloWithIva(_marketCodeList, articulo.XItem_IdArticu);

        //                    var precio = awi.Pvp;
        //                    var pcoste = awi.Puc;
        //                    var pvpSinIva = Math.Round(precio * 100 / (awi.Iva + 100), 2);
        //                    var stock = awi.StockActual;
        //                    var stockMinimo = awi.StockMaximo;
        //                    var stockMaximo = awi.StockMaximo;
        //                    var desc = awi.Descripcion.Strip();
        //                    var familia = farmatic.Familias.GetById(awi.XFam_IdFamilia);
        //                    var superFamilia = string.IsNullOrEmpty(familia?.Descripcion)
        //                            ? string.Empty
        //                            : GetSuperFamiliaFromLocalOrDefault(farmatic, familia.Descripcion);
        //                    var codigoBarras = GetCodidoBarrasFromLocalOrDefault(farmatic, awi.IdArticu);
        //                    var proveedor = GetProveedorFromLocalOrDefault(farmatic, awi.ProveedorHabitual);
        //                    var nombreLaboratorio = GetNombreLaboratorioFromLocalOrDefault(farmatic, consejo, awi.Laboratorio, "<Sin Laboratorio>");

        //                    var espepara = consejo.Esperas.Get(awi.IdArticu);
        //                    var present = espepara?.PRESENTACION ?? string.Empty;

        //                    var descripcionHtml = string.Empty;
        //                    var textos = consejo.Esperas.GetTextos(awi.IdArticu);
        //                    foreach (var texto in textos)
        //                    {
        //                        if (string.IsNullOrEmpty(descripcionHtml))
        //                            descripcionHtml = texto;
        //                        descripcionHtml += $@" <br> {texto}";
        //                    }
        //                    descripcionHtml = descripcionHtml.Length < 30000
        //                        ? descripcionHtml.Replace(Environment.NewLine, "<br>").Replace("\0", string.Empty).Strip()
        //                        : string.Empty;

        //                    var baja = awi.Baja;
        //                    var activo = !baja;
        //                    var medicamento = fisiotes.Medicamentos.GetByCodNacional(awi.IdArticu);
        //                    if (medicamento == null)
        //                        fisiotes.Medicamentos.Insert(codigoBarras.Strip(), awi.IdArticu.Strip(), desc.Strip(), superFamilia.Strip(),
        //                                familia?.Descripcion.Strip(), Convert.ToSingle(precio), desc.Strip(), awi.Laboratorio.Strip(), nombreLaboratorio.Strip(),
        //                                proveedor.Strip(), Convert.ToSingle(pvpSinIva), Convert.ToInt32(awi.Iva), stock, Convert.ToSingle(pcoste), stockMinimo, stockMaximo, present.Strip(),
        //                                descripcionHtml.Strip(), activo, baja);
        //                    else
        //                    {
        //                        if (desc.Strip() != medicamento.nombre || precio != medicamento.precio || familia?.Descripcion.Strip() != medicamento.familia ||
        //                            awi.Laboratorio != medicamento.laboratorio || awi.Iva != medicamento.iva || pcoste != medicamento.puc ||
        //                            stock != medicamento.stock || present.Strip() != medicamento.presentacion ||
        //                            descripcionHtml != medicamento.descripcion)
        //                            fisiotes.Medicamentos.Update(codigoBarras.Strip(), desc.Strip(), superFamilia.Strip(),
        //                                familia?.Descripcion.Strip(), Convert.ToSingle(precio), desc.Strip(), awi.Laboratorio.Strip(), nombreLaboratorio.Strip(),
        //                                proveedor.Strip(), Convert.ToInt32(awi.Iva), Convert.ToSingle(pvpSinIva), stock, Convert.ToSingle(pcoste), stockMinimo, stockMaximo, present.Strip(),
        //                                descripcionHtml.Strip(), activo, baja, awi.IdArticu);
        //                    }
        //                }
        //                farmatic.ListasArticulos.Update(_marketCodeList);
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Task.Delay(1500).Wait();
        //    }
        //}

        //public void ProcessCategorias(FarmaticService farmatic, FisiotesService fisiotes)
        //{
        //    try
        //    {
        //        var familias = farmatic.Familias.GetByDescripcion();
        //        foreach (var familia in familias)
        //        {
        //            var padre = GetPadreFromLocalOrDefault(farmatic, familia.IdFamilia, @"<SIN PADRE>");
        //            var categoria = fisiotes.Categorias.GetByCategoriaAndPadre(familia.Descripcion.Strip(), padre.Strip());
        //            if (categoria == null)
        //            {
        //                var categoriaOnlyPadre = fisiotes.Categorias.GetByPadre(padre.Strip());
        //                var padreId = categoriaOnlyPadre?.prestashopPadreId;
        //                fisiotes.Categorias.Insert(familia.Descripcion.Strip(), padre.Strip(), padreId);
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Task.Delay(1500).Wait();
        //    }
        //}

        //private string GetPadreFromLocalOrDefault(FarmaticService farmatic, short familia, string byDefault = "")
        //{
        //    return farmatic.Familias.GetSuperFamiliaDescripcionById(familia) ?? byDefault;
        //}

        //public void ProcessListasFechas(FarmaticService farmatic, FisiotesService fisiotes)
        //{
        //    try
        //    {
        //        var listas = farmatic.ListasArticulos.GetByFechaExceptList(_marketCodeList);
        //        foreach (var lista in listas)
        //        {
        //            var listaRemote = fisiotes.Listas.Get(lista.IdLista);
        //            if (listaRemote == null)
        //                fisiotes.Listas.Insert(lista.IdLista, lista.Descripcion.Strip());
        //            else
        //                fisiotes.Listas.Update(lista.IdLista, lista.Descripcion.Strip());

        //            fisiotes.Listas.DeArticulos.Delete(lista.IdLista);
        //            var articulos = farmatic.ListasArticulos.GetArticulosByLista(lista.IdLista);
        //            foreach (var articulo in articulos)
        //            {
        //                fisiotes.Listas.DeArticulos.Insert(articulo.XItem_IdLista, Convert.ToInt32(articulo.XItem_IdArticu));
        //            }
        //            if (articulos.Count != 0)
        //            {
        //                var take = 1000;
        //                for (int i = 0; i < articulos.Count; i += take)
        //                {
        //                    var items = articulos.Skip(i).Take(1000).Select(x => new Fisiotes.Models.ListaArticulo
        //                    {
        //                        cod_lista = x.XItem_IdLista,
        //                        cod_articulo = Convert.ToInt32(x.XItem_IdArticu)
        //                    }).ToList();
        //                    fisiotes.Listas.DeArticulos.Insert(items);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Task.Delay(1500).Wait();
        //    }
        //}

        //public void ProcessListas(FarmaticService farmatic, FisiotesService fisiotes)
        //{
        //    try
        //    {
        //        fisiotes.Listas.CheckAndCreatePorDondeVoyField();
        //        var codLista = fisiotes.Listas.GetCodPorDondeVoy();
        //        var listas = farmatic.ListasArticulos.GetByIdGreaterThan(codLista);
        //        foreach (var lista in listas)
        //        {
        //            var listaRemote = fisiotes.Listas.Get(lista.IdLista);
        //            fisiotes.Listas.ResetPorDondeVoy();
        //            if (listaRemote == null)
        //                fisiotes.Listas.InsertWithPorDondeVoy(lista.IdLista, lista.Descripcion.Strip());
        //            else
        //                fisiotes.Listas.UpdateWithPorDondeVoy(lista.IdLista, lista.Descripcion.Strip());

        //            var articulos = farmatic.ListasArticulos.GetArticulosByLista(lista.IdLista);
        //            if (articulos.Count != 0)
        //            {
        //                fisiotes.Listas.DeArticulos.Delete(lista.IdLista);
        //                var take = 1000;
        //                for (int i = 0; i < articulos.Count; i += take)
        //                {
        //                    var items = articulos.Skip(i).Take(1000).Select(x => new Fisiotes.Models.ListaArticulo
        //                    {
        //                        cod_lista = x.XItem_IdLista,
        //                        cod_articulo = Convert.ToInt32(x.XItem_IdArticu)
        //                    }).ToList();
        //                    fisiotes.Listas.DeArticulos.Insert(items);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Task.Delay(1500).Wait();
        //    }
        //}

        //public void ProcessEncargos(FarmaticService farmatic, ConsejoService consejo, FisiotesService fisiotes)
        //{
        //    try
        //    {
        //        fisiotes.Encargos.CheckAndCreateProveedorField();
        //        var encargoRemote = fisiotes.Encargos.Last();
        //        var idEncargo = encargoRemote != null ? encargoRemote.idEncargo : 1;

        //        var encargos = farmatic.Encargos.GetByContadorGreaterOrEqual(idEncargo);
        //        foreach (var encargo in encargos)
        //        {
        //            var codNacional = encargo.XArt_IdArticu?.Trim();
        //            var cliente = !string.IsNullOrEmpty(encargo.XCli_IdCliente)
        //                ? encargo.XCli_IdCliente.Trim()
        //                : "0";

        //            // Recuperamos el artículo para establcer los siguientes datos
        //            var nombre = string.Empty;
        //            var familia = string.Empty;
        //            var superFamilia = string.Empty;
        //            var codLaboratorio = string.Empty;
        //            var nombreLaboratorio = string.Empty;
        //            var proveedor = string.Empty;
        //            var pcoste = 0d;
        //            var precioMed = 0d;

        //            var articulo = farmatic.Articulos.GetById(encargo.XArt_IdArticu);
        //            if (articulo == null)
        //                nombreLaboratorio = "<Sin Laboratorio>";
        //            else
        //            {
        //                nombre = articulo.Descripcion.Strip();
        //                pcoste = articulo.Puc;
        //                precioMed = articulo.Pvp;
        //                familia = GetFamiliaFromLocalOrDefault(farmatic, articulo.XFam_IdFamilia, "<Sin Clasificar>");
        //                superFamilia = familia.Equals("<Sin Clasificar>")
        //                    ? familia
        //                    : GetSuperFamiliaFromLocalOrDefault(farmatic, familia, "<Sin Clasificar>");
        //                proveedor = GetProveedorFromLocalOrDefault(farmatic, articulo.ProveedorHabitual);
        //                codLaboratorio = articulo.Laboratorio ?? string.Empty;
        //                nombreLaboratorio = GetNombreLaboratorioFromLocalOrDefault(farmatic, consejo, codLaboratorio, "<Sin Laboratorio>");
        //            }

        //            var trabajador = GetNombreVendedorOrDefault(farmatic, encargo.Vendedor);
        //            encargoRemote = fisiotes.Encargos.Get(encargo.IdContador);

        //            if (encargoRemote == null)
        //                fisiotes.Encargos.Insert(encargo.IdContador, codNacional, nombre, superFamilia.Strip(),
        //                    familia, codLaboratorio.Strip(), nombreLaboratorio.Strip(), proveedor.Strip(),
        //                    Convert.ToSingle(precioMed), Convert.ToSingle(pcoste), cliente, encargo.IdFecha, trabajador, encargo.Unidades,
        //                    encargo.FechaEntrega, encargo.Observaciones.Strip());
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Task.Delay(1500).Wait();
        //    }
        //}

        //public void ProcessFamilia(FarmaticService farmatic, FisiotesService fisiotes)
        //{
        //    try
        //    {
        //        var familias = farmatic.Familias.Get();
        //        foreach (var familia in familias)
        //        {
        //            var familiaRemote = fisiotes.Familias.GetByFamilia(familia.Descripcion);
        //            if (familiaRemote == null)
        //                fisiotes.Familias.Insert(familia.Descripcion);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Task.Delay(1500).Wait();
        //    }
        //}

        //public void ProcessProductosCrticos(FarmaticService farmatic, ConsejoService consejo, FisiotesService fisiotes)
        //{
        //    try
        //    {
        //        fisiotes.Faltas.CheckAndCreateProveedorField();
        //        var falta = fisiotes.Faltas.Last();
        //        var pedidos = falta == null
        //            ? farmatic.Pedidos.GetByFechaGreaterOrEqual(DateTime.Now)
        //            : farmatic.Pedidos.GetByIdGreaterOrEqual(falta.idPedido);

        //        foreach (var pedido in pedidos)
        //        {
        //            var fechaPedido = pedido.Hora;
        //            var fechaActual = DateTime.Now;

        //            var detallePedido = farmatic.Pedidos.GetLineasByPedido(pedido.IdPedido);
        //            foreach (var linea in detallePedido)
        //            {
        //                if (!string.IsNullOrEmpty(linea.XArt_IdArticu?.Trim()))
        //                {
        //                    var familia = string.Empty;
        //                    var superFamilia = string.Empty;
        //                    var codLaboratorio = string.Empty;
        //                    var nombreLaboratorio = string.Empty;
        //                    var proveedor = string.Empty;
        //                    var pcoste = 0d;
        //                    var precioMed = 0d;

        //                    var articulo = farmatic.Articulos.GetById(linea.XArt_IdArticu);
        //                    if (articulo == null)
        //                        nombreLaboratorio = "<Sin Laboratorio>";
        //                    else
        //                    {
        //                        pcoste = articulo.Puc;
        //                        precioMed = articulo.Pvp;

        //                        familia = GetFamiliaFromLocalOrDefault(farmatic, articulo.XFam_IdFamilia, "<Sin Clasificar>");
        //                        superFamilia = familia.Equals("<Sin Clasificar>")
        //                            ? superFamilia = familia
        //                            : GetSuperFamiliaFromLocalOrDefault(farmatic, familia, "<Sin Clasificar>");

        //                        proveedor = GetProveedorFromLocalOrDefault(farmatic, articulo.ProveedorHabitual);

        //                        codLaboratorio = articulo.Laboratorio ?? string.Empty;
        //                        nombreLaboratorio = GetNombreLaboratorioFromLocalOrDefault(farmatic, consejo, codLaboratorio, "<Sin Laboratorio>");
        //                    }

        //                    var faltaLineaActual = fisiotes.Faltas.GetByLineaPedido(linea.IdPedido, linea.IdLinea);
        //                    if (faltaLineaActual == null && articulo.StockActual == 0)
        //                        fisiotes.Faltas.Insert(linea.IdPedido, linea.IdLinea, articulo.IdArticu.Strip(),
        //                            articulo.Descripcion.Strip(), familia.Strip(), superFamilia.Strip(), linea.Unidades,
        //                            fechaActual, codLaboratorio.Strip(), nombreLaboratorio.Strip(), proveedor.Strip(),
        //                            fechaPedido, Convert.ToSingle(precioMed), Convert.ToSingle(pcoste));
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Task.Delay(1500).Wait();
        //    }
        //}
    }
}