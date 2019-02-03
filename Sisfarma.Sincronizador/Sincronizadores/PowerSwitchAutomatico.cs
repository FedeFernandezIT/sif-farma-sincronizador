using Sisfarma.Sincronizador.Extensions;
using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Fisiotes;
using Sisfarma.Sincronizador.Fisiotes.Models;
using Sisfarma.Sincronizador.Sincronizadores.SuperTypes;
using System;

namespace Sisfarma.Sincronizador.Sincronizadores
{
    public class PowerSwitchAutomatico : BaseSincronizador
    {
        public static bool EstaEncendido;        
        
        public PowerSwitchAutomatico(FarmaticService farmatic, FisiotesService fisiotes)
            : base(farmatic, fisiotes)
        {
            SincronizadorTaskManager.PowerOn();
            EstaEncendido = true;
        }
        
        public override void Process() => ProcessPowerSwitch();

        private void ProcessPowerSwitch()
        {
            if (EsHorario(Programacion.Encendido) && !EstaEncendido)
            {
                SincronizadorTaskManager.PowerOn();
                EstaEncendido = true;
            }                
            else if (EsHorario(Programacion.Apagado) && EstaEncendido)
            {
                SincronizadorTaskManager.PowerOff();
                EstaEncendido = false;
            }
        }

        private bool EsHorario(string horario)
        {
            var programacion = _fisiotes.Programacion.GetProgramacionOrDefault(horario);
            if (programacion == null)
                return false;

            var horaActual = DateTime.Now.ToTimeInteger();
            if (TimeSpan.TryParse(programacion.horaM, out var mañana))
            {
                var hora = DateTime.Today.Add(mañana).ToTimeInteger();
                if (horaActual == hora)
                    return true;
            }
            if (TimeSpan.TryParse(programacion.horaT, out var tarde))
            {
                var hora = DateTime.Today.Add(tarde).ToTimeInteger();
                return horaActual == hora;
            }
            return false;
        }        
    }
}
