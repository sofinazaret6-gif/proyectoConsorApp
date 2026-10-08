using System;
using System.Collections.Generic;
using System.Text;
using System;

namespace ConsorApp.Entidades
{
    public class ExpensaPropietarioItem
    {
        public HistorialExpensa Origen { get; }

        public ExpensaPropietarioItem(HistorialExpensa origen)
        {
            Origen = origen;
        }

        public string Periodo => Origen.Periodo;
        public decimal MontoAPagar => Origen.MontoAPagar;
        public DateTime FechaVencimiento => Origen.FechaVencimiento;
        public string EstadoPago => Origen.EstadoPago;
        public DateTime? FechaPago => Origen.FechaPago;
        public string MetodoPago => Origen.MetodoPago;

        public bool EstaPagada =>
            string.Equals(Origen.EstadoPago, "Pagado", StringComparison.OrdinalIgnoreCase);

        public bool EstaAtrasada =>
            !EstaPagada && Origen.FechaVencimiento.Date < DateTime.Today;

        public int DiasAtraso =>
            EstaAtrasada ? (DateTime.Today - Origen.FechaVencimiento.Date).Days : 0;

        public string DiasAtrasoTexto =>
            EstaAtrasada ? $"{DiasAtraso} días" : "-";

        public string Situacion
        {
            get
            {
                if (EstaPagada) return "Pagada";
                if (EstaAtrasada) return "Atrasada";

                int dias = (Origen.FechaVencimiento.Date - DateTime.Today).Days;
                return dias == 0 ? "Vence hoy" : $"Pendiente (vence en {dias} días)";
            }
        }
    }
}