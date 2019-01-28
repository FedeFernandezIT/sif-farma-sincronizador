using Sisfarma.Sincronizador.Extensions;
using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Fisiotes;
using Sisfarma.Sincronizador.Helpers;
using Sisfarma.Sincronizador.Sincronizadores.SuperTypes;

namespace Sisfarma.Sincronizador.Sincronizadores
{
    public class HuecoSincronizador : BaseSincronizador
    {
        private readonly bool _hasSexo;

        public HuecoSincronizador(FarmaticService farmatic, FisiotesService fisiotes) 
            : base(farmatic, fisiotes)
        {
            _hasSexo = farmatic.Clientes.HasSexoField();
        }

        public override void Process() => ProcessClientesHuecos(_farmatic, _fisiotes);

        private void ProcessClientesHuecos(FarmaticService farmaticService, FisiotesService fisiotesService)
        {            
            var remoteHuecos = fisiotesService.Huecos.GetByOrderAsc();
            
            foreach (var hueco in remoteHuecos)
            {
                var cliente = farmaticService.Clientes.GetOneOrDefaulById(hueco.ToIntegerOrDefault());
                if (cliente != null)
                {
                    // Extraemos los datos necesarios del cliente local para sincronizar con el remoto
                    var clientData = Generator.FetchLocalClienteData(farmaticService, cliente, _hasSexo);
                    fisiotesService.Clientes.InsertOrUpdate(
                            clientData.Trabajador, clientData.Tarjeta, cliente.IDCLIENTE, clientData.Nombre.Strip(), clientData.Telefono, clientData.Direccion.Strip(),
                            clientData.Movil, clientData.Email, clientData.Puntos, clientData.FechaNacimiento, clientData.Sexo, clientData.FechaAlta, clientData.Baja, clientData.Lopd);

                    // Eliminamos el hueco del cliente.
                    fisiotesService.Huecos.Delete(hueco);
                }
            }
        }
    }
}
