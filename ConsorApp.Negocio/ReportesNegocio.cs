using System;
using System.Data;
using ConsorApp.Datos;
using ConsorApp.Negocio.Modelos;

namespace ConsorApp.Negocio
{
    public class ReportesNegocio
    {
        private readonly ReportesDatos _datos = new ReportesDatos();

        // --- Métodos existentes, intactos ---
        public DataTable ObtenerReporteMorosidad()
        {
            return _datos.ObtenerReporteMorosidad();
        }

        public DataTable ObtenerEstadisticasReclamos()
        {
            return _datos.ObtenerEstadisticasReclamos();
        }

        public DataTable ObtenerUsoEspaciosComunes()
        {
            return _datos.ObtenerUsoEspaciosComunes();
        }

        // --- NUEVO: resumen completo del período ---
        public ResumenReportes ObtenerResumen(DateTime desde, DateTime hasta)
        {
            if (hasta.Date < desde.Date)
                throw new ArgumentException("La fecha 'hasta' no puede ser anterior a 'desde'.");

            return new ResumenReportes
            {
                Desde = desde,
                Hasta = hasta,
                Morosidad = _datos.ObtenerReporteMorosidad(),
                Reclamos = _datos.ObtenerEstadisticasReclamos(desde, hasta),
                Espacios = _datos.ObtenerUsoEspaciosComunes(desde, hasta),
                Franjas = _datos.ObtenerReservasPorFranja(desde, hasta)
            };
        }
    }
}