using System;
using System.Data;

namespace ConsorApp.Negocio.Modelos
{
    /// <summary>Datos del edificio que van en el encabezado de los PDF.</summary>
    public class DatosEdificio
    {
        public string Nombre { get; set; } = "Consorcio";
        public string Ubicacion { get; set; } = "Sin especificar";
        public string CantDeptos { get; set; } = "-";
        public string CantPisos { get; set; } = "-";

        public static DatosEdificio Desde(DataTable? dt)
        {
            var edificio = new DatosEdificio();
            if (dt == null || dt.Rows.Count == 0) return edificio;

            DataRow fila = dt.Rows[0];
            edificio.Nombre = Texto(fila, "descripcion", edificio.Nombre);
            edificio.Ubicacion = Texto(fila, "ubicacion", edificio.Ubicacion);
            edificio.CantDeptos = Texto(fila, "CantDepto", edificio.CantDeptos);
            edificio.CantPisos = Texto(fila, "cantPisos", edificio.CantPisos);
            return edificio;
        }

        private static string Texto(DataRow fila, string columna, string porDefecto)
        {
            if (!fila.Table.Columns.Contains(columna) || fila[columna] == DBNull.Value)
                return porDefecto;

            string valor = fila[columna].ToString() ?? "";
            return string.IsNullOrWhiteSpace(valor) ? porDefecto : valor.Trim();
        }
    }

    public class ResumenReportes
    {
        public DataTable Morosidad { get; set; } = null!;
        public DataTable Reclamos { get; set; } = null!;
        public DataTable Espacios { get; set; } = null!;
        public DataTable Franjas { get; set; } = null!;
        public DataTable EspacioTurno { get; set; } = null!;
        public DataTable Gastos { get; set; } = null!;
        public DatosEdificio Edificio { get; set; } = new DatosEdificio();
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
                    suma += Decimal(fila, "Deuda");
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
                    suma += Entero(fila, "Pendientes");
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
                    suma += Entero(fila, "CantidadReservas");
                return suma;
            }
        }

        public decimal TotalGastos
        {
            get
            {
                if (Gastos == null) return 0;
                decimal suma = 0;
                foreach (DataRow fila in Gastos.Rows)
                    suma += Decimal(fila, "TotalGastado");
                return suma;
            }
        }

        // Antes un NULL en la base hacía explotar Convert.ToDecimal / ToInt32
        private static decimal Decimal(DataRow fila, string col) =>
            fila[col] == DBNull.Value ? 0m : Convert.ToDecimal(fila[col]);

        private static int Entero(DataRow fila, string col) =>
            fila[col] == DBNull.Value ? 0 : Convert.ToInt32(fila[col]);
    }
}