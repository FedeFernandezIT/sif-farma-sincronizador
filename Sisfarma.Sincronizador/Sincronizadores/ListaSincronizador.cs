using System;
using System.Linq;
using Sisfarma.Sincronizador.Extensions;
using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Fisiotes;
using Sisfarma.Sincronizador.Sincronizadores.SuperTypes;

namespace Sisfarma.Sincronizador.Sincronizadores
{
    public class ListaSincronizador : TaskSincronizador
    {
        private const int BATCH_SIZE = 1000;

        public ListaSincronizador(FarmaticService farmatic, FisiotesService fisiotes) 
            : base(farmatic, fisiotes)
        {
        }

        public override void Process() => ProcessListas(_farmatic, _fisiotes);

        private void ProcessListas(FarmaticService farmatic, FisiotesService fisiotes)
        {
            var codActual = fisiotes.Listas.GetCodPorDondeVoyOrDefault()?.cod ?? -1;
            var listas = farmatic.ListasArticulos.GetByIdGreaterThan(codActual);
            foreach (var lista in listas)
            {
                _cancellationToken.ThrowIfCancellationRequested();

                fisiotes.Listas.ResetPorDondeVoy();
                fisiotes.Listas.InsertOrUpdate(new Fisiotes.Models.Lista
                {
                    cod = lista.IdLista,
                    lista = lista.Descripcion.Strip()                    
                });
                
                var articulos = farmatic.ListasArticulos.GetArticulosByLista(lista.IdLista);
                if (articulos.Any())
                {
                    fisiotes.Listas.DeArticulos.Delete(lista.IdLista);                    

                    for (int i = 0; i < articulos.Count; i += BATCH_SIZE)
                    {
                        var items = articulos
                            .Skip(i)
                            .Take(BATCH_SIZE)
                            .Select(x => new Fisiotes.Models.ListaArticulo
                            {
                                cod_lista = x.XItem_IdLista,
                                cod_articulo = x.XItem_IdArticu.ToIntegerOrDefault(-1)
                            }).ToList();

                        fisiotes.Listas.DeArticulos.Insert(items);
                    }
                }
            }

            fisiotes.Listas.ResetPorDondeVoy();
        }
    }
}
