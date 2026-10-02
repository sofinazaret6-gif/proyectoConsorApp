using System;

namespace ConsorApp.Entidades
{
    public class HistorialExpensa
    {
        public int IdDetalleExpensa { get; set; }

        public int IdExpensa { get; set; }

        public int IdDepartamento { get; set; }

        public string Piso { get; set; } = string.Empty;

        public string Unidad { get; set; } = string.Empty;

        public int IdPropietario { get; set; }

        public int IdUsuario { get; set; }

        public string NombrePropietario { get; set; } = string.Empty;

        public string Periodo { get; set; } = string.Empty;

        public decimal MontoAPagar { get; set; }

        public string EstadoPago { get; set; } = string.Empty;

        public DateTime FechaVencimiento { get; set; }

        public DateTime? FechaPago { get; set; }

        public string MetodoPago { get; set; } = string.Empty;

        public DateTime FechaDesdePropietario { get; set; }

        public DateTime? FechaHastaPropietario { get; set; }
    }
}