using System;
using System.Linq;
using Sisfarma.Sincronizador.Extensions;
using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Fisiotes;
using Sisfarma.Sincronizador.Fisiotes.Models;
using Sisfarma.Sincronizador.Sincronizadores.SuperTypes;

namespace Sisfarma.Sincronizador.Sincronizadores
{
    public class SinonimoSincronizador : BaseSincronizador
    {
        private readonly string[] _horariosDeVaciamiento;
        private readonly int _batchSize;

        public SinonimoSincronizador(FarmaticService farmatic, FisiotesService fisiotes) 
            : base(farmatic, fisiotes)
        {
             _horariosDeVaciamiento = new[] { "1230", "1730", "1930" };
            _batchSize = 1000;
        }

        public override void Process() => ProcessSinonimos(_farmatic, _fisiotes);

        public void ProcessSinonimos(FarmaticService farmaticService, FisiotesService fisiotesService)
        {
            var isEmpty = fisiotesService.Sinonimos.IsEmpty();
            if (isEmpty || _horariosDeVaciamiento.Any(x => x.Equals(DateTime.Now.ToString("HHmm"))))
            {
                if (!isEmpty) fisiotesService.Sinonimos.Empty();

                var sinonimos = farmaticService.Sinonimos.GetAll();
                
                for (int i = 0; i < sinonimos.Count; i += _batchSize)
                {
                    var items = sinonimos
                        .Skip(i)
                        .Take(_batchSize)
                            .Select(x => new Sinonimo
                            {
                                cod_barras = x.Sinonimo.Strip(),
                                cod_nacional = x.IdArticu.Strip()
                            }).ToList();

                    fisiotesService.Sinonimos.Insert(items);
                }
            }
        }
    }
}
