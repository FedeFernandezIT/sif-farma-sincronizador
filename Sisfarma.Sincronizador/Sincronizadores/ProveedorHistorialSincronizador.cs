using System;
using System.Collections.Generic;
using System.Linq;
using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Farmatic.DTO.Recepciones;
using Sisfarma.Sincronizador.Fisiotes;
using Sisfarma.Sincronizador.Fisiotes.Models;
using Sisfarma.Sincronizador.Sincronizadores.SuperTypes;

namespace Sisfarma.Sincronizador.Sincronizadores
{
    public class ProveedorHistorialSincronizador : BaseSincronizador
    {
        private readonly int _batchSize;

        public ProveedorHistorialSincronizador(FarmaticService farmatic, FisiotesService fisiotes) 
            : base(farmatic, fisiotes)
        {
            _batchSize = 1000;
        }

        public override void Process() => ProcessProveedorHistorial();

        private void ProcessProveedorHistorial()
        {
            var fechaMax = _fisiotes.Proveedores.GetFechaMaximaDeHistorico();            
            var recepciones = fechaMax.HasValue
                ? _farmatic.Recepciones.GetGroupGreaterThanByFecha(fechaMax.Value)
                : _farmatic.Recepciones.GetGroupGreaterOrEqualByFecha(DateTime.Now.Date.AddMonths(-2));

            for (int i = 0; i < recepciones.Count(); i += _batchSize)
            {
                _cancellationToken.ThrowIfCancellationRequested();

                var items = recepciones
                    .Skip(i)
                    .Take(_batchSize)
                        .Select(x => new ProveedorHistorial
                        {
                            idProveedor = x.XProv_IdProveedor,
                            cod_nacional = x.XArt_IdArticu,
                            fecha = x.Hora,
                            puc = (decimal) x.ImportePuc
                        }).ToList();

                _fisiotes.Proveedores.InsertHistorico(items);
            }
        }
    }
}
