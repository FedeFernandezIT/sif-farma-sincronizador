using System;
using System.Collections.Generic;
using System.Linq;
using Sisfarma.RestClient;
using Sisfarma.Sincronizador.Extensions;
using Sisfarma.Sincronizador.Fisiotes.Models;

namespace Sisfarma.Sincronizador.Fisiotes.Repositories
{
    public class ProveedoresRepository : FisiotesRepository
    {
        public ProveedoresRepository(IRestClient restClient, FisiotesConfig config) 
            : base(restClient, config)
        {
        }

        public DateTime? GetFechaMaximaDeHistorico()
        {
            return _restClient
                .Resource(_config.Proveedores.GetFechaMaximaHistorial)
                .SendGet<FechaMaxima>()
                    .fecha;
        }

        internal class FechaMaxima
        {
            public DateTime? fecha { get; set; }
        }

        public void InsertHistorico(IEnumerable<ProveedorHistorial> items)
        {
            var historicos = items.Select(item => new
            {
                idProveedor = item.idProveedor,
                cod_nacional = item.cod_nacional,
                fecha = item.fecha.ToIsoString(),
                puc = item.puc
            });
                            
            _restClient
                .Resource(_config.Proveedores.InsertHistorico)
                .SendPost(new { bulk = historicos });
        }
    }
}
