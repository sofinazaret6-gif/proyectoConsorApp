using System;
using System.Data;

namespace ConsorApp.Negocio.Modelos
{
    public class ResumenReportes
    {
        public DataTable Morosidad { get; set; } = null!;
        public DataTable Reclamos { get; set; } = null!;
        public DataTable Espacios { get; set; } = null!;
        public DataTable Franjas { get; set; } = null!;
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }

        public int UnidadesMorosas => Morosidad?.Rows.Count ?? 0;

        public decimal DeudaTotal
        {
            get
            {
                if (Morosidad == null) return 0;
                decimal suma = 0;
                foreach (DataRow fila in Morosidad.Rows)
                    suma += Convert.ToDecimal(fila["Deuda"]);
                return suma;
            }
        }

        public int ReclamosPendientes
        {
            get
            {
                if (Reclamos == null) return 0;
                int suma = 0;
                foreach (DataRow fila in Reclamos.Rows)
                    suma += Convert.ToInt32(fila["Pendientes"]);
                return suma;
            }
        }

        public int ReservasPeriodo
        {
            get
            {
                if (Espacios == null) return 0;
                int suma = 0;
                foreach (DataRow fila in Espacios.Rows)
                    suma += Convert.ToInt32(fila["CantidadReservas"]);
                return suma;
            }
        }
    }
}