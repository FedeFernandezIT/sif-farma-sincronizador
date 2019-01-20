using System.Collections.Generic;

namespace Sisfarma.Sincronizador.Fisiotes
{
    public class FisiotesConfig
    {        
        public string BaseAddress { get; set; }

        public Credential Credentials { get; set; }

        public CategoriaResource Categorias { get; set; }

        public ClienteResource Clientes { get; set; }

        public HuecoResource Huecos { get; set; }

        public PuntoResource Puntos { get; set; }

        public ConfiguracionResource Configuraciones { get; set; }

        public EntregaResource Entregas { get; set; }

        public MedicamentoResource Medicamentos { get; set; }

        public SinonimoResource Sinonimos { get; set; }

        public PedidoResource Pedidos { get; set; }
        public FaltaResource Faltas { get; set; }
        public FamiliaResource Familias { get; internal set; }
        public EncargoResource Encargos { get; internal set; }

        public static FisiotesConfig TestConfig(string remoteServer, string username, string password)
        {
            return new FisiotesConfig
            {
                BaseAddress = remoteServer,
                Credentials = new Credential
                {
                    Username = username,
                    Password = password
                },

                Clientes = new ClienteResource
                {
                    GetDniTrackingLast = "api/cliente/ultimo",
                    ResetDniTracking = "api/cliente/setCeroClientes",
                    GetByDni = "api/cliente/index/dni/{dni}",
                    Insert = "api/cliente/index/dni/{dni}",
                    Update = "api/cliente/update"
                },

                Huecos = new HuecoResource
                {
                    Exists = "api/cliente/exists/hueco/{hueco}",
                    Insert = "api/cliente/hueco",
                    GetAll = "api/cliente/hueco",
                    Delete = "api/cliente/huecoeliminar"
                },

                Puntos = new PuntoResource
                {
                    GetVentasNoActualizadas = "api/puntos/ventasNoActualizado",
                    GetSinRedencion = "api/puntos/sinRedencion",
                    GetLastOfYear = "api/puntos/ultimo/year/{year}",
                    GetByItemVenta = "api/puntos/item/venta/{venta}/linea/{linea}",
                    GetUltimaVenta = "api/puntos/ultimo",
                    Update = "api/puntos/update",
                    Insert = "api/puntos/createUpdate"
                },

                Configuraciones = new ConfiguracionResource
                {
                    GetValorByCampo = "/api/configuracion/index/campo/{campo}",
                    UpdateValorByCampo = "/api/configuracion/campo",
                },

                Entregas = new EntregaResource
                {
                    GetByKey = "/api/entregas/index/venta/{venta}/linea/{linea}",
                    Insert = "/api/entregas/create"
                },

                Medicamentos = new MedicamentoResource
                {
                    GetGreaterOrEqualByCodigoNacional = "/api/medicamentos/gteq/id/{id}/limit/{limit}/order/{order}",
                    GetByCodNacional = "/api/medicamentos/index/id/{id}",
                    Delete = "/api/medicamentos/eliminar",
                    ResetSeguimientoSinStock = "/api/medicamentos/resetSinStock",
                    ResetSeguimientoDondeVoy = "/api/medicamentos/resetDondeVoy",
                    Insert = "/api/medicamentos/createUpdate",
                    Update = "/api/medicamentos/createUpdate"
                },

                Sinonimos = new SinonimoResource
                {
                    IsEmpty = "/api/sinonimos/isEmpty",
                    Empty = "/api/sinonimos/empty",
                    Insert = "/api/sinonimos/createUpdate"
                },

                Pedidos = new PedidoResource
                {
                    Ultimo = "/api/pedido/ultimo",
                    GetByPedido = "/api/pedido/index/idPedido/{pedido}",
                    Insert = "/api/pedido/createUpdate",
                    GetByLineaDePedido = "/api/linea/index/pedido/{pedido}/linea/{linea}",
                    InsertLineaDePedido = "/api/linea/createUpdate"
                },

                Faltas = new FaltaResource
                {
                    Ultima = "api/faltas/ultimo",
                    GetByLineaDePedido = "api/faltas/index/pedido/{pedido}/linea/{linea}",
                    InsertLineaDePedido = "/api/faltas/createUpdate"
                },

                Familias = new FamiliaResource
                {
                    GetByFamilia = "/api/familia/cod/familia/{familia}",
                    Insert = "/api/familia/createUpdate"
                },

                Encargos = new EncargoResource
                {
                    Ultimo = "/api/encargo/ultimo",
                    GetByEncargo = "/api/encargo/index/encargo/{encargo}",
                    Insert = "api/encargo/createUpdate"
                }

            };
        }
    }
}

public class CategoriaResource
{
}

public class ClienteResource
{
    public string GetDniTrackingLast { get; set; }

    public string ResetDniTracking { get; set; }

    public string GetByDni { get; set; }

    public string Insert { get; set; }

    public string Update { get; set; }
}

public class HuecoResource
{
    public string Exists { get; set; }

    public string Insert { get; set; }

    public string GetAll { get; set; }

    public string Delete { get; set; }
}

public class PuntoResource
{
    public string GetVentasNoActualizadas { get; set; }
    public string GetSinRedencion { get; set; }
    public string Update { get; set; }
    public string GetLastOfYear { get; set; }
    public string GetByItemVenta { get; set; }
    public string Insert { get; internal set; }
    public string GetUltimaVenta { get; set; }
}

public class ConfiguracionResource
{
    public string GetValorByCampo { get; set; }
    public string UpdateValorByCampo { get; set; }
}

public class EntregaResource
{
    public string GetByKey { get; set; }
    public string Insert { get; internal set; }
}

public class MedicamentoResource
{
    public string GetGreaterOrEqualByCodigoNacional { get; set; }
    public string Delete { get; set; }
    public string ResetSeguimientoSinStock { get; set; }
    public string ResetSeguimientoDondeVoy { get; set; }
    public string GetByCodNacional { get; internal set; }
    public string Insert { get; set; }
    public string Update { get; set; }    
}

public class SinonimoResource
{
    public string IsEmpty { get; set; }
    public string Empty { get; set; }
    public string Insert { get; set; }
}

public class PedidoResource
{
    public string Ultimo { get; set; }
    public string GetByPedido { get; set; }
    public string Insert { get; internal set; }
    public string GetByLineaDePedido { get; set; }
    public string InsertLineaDePedido { get; set; }
}

public class FaltaResource
{
    public string Ultima { get; set; }
    public string GetByLineaDePedido { get; set; }
    public string InsertLineaDePedido { get; set; }
}

public class FamiliaResource
{
    public string GetByFamilia { get; set; }
    public string Insert { get; set; }
}


public class EncargoResource
{
    public string Ultimo { get; set; }
    public string GetByEncargo { get; set; }
    public string Insert { get; set; }
}

public class Credential
{
    public string Username { get; set; }

    public string Password { get; set; }
}