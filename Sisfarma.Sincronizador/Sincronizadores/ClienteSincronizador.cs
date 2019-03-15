using Sisfarma.Sincronizador.Extensions;
using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Fisiotes;
using Sisfarma.Sincronizador.Helpers;
using Sisfarma.Sincronizador.Sincronizadores.SuperTypes;
using System;
using System.Threading;
using System.Threading.Tasks;
using static Sisfarma.Sincronizador.Fisiotes.Repositories.ConfiguracionesRepository;

namespace Sisfarma.Sincronizador.Sincronizadores
{
    public class ClienteSincronizador : TaskSincronizador
    {
        private const string FIELD_PUNTOS_SISFARMA = FieldsConfiguracion.FIELD_PUNTOS_SISFARMA;
        private readonly bool _hasSexo;
        private readonly string _fileLogs;

        private string _puntosDeSisfarma;
        private bool _perteneceFarmazul;

        public ClienteSincronizador(FarmaticService farmatic, FisiotesService fisiotes)
            : base(farmatic, fisiotes)
        {
            _hasSexo = farmatic.Clientes.HasSexoField();
        }

        public override async Task Run(CancellationToken cancellationToken = default(CancellationToken))
        {
            _fisiotes.Clientes.ResetDniTracking();
            _puntosDeSisfarma = _fisiotes.Configuraciones.GetByCampo(FIELD_PUNTOS_SISFARMA) ?? string.Empty;
            _perteneceFarmazul = _fisiotes.Configuraciones.PerteneceFarmazul();
            await base.Run(cancellationToken);
        }

        public override void Process() => ProcessClientes();

        public void ProcessClientes()
        {
            var lastCliente = _fisiotes.Clientes.GetDniTrackingLast();
            var localClientes = _farmatic.Clientes.GetGreatThanId(Convert.ToInt32(lastCliente));

            var contadorHuecos = -1;
            foreach (var cliente in localClientes)
            {
                _cancellationToken.ThrowIfCancellationRequested();

                if (contadorHuecos == -1)
                    contadorHuecos = Convert.ToInt32(cliente.IDCLIENTE);

                _fisiotes.Clientes.ResetDniTracking();

                InsertOrUpdateCliente(cliente);

                var intIdCliente = Convert.ToInt32(cliente.IDCLIENTE);
                if (intIdCliente != contadorHuecos)
                {
                    for (int i = contadorHuecos; i < intIdCliente; i++)
                    {
                        if (!_fisiotes.Huecos.Any(i))
                            _fisiotes.Huecos.Insert(i.ToString());
                    }
                    contadorHuecos = intIdCliente;
                }
                contadorHuecos++;
            }
        }

        private void InsertOrUpdateCliente(Farmatic.Models.Cliente cliente)
        {
            var clienteDTO = Generator.FetchLocalClienteData(_farmatic, cliente, _hasSexo);

            var puntosDeSisfarma = _puntosDeSisfarma;
            var debeCargarPuntos = puntosDeSisfarma.ToLower().Equals("no") || string.IsNullOrWhiteSpace(puntosDeSisfarma);

            var dniCliente = cliente.PER_NIF.Strip();

            if (_perteneceFarmazul)
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
    }
}