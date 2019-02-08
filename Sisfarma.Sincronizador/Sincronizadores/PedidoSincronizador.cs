using System;
using System.Linq;
using Sisfarma.Sincronizador.Consejo;
using Sisfarma.Sincronizador.Extensions;
using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Farmatic.Models;
using Sisfarma.Sincronizador.Fisiotes;
using Sisfarma.Sincronizador.Sincronizadores.SuperTypes;
using static Sisfarma.Sincronizador.Fisiotes.Repositories.ConfiguracionesRepository;

namespace Sisfarma.Sincronizador.Sincronizadores
{
    public class PedidoSincronizador : TaskSincronizador
    {
        private const string YEAR_FOUND = FieldsConfiguracion.FIELD_ANIO_INICIO;
        private const string LABORATORIO_DEFAULT = "<Sin Laboratorio>";
        private const string FAMILIA_DEFAULT = "<Sin Clasificar>";        

        private ConsejoService _consejo;

        public PedidoSincronizador(FarmaticService farmatic, FisiotesService fisiotes, ConsejoService consejo) 
            : base(farmatic, fisiotes)
        {
            _consejo = consejo ?? throw new ArgumentNullException(nameof(consejo));
        }

        public override void Process() => ProcessPedidos();

        private  void ProcessPedidos()
        {
            var anioInicio = _fisiotes.Configuraciones.GetByCampo(YEAR_FOUND)
                .ToIntegerOrDefault(@default: DateTime.Now.Year - 2);

            var lastPedido = _fisiotes.Pedidos.LastOrDefault();
            var recepciones = (lastPedido == null)
                ? _farmatic.Recepciones.GetByYear(anioInicio)
                : _farmatic.Recepciones.GetByIdAndYear(anioInicio, lastPedido.idPedido);

            foreach (var recepcion in recepciones)
            {
                _cancellationToken.ThrowIfCancellationRequested();

                var resume = _farmatic.Recepciones.GetResumeById(recepcion.IdRecepcion);
                if (resume.numLineas > 0)
                {
                    if (!_fisiotes.Pedidos.Exists(recepcion.IdRecepcion))                                            
                        _fisiotes.Pedidos.Insert(GenerarPedido(_farmatic, recepcion, resume));                        
                    
                    var lineas = _farmatic.Recepciones.GetLineasById(recepcion.IdRecepcion)
                            .Where(l => !string.IsNullOrEmpty(l.XArt_IdArticu));

                    foreach (var linea in lineas)
                    {                        
                        var articulo = _farmatic.Articulos.GetOneOrDefaultById(linea.XArt_IdArticu);
                        if (articulo != null)
                        {                            
                            if (!_fisiotes.Pedidos.ExistsLinea(linea.IdRecepcion, linea.IdNLinea))                                
                                _fisiotes.Pedidos.InsertLinea(GenerarLineaDePedido(_farmatic, recepcion, linea, articulo, _consejo));                                                                    
                        }                                                                                                                                                                                     
                    }
                }
                                    
            }            
        }

        private Fisiotes.Models.LineaPedido GenerarLineaDePedido(FarmaticService farmatic, Recepcion recepcion, LineaRecepcion linea, Articulo articulo, ConsejoService consejo)
        {            
            var puc = linea.ImportePuc;
            var pvp = linea.ImportePvp;

            var familia = farmatic.Familias.GetById(articulo.XFam_IdFamilia)?.Descripcion;
            if (string.IsNullOrWhiteSpace(familia))
                familia = FAMILIA_DEFAULT;

            var superFamilia = !familia.Equals(FAMILIA_DEFAULT)
                ? farmatic.Familias.GetSuperFamiliaDescripcionByFamilia(familia) ?? FAMILIA_DEFAULT
                : familia;

            var codLaboratorio = articulo.Laboratorio ?? string.Empty;
            var nombreLaboratorio = GetNombreLaboratorioFromLocalOrDefault(farmatic, consejo, codLaboratorio, LABORATORIO_DEFAULT);

            return new Fisiotes.Models.LineaPedido
            {
                idPedido = linea.IdRecepcion,
                idLinea = linea.IdNLinea,
                fechaPedido = recepcion.Hora,
                cod_nacional = Convert.ToInt64(articulo.IdArticu.Strip()),
                descripcion = articulo.Descripcion.Strip(),
                familia = familia.Strip(),
                superFamilia = superFamilia.Strip(),
                cantidad = linea.Recibidas,
                pvp = Convert.ToSingle(pvp),
                puc = Convert.ToSingle(puc),
                cod_laboratorio = codLaboratorio.Strip(),
                laboratorio = nombreLaboratorio.Strip()
            };            
        }

        private Fisiotes.Models.Pedido GenerarPedido(FarmaticService farmatic, Recepcion recepcion, RecepcionResume resume)
        {
            var proveedor = farmatic.Proveedores.GetById(recepcion.XProv_IdProveedor)?.FIS_NOMBRE
                            ?? string.Empty;

            var trabajador = farmatic.Vendedores.GetOneOrDefaultById(Convert.ToInt16(recepcion.XVend_IdVendedor))?.NOMBRE
                ?? string.Empty;            

            return new Fisiotes.Models.Pedido
            {
                idPedido = recepcion.IdRecepcion,
                fechaPedido = recepcion.Hora,
                hora = DateTime.Now,
                numLineas = resume.numLineas,
                importePvp = Convert.ToSingle(resume.importePvp),
                importePuc = Convert.ToSingle(resume.importePuc),
                idProveedor = recepcion.XProv_IdProveedor,
                proveedor = proveedor,
                trabajador = trabajador
            };
        }

        private string GetNombreLaboratorioFromLocalOrDefault(FarmaticService farmaticService, ConsejoService consejoService, string codigo, string byDefault = "")
        {
            var nombreLaboratorio = byDefault;
            if (!string.IsNullOrEmpty(codigo?.Trim()) && !string.IsNullOrWhiteSpace(codigo))
            {
                var laboratorioDb = default(Consejo.Models.Labor); //consejoService.Laboratorios.Get(codigo);
                if (laboratorioDb == null)
                {
                    var laboratorioLocal =
                        farmaticService.Laboratorios.GetById(codigo);
                    nombreLaboratorio = laboratorioLocal?.Nombre ?? byDefault;
                }
                else nombreLaboratorio = laboratorioDb.NOMBRE;
            }
            else nombreLaboratorio = byDefault;
            return nombreLaboratorio;
        }
    }
}
