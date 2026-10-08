using System;

namespace ConsorApp.Entidades
{
    public class Detalle_Expensa
    {
        public int Id_DetalleExpensa { get; set; }
        public int Id_Expensa { get; set; }
        public int Id_Departamento { get; set; }
        public decimal MontoAPagar { get; set; }
        public DateTime FechaVencimiento { get; set; }

        // Control administrativo del pago
        public string EstadoPago { get; set; } = "Pendiente"; // 'Pendiente' o 'Pagado'
        public DateTime? FechaPago { get; set; }
        public string? MetodoPago { get; set; }

        // Propiedades de navegación
        public Expensa Expensa { get; set; } = null!;
        public Departamento Departamento { get; set; } = null!;

        public bool EstaAtrasada =>
    !string.Equals(EstadoPago, "Pagado", StringComparison.OrdinalIgnoreCase)
    && FechaVencimiento.Date < DateTime.Today;
    }
}