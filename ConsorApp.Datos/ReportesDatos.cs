using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace ConsorApp.Datos
{
    public class ReportesDatos
    {
        private readonly string _cadenaConexion =
            @"Server=.\SQLEXPRESS;Database=consorAppDb;Integrated Security=True;TrustServerCertificate=True;";

        // --- 1. MOROSIDAD Y DEUDAS ---
        // Sin parámetros: toda la deuda vencida e impaga (sin límite de fechas).
        public DataTable ObtenerReporteMorosidad()
        {
            return ObtenerReporteMorosidad(new DateTime(2000, 1, 1), DateTime.Today);
        }

        // Con rango: "Hasta" es la FECHA DE CORTE (si es futura se usa hoy).
        // Entran las expensas cuyo vencimiento cae entre Desde y el corte, ya vencidas a esa
        // fecha y que seguían impagas en ese momento (impagas hoy, o pagadas después del corte).
        // Los días de atraso van desde el vencimiento más antiguo del departamento hasta el corte.
        // Es un reporte: una sola fila por departamento con la deuda acumulada.
        public DataTable ObtenerReporteMorosidad(DateTime desde, DateTime hasta)
        {
            const string sql = @"
        SELECT
            d.unidad AS Unidad,
            ISNULL((
                SELECT TOP 1 us.Nombre + ' ' + us.Apellido
                FROM Propietario p
                JOIN Usuarios us ON us.IdUsuario = p.Id_Usuario
                WHERE p.id_Departamento = d.id_Departamento AND us.Estado = 1
            ), 'Sin asignar') AS Propietario,
            SUM(de.MontoAPagar) AS Deuda,
            DATEDIFF(DAY, MIN(de.FechaVencimiento), @Corte) AS DiasAtraso
        FROM Departamento d
        JOIN Detalle_Expensa de ON de.Id_Departamento = d.id_Departamento
        WHERE (ISNULL(de.EstadoPago, 'Pendiente') <> 'Pagado'
               OR de.FechaPago >= DATEADD(DAY, 1, @Corte))   -- seguía impaga al corte
          AND de.FechaVencimiento <  @Corte                  -- ya vencida al corte
          AND de.FechaVencimiento >= @Desde
        GROUP BY d.unidad, d.id_Departamento
        ORDER BY Deuda DESC;";

            using (var cn = new SqlConnection(_cadenaConexion))
            using (var cmd = new SqlCommand(sql, cn))
            using (var da = new SqlDataAdapter(cmd))
            {
                // Fecha de corte: el "Hasta" elegido, sin pasar de hoy
                DateTime corte = hasta.Date > DateTime.Today ? DateTime.Today : hasta.Date;

                cmd.Parameters.Add("@Desde", SqlDbType.Date).Value = desde.Date;
                cmd.Parameters.Add("@Corte", SqlDbType.Date).Value = corte;

                var tabla = new DataTable();
                cn.Open();
                da.Fill(tabla);
                return tabla;
            }
        }

        public DataTable ObtenerEstadisticasReclamos()
        {
            return ObtenerEstadisticasReclamos(new DateTime(2000, 1, 1), DateTime.Today.AddYears(5));
        }

        public DataTable ObtenerUsoEspaciosComunes()
        {
            return ObtenerUsoEspaciosComunes(new DateTime(2000, 1, 1), DateTime.Today.AddYears(5));
        }

        // --- 2. RECLAMOS POR CATEGORÍA, FILTRADOS POR PERÍODO ---
        public DataTable ObtenerEstadisticasReclamos(DateTime desde, DateTime hasta)
        {
            const string sql = @"
                SELECT
                    ISNULL(r.Categoria, 'Otro')                                        AS Categoria,
                    COUNT(r.id_Reclamo)                                                AS TotalReclamos,
                    SUM(CASE WHEN r.estadoReclamo = 'Resuelto' THEN 1 ELSE 0 END)      AS Resueltos,
                    SUM(CASE WHEN r.estadoReclamo <> 'Resuelto' THEN 1 ELSE 0 END)     AS Pendientes
                FROM Reclamo r
                WHERE r.fechaReclamo BETWEEN @Desde AND @Hasta
                GROUP BY ISNULL(r.Categoria, 'Otro')
                ORDER BY COUNT(r.id_Reclamo) DESC;";

            using (var cn = new SqlConnection(_cadenaConexion))
            using (var cmd = new SqlCommand(sql, cn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.Add("@Desde", SqlDbType.DateTime).Value = desde;
                cmd.Parameters.Add("@Hasta", SqlDbType.DateTime).Value =
                    hasta.Date.AddDays(1).AddSeconds(-1);

                var tabla = new DataTable();
                cn.Open();
                da.Fill(tabla);
                return tabla;
            }
        }

        // --- 3. USO DE ESPACIOS FILTRADO POR PERÍODO (CON CONTEO DE CANCELADAS) ---
        // --- 3. USO DE ESPACIOS FILTRADO POR PERÍODO (CON CONTEO DE CANCELADAS) ---
        public DataTable ObtenerUsoEspaciosComunes(DateTime desde, DateTime hasta)
        {
            const string sql = @"
                SELECT
                    ISNULL(e.nombre, 'Sin especificar')                                                     AS Espacio,
                    SUM(CASE WHEN r.id_Reserva IS NOT NULL AND ISNULL(r.estadoReserva, '') <> 'Cancelada' THEN 1 ELSE 0 END) AS CantidadReservas,
                    SUM(CASE WHEN r.id_Reserva IS NOT NULL AND r.estadoReserva = 'Cancelada' THEN 1 ELSE 0 END)             AS ReservasCanceladas,
                    0                                                                                        AS HorasTotales
                FROM espacioComun e
                LEFT JOIN Reserva r
                       ON r.id_EspacioComun = e.id_EspacioComun
                      AND r.fechaReserva BETWEEN @Desde AND @Hasta
                GROUP BY ISNULL(e.nombre, 'Sin especificar')
                ORDER BY CantidadReservas DESC;";

            using (var cn = new SqlConnection(_cadenaConexion))
            using (var cmd = new SqlCommand(sql, cn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.Add("@Desde", SqlDbType.DateTime).Value = desde;
                cmd.Parameters.Add("@Hasta", SqlDbType.DateTime).Value =
                    hasta.Date.AddDays(1).AddSeconds(-1);

                var tabla = new DataTable();
                cn.Open();
                da.Fill(tabla);
                return tabla;
            }
        }

        // --- 4. RESERVAS POR TURNO ---
        // Una fila por turno (Mañana / Tarde / Noche), incluso los que no tuvieron reservas.
        // No cuenta las canceladas, igual que el reporte de uso de espacios.
        public DataTable ObtenerReservasPorFranja(DateTime desde, DateTime hasta)
        {
            const string sql = @"
                SELECT
                    t.id_Turno                                                       AS IdTurno,
                    t.nombreTurno                                                    AS Franja,
                    CONVERT(VARCHAR(5), t.horaInicio, 108) + ' a '
                      + CONVERT(VARCHAR(5), t.horaFin, 108)                          AS Horario,
                    COUNT(r.id_Reserva)                                              AS CantidadReservas
                FROM Turno t
                LEFT JOIN Reserva r
                       ON r.id_Turno = t.id_Turno
                      AND r.fechaReserva BETWEEN @Desde AND @Hasta
                      AND ISNULL(r.estadoReserva, '') <> 'Cancelada'
                GROUP BY t.id_Turno, t.nombreTurno, t.horaInicio, t.horaFin
                ORDER BY t.horaInicio;";

            using (var cn = new SqlConnection(_cadenaConexion))
            using (var cmd = new SqlCommand(sql, cn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.Add("@Desde", SqlDbType.DateTime).Value = desde;
                cmd.Parameters.Add("@Hasta", SqlDbType.DateTime).Value =
                    hasta.Date.AddDays(1).AddSeconds(-1);

                var tabla = new DataTable();
                cn.Open();
                da.Fill(tabla);
                return tabla;
            }
        }

        // --- 4b. RESERVAS POR ESPACIO Y TURNO (cruce completo, con ceros) ---
        public DataTable ObtenerReservasEspacioTurno(DateTime desde, DateTime hasta)
        {
            const string sql = @"
                SELECT
                    e.nombre                AS Espacio,
                    t.nombreTurno           AS Turno,
                    COUNT(r.id_Reserva)     AS CantidadReservas
                FROM espacioComun e
                CROSS JOIN Turno t
                LEFT JOIN Reserva r
                       ON r.id_EspacioComun = e.id_EspacioComun
                      AND r.id_Turno = t.id_Turno
                      AND r.fechaReserva BETWEEN @Desde AND @Hasta
                      AND ISNULL(r.estadoReserva, '') <> 'Cancelada'
                GROUP BY e.nombre, t.nombreTurno, t.horaInicio
                ORDER BY e.nombre, t.horaInicio;";

            using (var cn = new SqlConnection(_cadenaConexion))
            using (var cmd = new SqlCommand(sql, cn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.Add("@Desde", SqlDbType.DateTime).Value = desde;
                cmd.Parameters.Add("@Hasta", SqlDbType.DateTime).Value =
                    hasta.Date.AddDays(1).AddSeconds(-1);

                var tabla = new DataTable();
                cn.Open();
                da.Fill(tabla);
                return tabla;
            }
        }

        // --- 5. GASTOS POR CONCEPTO (en qué se va la plata) ---
        // Una fila por concepto con el total gastado, cantidad de gastos, % del total y ranking.
        // Ignora los gastos anulados. El rango filtra por la fecha del gasto.
        public DataTable ObtenerGastosPorConcepto(DateTime desde, DateTime hasta)
        {
            const string sql = @"
                SELECT
                    ROW_NUMBER() OVER (ORDER BY SUM(g.monto) DESC)                         AS Ranking,
                    c.nombreConcepto                                                       AS Concepto,
                    COUNT(g.id_Gasto)                                                      AS CantidadGastos,
                    SUM(g.monto)                                                           AS TotalGastado,
                    AVG(g.monto)                                                           AS PromedioGasto,
                    CAST(SUM(g.monto) * 100.0 / NULLIF(SUM(SUM(g.monto)) OVER (), 0)
                         AS DECIMAL(5,2))                                                  AS Porcentaje
                FROM Gasto_Edificio g
                INNER JOIN conceptoGasto c
                        ON g.Id_conceptoGasto = c.id_conceptoGasto
                WHERE ISNULL(g.estado, '') <> 'ANULADO'
                  AND g.fecha >= @Desde
                  AND g.fecha <  DATEADD(DAY, 1, @Hasta)
                GROUP BY c.nombreConcepto
                ORDER BY TotalGastado DESC;";

            using (var cn = new SqlConnection(_cadenaConexion))
            using (var cmd = new SqlCommand(sql, cn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.Add("@Desde", SqlDbType.Date).Value = desde.Date;
                cmd.Parameters.Add("@Hasta", SqlDbType.Date).Value = hasta.Date;

                var tabla = new DataTable();
                cn.Open();
                da.Fill(tabla);
                return tabla;
            }
        }

        public DataTable ObtenerDatosEdificio()
        {
            const string sql = @"SELECT TOP 1 descripcion, ubicacion, CantDepto, cantPisos FROM Edificio;";

            using (var cn = new SqlConnection(_cadenaConexion))
            using (var cmd = new SqlCommand(sql, cn))
            using (var da = new SqlDataAdapter(cmd))
            {
                var tabla = new DataTable();
                cn.Open();
                da.Fill(tabla);
                return tabla;
            }
        }
    }
}