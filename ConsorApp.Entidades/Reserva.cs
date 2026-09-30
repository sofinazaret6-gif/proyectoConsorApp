using System;

namespace ConsorApp.Entidades
{
    public class Reserva
    {
        public int IdReserva { get; set; }
        public int IdEspacioComun { get; set; }
        public int IdPropietario { get; set; }
        public int IdTurno { get; set; } // Agregado según la base de datos
        public string CodigoSeguimiento { get; set; } = string.Empty;
        public DateTime FechaReserva { get; set; }
        public string EstadoReserva { get; set; } = string.Empty;

        // Propiedades de navegación (si usas Turno como tabla también)
        public EspacioComun? EspacioComun { get; set; }
        public Propietario? Propietario { get; set; }
        public Turno? Turno { get; set; } // la clase Turno
    }
}