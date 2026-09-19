using System;

namespace ConsorApp.Entidades
{
    public class Reclamo
    {
        public int IdReclamo { get; set; }
        public int IdUsuarioDepartamento { get; set; } // Apunta a IdPropietario
        public string Descripcion { get; set; } = string.Empty;
        public string CodigoSeguimiento { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string EstadoReclamo { get; set; } = string.Empty;
        public DateTime FechaReclamo { get; set; }

        public Propietario? Propietario { get; set; }
    }
}