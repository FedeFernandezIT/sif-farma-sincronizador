using Sisfarma.Sincronizador.Extensions;
using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Fisiotes;
using Sisfarma.Sincronizador.Helpers;
using System;

namespace Sisfarma.Sincronizador.Sincronizadores
{
    public class ClienteSincronizador : BaseSincronizador
    {
        public ClienteSincronizador(FarmaticService farmatic, FisiotesService fisiotes) 
            : base(farmatic, fisiotes)
        {
        }

        public override void Process() => ProcessClientes(_farmatic, _fisiotes);

        public void ProcessClientes(FarmaticService farmaticService, FisiotesService fisiotesService)
        {            
            var existFieldSexo = farmaticService.Clientes.HasSexoField();
                
            var lastCliente = fisiotesService.Clientes.GetDniTrackingLast();
                             
            var localClientes = farmaticService.Clientes.GetGreatThanId(Convert.ToInt32(lastCliente));

            // Sincronizamos los clientes locales con la BD remota
            var contadorHuecos = -1;
            foreach (var cliente in localClientes)
            {
                if (contadorHuecos == -1)
                    //Guardamos el Id del cliente local
                    contadorHuecos = Convert.ToInt32(cliente.IDCLIENTE);

                //Extraemos los datos necesarios del cliente local para sincronizar con el remoto
                var clientData = Generator.FetchLocalClienteData(farmaticService, cliente, false);
                    
                fisiotesService.Clientes.ResetDniTracking();
                fisiotesService.Clientes.InsertOrUpdate(
                            clientData.Trabajador, clientData.Tarjeta, cliente.IDCLIENTE, clientData.Nombre.Strip(), clientData.Telefono, clientData.Direccion.Strip(),
                            clientData.Movil, clientData.Email, clientData.Puntos, clientData.FechaNacimiento, clientData.Sexo, clientData.FechaAlta, clientData.Baja, clientData.Lopd,
                            withTrack: true);


                //Almacenamos todos los huecos de clientes que hayan.
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
    }
}
