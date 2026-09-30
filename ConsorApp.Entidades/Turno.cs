using System;
using System.Collections.Generic;

namespace ConsorApp.Entidades
{
    public class Turno
    {
        public int Id_Turno { get; set; }
        public string NombreTurno { get; set; } = string.Empty;
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }

        // Relación opcional con Reservas si querés navegar desde el Turno
        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    }
}