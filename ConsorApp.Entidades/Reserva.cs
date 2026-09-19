using System;

namespace ConsorApp.Entidades
{
    public class Reserva
    {
        public int IdReserva { get; set; }
        public int IdEspacioComun { get; set; }
        public int IdPropietario { get; set; }
        public string CodigoSeguimiento { get; set; } = string.Empty;
        public DateTime FechaReserva { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public string EstadoReserva { get; set; } = string.Empty;

        public EspacioComun? EspacioComun { get; set; }
        public Propietario? Propietario { get; set; }
    }
}