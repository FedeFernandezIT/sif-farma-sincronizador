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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Sisfarma.Sincronizador.Fisiotes.Repositories.ConfiguracionesRepository;

namespace Sisfarma.Sincronizador
{
    public class SincronizadorApplication : ApplicationContext
    {
        private string _remoteBase, _remoteServer;
        private string _localBase, _localServer, _localUser, _localPass;
        private int _marketCodeList;
        
        private System.Timers.Timer timerClientes;
        private System.Timers.Timer timerClientesHuecos;
        private System.Timers.Timer timerActualizarRecetasPendientes;
        private System.Timers.Timer timerActualizarEntregasClientes;
        private System.Timers.Timer timerActualizarProductosBorrados;
        private System.Timers.Timer timerActualizarPuntosPendientes;
        private System.Timers.Timer timerSinonimos;
        private System.Timers.Timer timerControlSinStockInicial;
        private System.Timers.Timer timerControlStockFechasSalida;
        private System.Timers.Timer timerControlStockInicial;
        private System.Timers.Timer timerControlStockFechasEntrada;

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
                timerClientes.Stop();
                FarmaticService farmatic = new FarmaticService();
                FisiotesService fisiotes = new FisiotesService();
                //fisiotes.SetCeroClientes();
                ProcessClientes(farmatic, fisiotes);
                timerClientes.Start();
            };

            timerClientesHuecos = new System.Timers.Timer(63000);
            timerClientesHuecos.Elapsed += (sender, @event) =>
            {
                timerClientesHuecos.Stop();
                FarmaticService farmatic = new FarmaticService();
                FisiotesService fisiotes = new FisiotesService();
                ProcessClientesHuecos(farmatic, fisiotes);
                timerClientesHuecos.Start();
            };

            timerActualizarRecetasPendientes = new System.Timers.Timer(5000);
            timerActualizarRecetasPendientes.Elapsed += (sender, @event) =>
            {
                timerActualizarRecetasPendientes.Stop();
                FarmaticService farmatic = new FarmaticService();
                FisiotesService fisiotes = new FisiotesService();
                ProcessClientesHuecos(farmatic, fisiotes);
                timerActualizarRecetasPendientes.Start();
            };

            timerActualizarEntregasClientes = new System.Timers.Timer(30000);
            timerActualizarEntregasClientes.Elapsed += (sender, @event) =>
            {
                timerActualizarEntregasClientes.Stop();
                FarmaticService farmatic = new FarmaticService();
                FisiotesService fisiotes = new FisiotesService();
                ProcessUpdateEntregasClientes(farmatic, fisiotes);
                timerActualizarEntregasClientes.Start();
            };

            timerActualizarProductosBorrados = new System.Timers.Timer(30000);
            timerActualizarProductosBorrados.Elapsed += (sender, @event) =>
            {
                timerActualizarProductosBorrados.Stop();
                FarmaticService farmatic = new FarmaticService();
                FisiotesService fisiotes = new FisiotesService();
                ProcessUpdateProductosBorrados(farmatic, fisiotes);
                timerActualizarProductosBorrados.Start();
            };

            timerActualizarPuntosPendientes = new System.Timers.Timer(5000);
            timerActualizarPuntosPendientes.Elapsed += (sender, @event) =>
            {
                timerActualizarPuntosPendientes.Stop();
                FarmaticService farmatic = new FarmaticService();
                FisiotesService fisiotes = new FisiotesService();
                ProcessUpdatePuntosPendientes(farmatic, fisiotes);
                timerActualizarPuntosPendientes.Start();
            };

            timerSinonimos = new System.Timers.Timer(50000);
            timerSinonimos.Elapsed += (sender, @event) =>
            {
                timerSinonimos.Stop();
                FarmaticService farmatic = new FarmaticService();
                FisiotesService fisiotes = new FisiotesService();
                ProcessSinonimos(farmatic, fisiotes);
                timerSinonimos.Start();
            };

            timerControlSinStockInicial = new System.Timers.Timer(10000);
            timerControlSinStockInicial.Elapsed += (sender, @event) =>
            {
                timerControlSinStockInicial.Stop();
                FarmaticService farmatic = new FarmaticService();
                FisiotesService fisiotes = new FisiotesService();
                ConsejoService consejo = new ConsejoService();
                ProcessControlSinStockInicial(farmatic, consejo, fisiotes);
                timerControlSinStockInicial.Start();
            };

            timerControlStockFechasSalida = new System.Timers.Timer(60000);
            timerControlStockFechasSalida.Elapsed += (sender, @event) =>
            {
                timerControlStockFechasSalida.Stop();
                FarmaticService farmatic = new FarmaticService();
                FisiotesService fisiotes = new FisiotesService();
                ConsejoService consejo = new ConsejoService();
                ProcessControlStockFechasSalida(farmatic, consejo, fisiotes);
                timerControlStockFechasSalida.Start();
            };

            timerControlStockInicial = new System.Timers.Timer(5000);
            timerControlStockInicial.Elapsed += (sender, @event) =>
            {
                timerControlStockInicial.Stop();
                FarmaticService farmatic = new FarmaticService();
                FisiotesService fisiotes = new FisiotesService();
                ConsejoService consejo = new ConsejoService();
                ProcessControlStockInicial(farmatic, consejo, fisiotes);
                timerControlStockInicial.Start();
            };

            timerControlStockFechasEntrada = new System.Timers.Timer(60000);
            timerControlStockFechasEntrada.Elapsed += (sender, @event) =>
            {
                timerControlStockFechasEntrada.Stop();
                FarmaticService farmatic = new FarmaticService();
                FisiotesService fisiotes = new FisiotesService();
                ConsejoService consejo = new ConsejoService();
                
                timerControlStockFechasEntrada.Start();
            };


            timerClientes.Start();
            timerClientesHuecos.Start();
            timerActualizarRecetasPendientes.Start();
            timerActualizarEntregasClientes.Start();
            timerActualizarProductosBorrados.Start();
            timerActualizarPuntosPendientes.Start();
            timerSinonimos.Start();
            timerControlSinStockInicial.Start();
            timerControlStockFechasSalida.Start();
            timerControlStockInicial.Start();
            timerControlStockFechasEntrada.Start();
        }

        private void LeerFicherosConfiguracion()
        {
            try
            {
                var dir = ConfigurationManager.AppSettings["Directory.Setup"];

                // Configuramos el acceso a el servidor remoto de MySql
                var path = ConfigurationManager.AppSettings["File.Remote.Server"];
                _remoteBase = new StreamReader(Path.Combine(dir, path)).ReadLine();
                path = ConfigurationManager.AppSettings["File.Remote.Base"];
                _remoteServer = new StreamReader(Path.Combine(dir, path)).ReadLine();

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
                // Creamos la tabla clientes_huecos
                fisiotesService.Huecos.CreateTable();
                
                // Validamos los campos existentes en clientes
                fisiotesService.Clientes.CheckAndCreateFields();
                
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
                
                var contadorHuecos = -1;
                // Sincronizamos los clientes locales con la BD remota
                foreach (var cliente in localClientes)
                {
                    if (contadorHuecos == -1)
                        // Guardamos el Id del cliente local
                        contadorHuecos = Convert.ToInt32(cliente.IDCLIENTE);

                    // Extraemos los datos necesarios del cliente local para sincronizar con el remoto
                    var clientData = FetchLocalClienteData(farmaticService, cliente, false);

                    // Recuperamos el último cliente remoto
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

                    // Almacenamos todos los huecos de clientes que hayan.
                    var intIdCliente = Convert.ToInt32(cliente.IDCLIENTE);
                    if (intIdCliente != contadorHuecos)
                    {
                        for (int i = contadorHuecos; i < intIdCliente; i++)
                        {                            
                            if (!fisiotesService.Huecos.Any(i))                            
                                fisiotesService.Huecos.Insert(i.ToString());                            
                        }
                        contadorHuecos = intIdCliente;
                    }
                    contadorHuecos++;
                }
            }
            catch (Exception e)
            {                                
                Task.Delay(1500).Wait();
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
                Task.Delay(1500).Wait();
            }
        }

        public void ProcessUpdateRecetasPendientes(FarmaticService farmaticService, FisiotesService fisiotesService)
        {         
            try
            {
                var puntos = fisiotesService.PuntosPendientes.GetOfRecetasPendientes();
                foreach (var punto in puntos)
                {
                    var lineaVenta = farmaticService.Ventas.GetLineaVentaByKey(punto.idventa, punto.idnlinea);

                    if (lineaVenta != null && (punto.recetaPendiente == null || lineaVenta.RecetaPendiente != "D"))
                        fisiotesService.PuntosPendientes.Update(lineaVenta.RecetaPendiente, punto.idventa, punto.idnlinea);
                    else if (punto.recetaPendiente == null)
                        fisiotesService.PuntosPendientes.Update(punto.idventa, punto.idnlinea);
                }
            }
            catch (Exception e)
            {              
                Task.Delay(1500).Wait();
            }
        }

        public void ProcessUpdateEntregasClientes(FarmaticService farmaticService, FisiotesService fisiotesService)
        {            
            const string FIELD_POR_DONDE_VOY_ENTREGAS_CLIENTES =
                FieldsConfiguracion.FIELD_POR_DONDE_VOY_ENTREGAS_CLIENTES;
            try
            {
                var cfg =
                    fisiotesService.Configuraciones.GetByCampo(FIELD_POR_DONDE_VOY_ENTREGAS_CLIENTES);
                var venta = 0L;
                if (cfg == null)
                {
                    fisiotesService.Configuraciones.Insert(FIELD_POR_DONDE_VOY_ENTREGAS_CLIENTES);
                    var entregaAny = fisiotesService.Entregas.Last();
                    if (entregaAny == null)
                    {
                        var pendiente = fisiotesService.PuntosPendientes.Last();
                        venta = pendiente?.idventa ?? 0;
                    }
                    else venta = entregaAny.idventa;

                    fisiotesService.Configuraciones.Update(FIELD_POR_DONDE_VOY_ENTREGAS_CLIENTES, venta.ToString());
                }
                else venta = Convert.ToInt64(cfg.valor);

                var ventasVirtuales = farmaticService.Ventas.GetVirtualesLessThanId(venta);
                foreach (var vtaVirtual in ventasVirtuales)
                {
                    fisiotesService.Configuraciones.Update(FIELD_POR_DONDE_VOY_ENTREGAS_CLIENTES, vtaVirtual.IdVenta.ToString());
                    var dni = vtaVirtual.XClie_IdCliente.Strip();
                    if (string.IsNullOrEmpty(dni))
                        dni = "0";

                    var trabajador = GetNombreVendedorOrDefault(farmaticService, vtaVirtual.XVend_IdVendedor, "NO");
                    var lineas = farmaticService.Ventas.GetLineasVirtualesByVenta(vtaVirtual.IdVenta);
                    foreach (var linea in lineas)
                    {
                        var entrega = fisiotesService.Entregas.GetByKey(linea.IdVenta, linea.IdNLinea);
                        if (entrega == null)
                            fisiotesService.Entregas.Insert(linea.IdVenta, linea.IdNLinea, linea.Codigo.Strip(),
                                linea.Descripcion.Strip(), linea.Cantidad, Convert.ToDecimal(linea.ImporteNeto), linea.TipoLinea,
                                Convert.ToInt32(vtaVirtual.FechaHora.ToString("yyyyMMdd")), dni, vtaVirtual.Maquina, trabajador,
                                vtaVirtual.FechaHora, Convert.ToSingle(linea.Pvp));
                    }
                }
            }
            catch (Exception e)
            {                
                Task.Delay(1500).Wait();
            }
        }

        public void ProcessUpdateProductosBorrados(FarmaticService farmaticService, FisiotesService fisiotesService)
        {        
            const string FIELD_POR_DONDE_VOY_BORRAR =
                FieldsConfiguracion.FIELD_POR_DONDE_VOY_BORRAR;
            try
            {
                var cfg = fisiotesService.Configuraciones.GetByCampo(FIELD_POR_DONDE_VOY_BORRAR);
                var codArticulo = "0";
                if (cfg == null)
                    fisiotesService.Configuraciones.Insert(FIELD_POR_DONDE_VOY_BORRAR);
                else
                    codArticulo = cfg.valor;

                var exsitWebInMedicamentos = fisiotesService.Medicamentos.HasWebField();
                var medicamentos =
                    fisiotesService.Medicamentos.GetCodigosNacionalesGreaterOrEqual(codArticulo, exsitWebInMedicamentos);
                switch (medicamentos.Count)
                {
                    case 0:
                        fisiotesService.Configuraciones.Update(FIELD_POR_DONDE_VOY_BORRAR, "0");
                        break;
                    case 1:
                        var articulo =
                            farmaticService.Articulos.GetById(medicamentos.First().PadLeft(6, '0'));
                        if (articulo == null)
                            fisiotesService.Medicamentos.DeleteByCodigoNacional(medicamentos.First());
                        fisiotesService.Configuraciones.Update(FIELD_POR_DONDE_VOY_BORRAR, "0");
                        break;
                }

                foreach (var codNac in medicamentos)
                {
                    var articulo = farmaticService.Articulos.GetById(codNac.PadLeft(6, '0'));
                    if (articulo == null)
                        fisiotesService.Medicamentos.DeleteByCodigoNacional(codNac);
                    fisiotesService.Configuraciones.Update(FIELD_POR_DONDE_VOY_BORRAR, codNac);
                }
            }
            catch (Exception e)
            {                
                Task.Delay(1500).Wait();
            }
        }

        public void ProcessUpdatePuntosPendientes(FarmaticService farmaticService, FisiotesService fisiotesService)
        {            
            try
            {
                var puntos = fisiotesService.PuntosPendientes.GetWithoutRedencion();
                foreach (var pto in puntos)
                {
                    var venta = farmaticService.Ventas.GetById(pto.idventa);
                    if (venta != null)
                    {
                        var lineas = farmaticService.Ventas.GetLineasVentaByVenta(venta.IdVenta);
                        foreach (var linea in lineas)
                        {
                            var lineaRedencion =
                                farmaticService.Ventas.GetLineaRedencionByKey(linea.IdVenta, linea.IdNLinea);
                            var redencion = lineaRedencion?.Redencion ?? 0;
                            var articulo = farmaticService.Articulos.GetById(linea.Codigo);
                            var proveedor = articulo != null
                                ? GetProveedorFromLocalOrDefault(farmaticService, articulo.ProveedorHabitual)
                                : string.Empty;
                            fisiotesService.PuntosPendientes.Update(venta.TipoVenta, proveedor,
                                Convert.ToSingle(linea.DescuentoLinea), Convert.ToSingle(venta.DescuentoOpera),
                                Convert.ToSingle(redencion), linea.IdVenta, linea.IdNLinea);
                        }
                    }
                    else
                    {
                        fisiotesService.PuntosPendientes.Update(pto.idventa);
                    }
                }
            }
            catch (Exception e)
            {         
                Task.Delay(1500).Wait();
            }
        }

        private string GetProveedorFromLocalOrDefault(FarmaticService farmaticService, string proveedor, string byDefault = "")
        {
            var proveedorDb = farmaticService.Proveedores.GetById(proveedor);
            return proveedorDb?.FIS_NOMBRE ?? byDefault;
        }

        public void ProcessSinonimos(FarmaticService farmaticService, FisiotesService fisiotesService)
        {            
            try
            {
                fisiotesService.Sinonimos.CheckAndAlterFields(_remoteBase);
                var sinonimo = fisiotesService.Sinonimos.First();
                var insertar = sinonimo == null || Array.Exists(new[] { "1230", "1730", "1930" }, x => x.Equals(DateTime.Now.ToString("HHmm")))
                        ? true
                        : false;

                if (insertar)
                {
                    fisiotesService.Sinonimos.Truncate();
                    var sinonimos = farmaticService.Sinonimos.Get();

                    var take = 1000;
                    for (int i = 0; i < sinonimos.Count; i += take)
                    {
                        var items = sinonimos.Skip(i).Take(1000).Select(x => new Sinonimo
                        {
                            cod_barras = x.Sinonimo.Strip(),
                            cod_nacional = x.IdArticu.Strip()
                        }).ToList();
                        fisiotesService.Sinonimos.Insert(items);
                    }

                }
            }
            catch (Exception e)
            {         
                Task.Delay(1500).Wait();
            }
        }

        public void ProcessControlSinStockInicial(FarmaticService farmaticService, ConsejoService consejoService, FisiotesService fisiotesService)
        {           
            const string FIELD_POR_DONDE_VOY_SIN_STOCK = FieldsConfiguracion.FIELD_POR_DONDE_VOY_SIN_STOCK;
            try
            {
                var configuracion = fisiotesService.Configuraciones.GetByCampo(FIELD_POR_DONDE_VOY_SIN_STOCK);
                if (configuracion == null)
                    fisiotesService.Configuraciones.Insert(FIELD_POR_DONDE_VOY_SIN_STOCK);
                var codArticulo = configuracion != null ? configuracion.valor : "0";
                var articulos = farmaticService.Articulos.GetWithoutStockByIdGreaterOrEqual(codArticulo);
                foreach (var articulo in articulos)
                {
                    SyncUpArticuloWithIva(farmaticService, consejoService, fisiotesService, articulo, FIELD_POR_DONDE_VOY_SIN_STOCK, articulo.IdArticu);
                }
                fisiotesService.Configuraciones.Update(FIELD_POR_DONDE_VOY_SIN_STOCK, "0");
                fisiotesService.Medicamentos.ResetPorDondeVoySinStock();
            }
            catch (Exception e)
            {                
                Task.Delay(1500).Wait();
            }
        }

        private void SyncUpArticuloWithIva(FarmaticService farmaticService, ConsejoService consejoService, FisiotesService fisiotesService, ArticuloWithIva articulo, string updatefield, string updateValue)
        {            
            fisiotesService.Configuraciones.Update(updatefield, updateValue);

            var familila = farmaticService.Familias.GetById(articulo.XFam_IdFamilia);
            var fechaUltimaCompra = articulo.FechaUltimaEntrada;
            var fechaUltimaVenta = articulo.FechaUltimaSalida;
            var precio = articulo.Pvp;
            var pcoste = articulo.Puc;
            var pvpsIva = Math.Round(articulo.Pvp * 100 / (articulo.Iva + 100), 2);
            var stock = articulo.StockActual;
            var stockMinimo = articulo.StockMinimo;
            var stockMaximo = articulo.StockMaximo;
            var desc = articulo.Descripcion.Strip();

            var superFamilia = !string.IsNullOrEmpty(familila?.Descripcion)
                    ? GetSuperFamiliaFromLocalOrDefault(farmaticService, familila.Descripcion)
                    : string.Empty;

            var codigoBarra = GetCodidoBarrasFromLocalOrDefault(farmaticService, articulo.IdArticu);
            var proveedor = GetProveedorFromLocalOrDefault(farmaticService, articulo.ProveedorHabitual);
            var nombreLaboratorio = GetNombreLaboratorioFromLocalOrDefault(farmaticService, consejoService, articulo.Laboratorio, "<Sin Laboratorio>");

            var espepara = consejoService.GetEspeparaByCodigo(articulo.IdArticu);
            var present = espepara?.PRESENTACION ?? string.Empty;

            var descripcionHtml = string.Empty;
            var textos = consejoService.GetTextosByCodigoEspepara(articulo.IdArticu);
            foreach (var texto in textos)
            {
                if (string.IsNullOrEmpty(descripcionHtml))
                    descripcionHtml = texto;
                descripcionHtml += $@" <br> {texto}";
            }
            descripcionHtml = descripcionHtml.Length < 30000
                ? descripcionHtml.Replace(Environment.NewLine, "<br>").Replace("\0", string.Empty).Strip()
                : string.Empty;

            var baja = articulo.Baja;
            var activo = !baja;

            var fechaCaducidad = articulo.FechaCaducidad;

            var medicamento = fisiotesService.Medicamentos.GetByCodNacional(articulo.IdArticu);
            if (medicamento == null)
                fisiotesService.Medicamentos.Insert(codigoBarra.Strip(), articulo.IdArticu.Strip(), desc.Strip(), superFamilia.Strip(),
                        familila?.Descripcion.Strip(), Convert.ToSingle(precio), desc.Strip(), articulo.Laboratorio.Strip(), nombreLaboratorio.Strip(),
                        proveedor.Strip(), Convert.ToSingle(pvpsIva), Convert.ToInt32(articulo.Iva), stock, Convert.ToSingle(pcoste), stockMinimo, stockMaximo, present.Strip(),
                        descripcionHtml.Strip(), activo, fechaCaducidad, fechaUltimaCompra, fechaUltimaVenta, baja);
            else
            {
                if (desc.Strip() != medicamento.nombre || precio != medicamento.precio ||
                    articulo.Laboratorio != medicamento.laboratorio || articulo.Iva != medicamento.iva ||
                    stock != medicamento.stock || present.Strip() != medicamento.presentacion ||
                    descripcionHtml != medicamento.descripcion)
                    fisiotesService.Medicamentos.Update(codigoBarra.Strip(), desc.Strip(), superFamilia.Strip(),
                        familila?.Descripcion.Strip(), Convert.ToSingle(precio), desc.Strip(), articulo.Laboratorio.Strip(), nombreLaboratorio.Strip(),
                        proveedor.Strip(), Convert.ToInt32(articulo.Iva), Convert.ToSingle(pvpsIva), stock, Convert.ToSingle(pcoste), stockMinimo, stockMaximo, present.Strip(),
                        descripcionHtml.Strip(), activo, fechaCaducidad, fechaUltimaCompra, fechaUltimaVenta, baja, articulo.IdArticu,
                        withSqlExtra: true);
                else
                    fisiotesService.Medicamentos.Update(codigoBarra.Strip(), desc.Strip(), superFamilia.Strip(),
                        familila?.Descripcion.Strip(), Convert.ToSingle(precio), desc.Strip(), articulo.Laboratorio.Strip(), nombreLaboratorio.Strip(),
                        proveedor.Strip(), Convert.ToInt32(articulo.Iva), Convert.ToSingle(pvpsIva), stock, Convert.ToSingle(pcoste), stockMinimo, stockMaximo, present.Strip(),
                        descripcionHtml.Strip(), activo, fechaCaducidad, fechaUltimaCompra, fechaUltimaVenta, baja, articulo.IdArticu);
            }            
        }

        private string GetSuperFamiliaFromLocalOrDefault(FarmaticService farmaticService, string familia, string byDefault = "")
        {
            return farmaticService.Familias.GetSuperFamiliaDescripcionByFamilia(familia) ?? byDefault;
        }

        private string GetCodidoBarrasFromLocalOrDefault(FarmaticService farmaticService, string articulo)
        {
            var sinonimo = farmaticService.Sinonimos.GetByArticulo(articulo);
            return sinonimo?.Sinonimo ?? "847000".PadLeft(6, '0');
        }

        private string GetNombreLaboratorioFromLocalOrDefault(FarmaticService farmaticService, ConsejoService consejoService, string codigo, string byDefault = "")
        {
            var nombreLaboratorio = byDefault;
            if (!string.IsNullOrEmpty(codigo?.Trim()) && !string.IsNullOrWhiteSpace(codigo))
            {
                var laboratorioDb = consejoService.GetLaboratorioByCodigo(codigo);
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

        public void ProcessControlStockFechasSalida(FarmaticService farmaticService, ConsejoService consejoService, FisiotesService fisiotesService)
        {            
            const string FIELD_NAME = FieldsConfiguracion.FIELD_STOCK_SALIDA;
            try
            {
                DateTime? fechaActualizacionStock = null;
                var configuracion = fisiotesService.Configuraciones.GetByCampo(FIELD_NAME);
                if (configuracion == null)
                    fisiotesService.Configuraciones.Insert(FIELD_NAME);
                else
                    fechaActualizacionStock = CalculateFechaActualizacion(configuracion.valor);

                // Recuperamos artículos de la base local junto al iva
                var articulosWithIva = farmaticService.Articulos.GetByFechaUltimaSalidaGreaterOrEqual(fechaActualizacionStock);
                foreach (var articulo in articulosWithIva)
                {
                    var strFecha = articulo.FechaUltimaSalida?.ToString("yyyy-MM-dd");
                    SyncUpArticuloWithIva(farmaticService, consejoService, fisiotesService, articulo, FIELD_NAME, strFecha);
                }
            }
            catch (Exception e)
            {         
                Task.Delay(1500).Wait();
            }
        }

        private static DateTime CalculateFechaActualizacion(string fecha)
        {
            try
            {
                return string.IsNullOrEmpty(fecha) || string.IsNullOrWhiteSpace(fecha)
                    ? DateTime.Now.AddDays(-7)
                    : (DateTime.Now - DateTime.ParseExact(fecha, "yyyy-dd-MM", CultureInfo.InvariantCulture)).TotalDays > 7
                    ? DateTime.Now.AddDays(-7)
                    : DateTime.ParseExact(fecha, "yyyy-dd-MM", CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return DateTime.ParseExact(fecha, "yyyy-dd-MM", CultureInfo.InvariantCulture);
            }
        }

        public void ProcessControlStockInicial(FarmaticService farmaticService, ConsejoService consejoService, FisiotesService fisiotesService)
        {            
            const string FIELD_STOCK_ENTRADA = FieldsConfiguracion.FIELD_STOCK_ENTRADA;
            const string FIELD_STOCK_SALIDA = FieldsConfiguracion.FIELD_STOCK_SALIDA;
            const string FIELD_POR_DONDE_VOY_CON_STOCK = FieldsConfiguracion.FIELD_POR_DONDE_VOY_CON_STOCK;
            const string FIELD_POR_DONDE_VOY_SIN_STOCK = FieldsConfiguracion.FIELD_POR_DONDE_VOY_SIN_STOCK;
            try
            {
                fisiotesService.Medicamentos.CheckAndCreateFields();
                var configuracion = fisiotesService.Configuraciones.GetByCampo(FIELD_STOCK_ENTRADA);
                if (configuracion == null)
                    fisiotesService.Configuraciones.Insert(FIELD_STOCK_ENTRADA);
                configuracion = fisiotesService.Configuraciones.GetByCampo(FIELD_STOCK_SALIDA);
                if (configuracion == null)
                    fisiotesService.Configuraciones.Insert(FIELD_STOCK_SALIDA);
                configuracion = fisiotesService.Configuraciones.GetByCampo(FIELD_POR_DONDE_VOY_CON_STOCK);
                if (configuracion == null)
                {
                    fisiotesService.Configuraciones.Insert(FIELD_POR_DONDE_VOY_CON_STOCK);
                    fisiotesService.Configuraciones.Insert(FIELD_POR_DONDE_VOY_SIN_STOCK);
                }
                var codArticulo = configuracion != null ? configuracion.valor : "0";
                var articulos = farmaticService.Articulos.GetWithStockByIdGreaterOrEqual(codArticulo);
                foreach (var articulo in articulos)
                {
                    SyncUpArticuloWithIva(farmaticService, consejoService, fisiotesService, articulo, FIELD_POR_DONDE_VOY_CON_STOCK, articulo.IdArticu);
                }
                fisiotesService.Configuraciones.Update(FIELD_POR_DONDE_VOY_CON_STOCK, "0");
                fisiotesService.Medicamentos.ResetPorDondeVoy();
            }
            catch (Exception e)
            {                
                Task.Delay(1500).Wait();
            }
        }

        public void ProcessControlStockFechasEntrada(FarmaticService farmaticService, ConsejoService consejoService, FisiotesService fisiotesService)
        {            
            const string FIELD_NAME = FieldsConfiguracion.FIELD_STOCK_ENTRADA;
            try
            {
                // Estblecemos la fecha de actualización del stock
                DateTime? fechaActualizacionStock = null;
                var configuracion = fisiotesService.Configuraciones.GetByCampo(FIELD_NAME);
                if (configuracion == null)
                    fisiotesService.Configuraciones.Insert(FIELD_NAME);
                else
                    fechaActualizacionStock = CalculateFechaActualizacion(configuracion.valor);

                // Recuperamos artículos de la base local junto al iva
                var articulosWithIva = farmaticService.Articulos.GetByFechaUltimaEntradaGreaterOrEqual(fechaActualizacionStock);
                foreach (var articulo in articulosWithIva)
                {
                    var strFecha = articulo.FechaUltimaEntrada?.ToString("yyyy-MM-dd");
                    SyncUpArticuloWithIva(farmaticService, consejoService, fisiotesService, articulo, FIELD_NAME, strFecha);
                }
            }
            catch (Exception e)
            {                
                Task.Delay(1500).Wait();
            }
        }
    }    
}
