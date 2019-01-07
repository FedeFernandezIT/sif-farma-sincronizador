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
                    Delete = "api/cliente/hueco"
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

public class Credential
{
    public string Username { get; set; }

    public string Password { get; set; }
}