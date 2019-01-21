using Sisfarma.Sincronizador.Consejo;
using Sisfarma.Sincronizador.Extensions;
using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Farmatic.Models;
using Sisfarma.Sincronizador.Fisiotes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Sisfarma.Sincronizador.Fisiotes.Repositories.ConfiguracionesRepository;

namespace Sisfarma.Sincronizador
{
    public class SincronizadorApplication : ApplicationContext
    {

        private string _remoteBase, _remoteServer, _remoteUsername, _remotePassword;
        private string _localBase, _localServer, _localUser, _localPass;
        private int _marketCodeList;



        private System.Timers.Timer timerListasFechas;




        public SincronizadorApplication()
        {                        
        }

        private void InitializeTimer()
        {            

            timerListasFechas = new System.Timers.Timer(62000);
            timerListasFechas.Elapsed += (sender, @event) =>
            {
                //timerListasFechas.Stop();
                //FarmaticService farmatic = new FarmaticService();
                //FisiotesService fisiotes = new FisiotesService();
                //ProcessListasFechas(farmatic, fisiotes);
                //timerListasFechas.Start();
            };



        }

        
                

        

        

        //public void ProcessListasFechas(FarmaticService farmatic, FisiotesService fisiotes)
        //{
        //    try
        //    {
        //        var listas = farmatic.ListasArticulos.GetByFechaExceptList(_marketCodeList);
        //        foreach (var lista in listas)
        //        {
        //            var listaRemote = fisiotes.Listas.Get(lista.IdLista);
        //            if (listaRemote == null)
        //                fisiotes.Listas.Insert(lista.IdLista, lista.Descripcion.Strip());
        //            else
        //                fisiotes.Listas.Update(lista.IdLista, lista.Descripcion.Strip());

        //            fisiotes.Listas.DeArticulos.Delete(lista.IdLista);
        //            var articulos = farmatic.ListasArticulos.GetArticulosByLista(lista.IdLista);
        //            foreach (var articulo in articulos)
        //            {
        //                fisiotes.Listas.DeArticulos.Insert(articulo.XItem_IdLista, Convert.ToInt32(articulo.XItem_IdArticu));
        //            }
        //            if (articulos.Count != 0)
        //            {
        //                var take = 1000;
        //                for (int i = 0; i < articulos.Count; i += take)
        //                {
        //                    var items = articulos.Skip(i).Take(1000).Select(x => new Fisiotes.Models.ListaArticulo
        //                    {
        //                        cod_lista = x.XItem_IdLista,
        //                        cod_articulo = Convert.ToInt32(x.XItem_IdArticu)
        //                    }).ToList();
        //                    fisiotes.Listas.DeArticulos.Insert(items);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Task.Delay(1500).Wait();
        //    }
        //}

    }
}