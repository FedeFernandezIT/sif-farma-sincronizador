using Sisfarma.Sincronizador.Extensions;
using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Fisiotes;
using Sisfarma.Sincronizador.Helpers;
using Sisfarma.Sincronizador.Sincronizadores.SuperTypes;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sisfarma.Sincronizador.Sincronizadores
{
    public class ClienteSincronizador : BaseSincronizador
    {
        private readonly bool _hasSexo;

        public ClienteSincronizador(FarmaticService farmatic, FisiotesService fisiotes) 
            : base(farmatic, fisiotes)
        {
            _hasSexo = farmatic.Clientes.HasSexoField();
        }

        public override Task Run(CancellationToken cancellationToken = default(CancellationToken))
        {
            _fisiotes.Clientes.ResetDniTracking();
            return base.Run(cancellationToken);
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
                
                var clientData = Generator.FetchLocalClienteData(_farmatic, cliente, _hasSexo);
                    
                _fisiotes.Clientes.ResetDniTracking();
                _fisiotes.Clientes.InsertOrUpdate(
                            clientData.Trabajador, clientData.Tarjeta, cliente.IDCLIENTE, clientData.Nombre.Strip(), clientData.Telefono, clientData.Direccion.Strip(),
                            clientData.Movil, clientData.Email, clientData.Puntos, clientData.FechaNacimiento, clientData.Sexo, clientData.FechaAlta, clientData.Baja, clientData.Lopd,
                            withTrack: true);

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
    }
}
