using Sisfarma.Sincronizador.Farmatic.Models;
using Sisfarma.Sincronizador.Farmatic.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisfarma.Sincronizador.Farmatic
{
    public class FarmaticService
    {
        private FarmaticContext _ctx;

        public ClientesRepository Clientes { get; private set; }
        public DestinatariosRepository Destinatarios { get; private set; }
        public VendedoresRepository Vendedores { get; private set; }
        public VentasRepository Ventas { get; private set; }
        public ArticulosRepository Articulos { get; set; }
        public ProveedoresRepository Proveedores { get; private set; }
        public SinonimosRepository Sinonimos { get; set; }
        public FamiliasRepository Familias { get; set; }
        public LaboratoriosRepository Laboratorios { get; set; }

        public FarmaticService()
        {
            _ctx = new FarmaticContext();
            Clientes = new ClientesRepository(_ctx);
            Destinatarios = new DestinatariosRepository(_ctx);
            Vendedores = new VendedoresRepository(_ctx);
            Ventas = new VentasRepository(_ctx);
            Articulos = new ArticulosRepository(_ctx);
            Proveedores = new ProveedoresRepository(_ctx);
            Sinonimos = new SinonimosRepository(_ctx);
            Familias = new FamiliasRepository(_ctx);
            Laboratorios = new LaboratoriosRepository(_ctx);
        }        
    }
}
