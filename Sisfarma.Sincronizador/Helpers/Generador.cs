using Sisfarma.Sincronizador.Extensions;
using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Farmatic.Models;
using Sisfarma.Sincronizador.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisfarma.Sincronizador.Helpers
{
    public static class Generator
    {
        public static ClienteDto FetchLocalClienteData(FarmaticService farmaticService, Cliente cliente, bool hasSexo)
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
                if (hasSexo)
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

        public static string GetNombreVendedorOrDefault(FarmaticService farmaticService, int? vendedor, string byDefault = "")
        {
            var vendedorDb = farmaticService.Vendedores.GetById(Convert.ToInt16(vendedor));
            return vendedorDb?.NOMBRE ?? byDefault;
        }
    }
}
