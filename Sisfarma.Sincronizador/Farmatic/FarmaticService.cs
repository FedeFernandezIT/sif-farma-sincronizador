﻿using Sisfarma.Sincronizador.Farmatic.Models;
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
        public ArticulosRepository Articulos { get; private set; }
        public ProveedoresRepository Proveedores { get; private set; }
        public SinonimosRepository Sinonimos { get; private set; }
        public FamiliasRepository Familias { get; private set; }
        public LaboratoriosRepository Laboratorios { get; private set; }
        public RecepcionesRepository Recepciones { get; private set; }
        public ListasArticulosRepository ListasArticulos { get; private set; }
        public EncargosRepository Encargos { get; private set; }
        public PedidosRepository Pedidos { get; private set; }

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
            Recepciones = new RecepcionesRepository(_ctx);
            ListasArticulos = new ListasArticulosRepository(_ctx);
            Encargos = new EncargosRepository(_ctx);
            Pedidos = new PedidosRepository(_ctx);
        }        
    }
}
