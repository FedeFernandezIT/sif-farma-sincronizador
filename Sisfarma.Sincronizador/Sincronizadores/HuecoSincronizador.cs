using Sisfarma.Sincronizador.Extensions;
using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Fisiotes;
using Sisfarma.Sincronizador.Helpers;
using Sisfarma.Sincronizador.Sincronizadores.SuperTypes;
using System.Threading;
using System.Threading.Tasks;

namespace Sisfarma.Sincronizador.Sincronizadores
{
    public class HuecoSincronizador : TaskSincronizador
    {
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
                    var clientData = Generator.FetchLocalClienteData(_farmatic, cliente, _hasSexo);
                    _fisiotes.Clientes.InsertOrUpdate(
                            clientData.Trabajador, clientData.Tarjeta, cliente.IDCLIENTE, clientData.Nombre.Strip(), clientData.Telefono, clientData.Direccion.Strip(),
                            clientData.Movil, clientData.Email, clientData.Puntos, clientData.FechaNacimiento, clientData.Sexo, clientData.FechaAlta, clientData.Baja, clientData.Lopd);
                 
                    _fisiotes.Huecos.Delete(hueco);
                }
            }
        }        
    }
}
