using System;
using System.IO;
using System.Linq;
using Sisfarma.Sincronizador.Extensions;
using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Fisiotes;
using Sisfarma.Sincronizador.Fisiotes.Models;
using Sisfarma.Sincronizador.Sincronizadores.SuperTypes;

namespace Sisfarma.Sincronizador.Sincronizadores
{
    public class SinonimoSincronizador : TaskSincronizador
    {
        private readonly string[] _horariosDeVaciamiento;
        private readonly int _batchSize;
        private readonly string _fileLogs;

        public SinonimoSincronizador(FarmaticService farmatic, FisiotesService fisiotes)
            : base(farmatic, fisiotes)
        {
            _horariosDeVaciamiento = new[] { "1230", "1730", "1930" };
            _batchSize = 1000;
            _fileLogs = System.Configuration.ConfigurationManager.AppSettings["Directory.Setup"] + @"SinonimosSincronizador.logs";
        }

        public override void Process() => ProcessSinonimos(_farmatic, _fisiotes);

        public void ProcessSinonimos(FarmaticService farmaticService, FisiotesService fisiotesService)
        {
            File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Init process v.133" });
            File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Fisiotes Recuperando sinonimos ..." });
            var isEmpty = fisiotesService.Sinonimos.IsEmpty();
            File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Fisiotes sinonimos recuperado" });
            if (isEmpty || _horariosDeVaciamiento.Any(x => x.Equals(DateTime.Now.ToString("HHmm"))))
            {
                if (!isEmpty)
                {
                    File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Fisiotes limpiando sinonimos ..." });
                    fisiotesService.Sinonimos.Empty();
                    File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Fisiotes sinonimos limpiados" });
                }

                File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + " Farmatic Recuperando sinonimos ..." });
                var sinonimos = farmaticService.Sinonimos.GetAll();
                File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + $" Farmatic sininimos recuperados {sinonimos.Count}" });

                for (int i = 0; i < sinonimos.Count; i += _batchSize)
                {
                    _cancellationToken.ThrowIfCancellationRequested();

                    File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + $" Farmatic preparando lote de {_batchSize} ..." });
                    var items = sinonimos
                        .Skip(i)
                        .Take(_batchSize)
                            .Select(x => new Sinonimo
                            {
                                cod_barras = x.Sinonimo.Strip(),
                                cod_nacional = x.IdArticu.Strip()
                            }).ToList();
                    File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + $" Farmatic lote preparado" });
                    File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + $" Fisiotes insertando sinonimos ..." });
                    fisiotesService.Sinonimos.Insert(items);
                    File.AppendAllLines(_fileLogs, new[] { DateTime.UtcNow.ToString("o") + $" Fisiotes sinonimos insertados" });
                }
            }
        }
    }
}