using Sisfarma.Sincronizador.Consejo;
using Sisfarma.Sincronizador.Extensions;
using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Farmatic.Models;
using Sisfarma.Sincronizador.Fisiotes;
using Sisfarma.Sincronizador.Fisiotes.Models;
using Sisfarma.Sincronizador.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisfarma.Sincronizador.Sincronizadores
{
    public class PuntosPendientesSincronizador
    {
        FarmaticService _farmatic;
        FisiotesService _fisiotes;
        ConsejoService _consejo;

        public PuntosPendientesSincronizador(FarmaticService farmatic, FisiotesService fisiotes, ConsejoService consejo)
        {
            _farmatic = farmatic ?? throw new ArgumentNullException(nameof(farmatic));
            _fisiotes = fisiotes ?? throw new ArgumentNullException(nameof(fisiotes));
            _consejo = consejo ?? throw new ArgumentNullException(nameof(consejo));
        }


        public async Task Run()
        {
            while (true)
            {
                ProcessPuntosPendientes(_farmatic, _consejo, _fisiotes);
                await Task.Delay(200);
            }
        }

        void ProcessPuntosPendientes(FarmaticService farmatic, ConsejoService consejo, FisiotesService fisiotes)
        {
            try
            {
                // Validamos los campos existentes en puntos_pendientes
                //fisiotes.PuntosPendientes.CheckAndCreateFields();

                // Creamos la tabla entregas_clientes en la BD remota, si no existe
                //fisiotes.Entregas.CreateTable(_remoteBase);

                // Verificamos el campo tipoPago en pendientes_puntos en la BD remota
                //fisiotes.PuntosPendientes.CheckTipoPagoField(_remoteBase);

                // Validamos los campos existentes en ClienteAux
                var existFieldSexo = farmatic.Clientes.HasSexoField();
                                
                var idVenta = fisiotes.PuntosPendientes.GetUltimaVenta();

                // Recuperamos las ventas locales
                var ventas = farmatic.Ventas.GetByIdGreaterOrEqual(idVenta);
                foreach (var venta in ventas)
                {
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

                    
                    var vendedor = farmatic.Vendedores.GetById(venta.XVend_IdVendedor)?.NOMBRE ?? "NO";
                    var detalleVenta = farmatic.Ventas.GetLineasVentaByVenta(venta.IdVenta);
               
                    foreach (var linea in detalleVenta)
                    {
                        if (!fisiotes.PuntosPendientes.Exists(venta.IdVenta, linea.IdNLinea))
                        {
                            var  puntoPendiente = GenerarPuntoPendiente(venta, linea, vendedor, farmatic, consejo);                            
                            fisiotes.PuntosPendientes.Insert(puntoPendiente);                            
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
                            var entrega = GenerarEntregaCliente(venta, lineaVirtual, vendedor);                            
                            fisiotes.Entregas.Insert(entrega);
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
            var redencion = (farmatic.Ventas.GetLineaRedencionByKey(venta.IdVenta, linea.IdNLinea)?
                .Redencion) ?? 0;
            var articulo = farmatic.Articulos.GetById(linea.Codigo);

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

        ClienteDto FetchLocalClienteData(FarmaticService farmaticService, Farmatic.Models.Cliente cliente, bool hasSexField)
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

        string GetCodidoBarrasFromLocalOrDefault(FarmaticService farmaticService, string articulo)
        {
            var sinonimo = farmaticService.Sinonimos.GetByArticulo(articulo);
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

        string GetNombreVendedorOrDefault(FarmaticService farmaticService, int? vendedor, string byDefault = "")
        {
            var vendedorDb = farmaticService.Vendedores.GetById(Convert.ToInt16(vendedor));
            return vendedorDb?.NOMBRE ?? byDefault;
        }
    }
}
