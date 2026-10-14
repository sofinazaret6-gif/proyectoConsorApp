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
        public DataTable ObtenerReporteMorosidad()
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
            SUM(ex.valorExpensaindividual) AS Deuda,
            DATEDIFF(day, MIN(ex.fechaEmision), GETDATE()) AS DiasAtraso
        FROM Departamento d
        JOIN Expensa ex ON ex.id_Edificio = d.id_Edificio
        WHERE ex.valorExpensaindividual > 0
        GROUP BY d.unidad, d.id_Departamento
        ORDER BY Deuda DESC;";

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

        public DataTable ObtenerEstadisticasReclamos()
        {
            return ObtenerEstadisticasReclamos(new DateTime(2000, 1, 1), DateTime.Today.AddYears(5));
        }

        public DataTable ObtenerUsoEspaciosComunes()
        {
            return ObtenerUsoEspaciosComunes(new DateTime(2000, 1, 1), DateTime.Today.AddYears(5));
        }

        // --- 2. RECLAMOS FILTRADOS POR PERÍODO ---
        public DataTable ObtenerEstadisticasReclamos(DateTime desde, DateTime hasta)
        {
            const string sql = @"
                SELECT
                    ISNULL(r.Motivo, 'Sin especificar')                               AS Motivo,
                    COUNT(r.id_Reclamo)                                                AS TotalReclamos,
                    SUM(CASE WHEN r.estadoReclamo = 'Resuelto' THEN 1 ELSE 0 END)      AS Resueltos,
                    SUM(CASE WHEN r.estadoReclamo <> 'Resuelto' THEN 1 ELSE 0 END)     AS Pendientes
                FROM Reclamo r
                WHERE r.fechaReclamo BETWEEN @Desde AND @Hasta
                GROUP BY ISNULL(r.Motivo, 'Sin especificar')
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
        public DataTable ObtenerUsoEspaciosComunes(DateTime desde, DateTime hasta)
        {
            const string sql = @"
                SELECT
                    ISNULL(e.nombre, 'Sin especificar')                                              AS Espacio,
                    SUM(CASE WHEN ISNULL(r.estadoReserva, '') <> 'Cancelada' THEN 1 ELSE 0 END)      AS CantidadReservas,
                    SUM(CASE WHEN r.estadoReserva = 'Cancelada' THEN 1 ELSE 0 END)                   AS ReservasCanceladas,
                    0                                                                                AS HorasTotales
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

        // --- 4. RESERVAS POR FRANJA HORARIA ---
        public DataTable ObtenerReservasPorFranja(DateTime desde, DateTime hasta)
        {
            const string sql = @"
                SELECT
                    'General' AS Franja,
                    COUNT(r.id_Reserva) AS CantidadReservas
                FROM Reserva r
                WHERE r.fechaReserva BETWEEN @Desde AND @Hasta
                  AND ISNULL(r.estadoReserva, '') <> 'Cancelada';";

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