using Sisfarma.Sincronizador.Extensions;
using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Fisiotes;
using Sisfarma.Sincronizador.Helpers;
using Sisfarma.Sincronizador.Sincronizadores.SuperTypes;
using System.Deployment.Application;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Sisfarma.Sincronizador.Fisiotes.Repositories.ConfiguracionesRepository;

namespace Sisfarma.Sincronizador.Sincronizadores
{
    public class HuecoSincronizador : TaskSincronizador
    {
        private const string FIELD_PUNTOS_SISFARMA = FieldsConfiguracion.FIELD_PUNTOS_SISFARMA;
        private readonly bool _hasSexo;

        public HuecoSincronizador(FarmaticService farmatic, FisiotesService fisiotes) 
            : base(farmatic, fisiotes)
        {
            _hasSexo = farmatic.Clientes.HasSexoField();
        }

        public override void Process() => ProcessClientesHuecos();

        private void ProcessClientesHuecos()
        {            
            var remoteHuecos = _fisiotes.Huecos.GetByOrderAsc();
            
            foreach (var hueco in remoteHuecos)
            {
                _cancellationToken.ThrowIfCancellationRequested();

                var cliente = _farmatic.Clientes.GetOneOrDefaulById(hueco.ToIntegerOrDefault());
                if (cliente != null)
                {                                        
                    InsertOrUpdateCliente(cliente);                                     
                    _fisiotes.Huecos.Delete(hueco);
                }
            }
        }

        private void InsertOrUpdateCliente(Farmatic.Models.Cliente cliente)
        {
            var clienteDTO = Generator.GenerarCliente(_farmatic, cliente, _hasSexo);

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
    }
}
