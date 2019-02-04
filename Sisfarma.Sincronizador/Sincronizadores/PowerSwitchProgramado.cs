using Sisfarma.Sincronizador.Extensions;
using Sisfarma.Sincronizador.Fisiotes;
using Sisfarma.Sincronizador.Fisiotes.Models;
using Sisfarma.Sincronizador.Sincronizadores.SuperTypes;
using System;

namespace Sisfarma.Sincronizador.Sincronizadores
{
    public class PowerSwitchProgramado : PowerSwitch
    {
        public PowerSwitchProgramado(FisiotesService fisiotes)
            : base(fisiotes) 
            => Encender();

        public override void Process() => ProcessPowerSwitch();

        private void ProcessPowerSwitch()
        {
            if (EsHorario(Programacion.Encendido) && !EstaEncendido)            
                Encender();            
            else if (EsHorario(Programacion.Apagado) && EstaEncendido)            
                Apagar();            
        }

        protected override void Encender()
        {
            base.Encender();
            _fisiotes.Configuraciones.Update(FIELD_ENCENDIDO, Programacion.Encendido);
        }

        protected override void Apagar()
        {
            base.Apagar();
            _fisiotes.Configuraciones.Update(FIELD_ENCENDIDO, Programacion.Apagado);
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
