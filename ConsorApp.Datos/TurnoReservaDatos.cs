using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace ConsorApp.Datos
{
    public class TurnoReservaDatos
    {
        private readonly string cadenaConexion =
            "Server=.\\SQLEXPRESS;Database=consorAppDb;Integrated Security=True;TrustServerCertificate=True;";

        // Obtiene todos los turnos disponibles de la base de datos
        public DataTable ObtenerTurnos()
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                string query = @"
                    SELECT 
                        id_Turno AS IdTurno,
                        nombreTurno + ' (' + CAST(horaInicio AS VARCHAR(5)) + ' a ' + CAST(horaFin AS VARCHAR(5)) + ')' AS TextoTurno
                    FROM Turno";

                SqlDataAdapter adaptador = new SqlDataAdapter(query, conexion);
                DataTable dt = new DataTable();
                adaptador.Fill(dt);
                return dt;
            }
        }

        // Inserta la reserva en la tabla Reserva
        public void RegistrarReserva(int idEspacioComun, int idPropietario, int idTurno, DateTime fechaReserva, string codigoSeguimiento)
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();
                string query = @"
                    INSERT INTO Reserva (id_EspacioComun, id_Propietario, id_Turno, fechaReserva, codigoSeguimiento, estadoReserva)
                    VALUES (@IdEspacio, @IdPropietario, @IdTurno, @FechaReserva, @CodigoSeguimiento, 'Confirmada')";

                SqlCommand comando = new SqlCommand(query, conexion);
                comando.Parameters.AddWithValue("@IdEspacio", idEspacioComun);
                comando.Parameters.AddWithValue("@IdPropietario", idPropietario);
                comando.Parameters.AddWithValue("@IdTurno", idTurno);
                comando.Parameters.AddWithValue("@FechaReserva", fechaReserva);
                comando.Parameters.AddWithValue("@CodigoSeguimiento", codigoSeguimiento);

                comando.ExecuteNonQuery();
            }
        }

        // Obtiene las reservas para el panel de administración con LEFT JOIN seguros e incluye el motivo de cancelación
        public DataTable ObtenerReservasAdmin()
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                string query = @"
                    SELECT 
                        r.id_Reserva AS IdReserva,
                        ISNULL(e.nombre, 'Sin Espacio') AS Espacio,
                        ISNULL('Depto ' + CAST(d.piso AS VARCHAR) + d.unidad, 'Sin Unidad') AS Unidad,
                        ISNULL(u.Nombre + ' ' + u.Apellido, 'Propietario ID: ' + CAST(r.id_Propietario AS VARCHAR)) AS Responsable,
                        CONVERT(VARCHAR(10), r.fechaReserva, 103) AS Fecha,
                        ISNULL(t.nombreTurno + ' (' + LEFT(CAST(t.horaInicio AS VARCHAR(8)), 5) + ' a ' + LEFT(CAST(t.horaFin AS VARCHAR(8)), 5) + ')', 'Turno ID: ' + CAST(r.id_Turno AS VARCHAR)) AS Turno,
                        ISNULL(r.estadoReserva, 'Confirmada') AS Estado,
                        r.motivoCancelacion AS MotivoCancelacion
                    FROM Reserva r
                    LEFT JOIN espacioComun e ON r.id_EspacioComun = e.id_espacioComun
                    LEFT JOIN Propietario p ON r.id_Propietario = p.id_Propietario
                    LEFT JOIN Departamento d ON p.id_Departamento = d.id_Departamento
                    LEFT JOIN Usuarios u ON p.Id_Usuario = u.IdUsuario
                    LEFT JOIN Turno t ON r.id_Turno = t.id_Turno
                    ORDER BY r.fechaReserva DESC";

                SqlDataAdapter adaptador = new SqlDataAdapter(query, conexion);
                DataTable dt = new DataTable();
                adaptador.Fill(dt);
                return dt;
            }
        }

        // Cancela una reserva por su ID actualizando el estado y guardando un motivo opcional
        public void CancelarReserva(int idReserva, string motivo)
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();
                string query = "UPDATE Reserva SET estadoReserva = 'Cancelada', motivoCancelacion = @Motivo WHERE id_Reserva = @IdReserva";

                SqlCommand comando = new SqlCommand(query, conexion);
                comando.Parameters.AddWithValue("@IdReserva", idReserva);

                if (string.IsNullOrEmpty(motivo))
                {
                    comando.Parameters.AddWithValue("@Motivo", DBNull.Value);
                }
                else
                {
                    comando.Parameters.AddWithValue("@Motivo", motivo);
                }

                comando.ExecuteNonQuery();
            }
        }

        // Verifica si un espacio está bloqueado y devuelve el detalle (o null si está libre)
        public string VerificarDetalleBloqueo(int idEspacioComun, DateTime fechaReserva)
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();
                string query = @"
                    SELECT TOP 1 fechaHasta, motivo 
                    FROM BloqueoEspacio 
                    WHERE id_espacioComun = @IdEspacio 
                      AND @FechaReserva >= fechaDesde 
                      AND @FechaReserva <= fechaHasta
                    ORDER BY fechaHasta DESC";

                SqlCommand comando = new SqlCommand(query, conexion);
                comando.Parameters.AddWithValue("@IdEspacio", idEspacioComun);
                comando.Parameters.AddWithValue("@FechaReserva", fechaReserva.Date);

                using (SqlDataReader reader = comando.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        DateTime hasta = Convert.ToDateTime(reader["fechaHasta"]);
                        string motivo = reader["motivo"].ToString();

                        return $"El espacio seleccionado se encuentra bloqueado por mantenimiento.\n\nMotivo: {motivo}\nBloqueado hasta el: {hasta:dd/MM/yyyy}";
                    }
                }
                return null;
            }
        }

        public DataTable ObtenerReservasPorPropietario(int idPropietario)
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                string query = @"
                    SELECT 
                        ISNULL(e.nombre, 'Sin Espacio') AS Espacio,
                        CONVERT(VARCHAR(10), r.fechaReserva, 103) AS Fecha,
                        ISNULL(t.nombreTurno + ' (' + LEFT(CAST(t.horaInicio AS VARCHAR(8)), 5) + ' a ' + LEFT(CAST(t.horaFin AS VARCHAR(8)), 5) + ')', 'Turno') AS Turno,
                        ISNULL(r.estadoReserva, 'Confirmada') AS Estado,
                        ISNULL(r.motivoCancelacion, '-') AS MotivoCancelacion
                    FROM Reserva r
                    LEFT JOIN espacioComun e ON r.id_EspacioComun = e.id_espacioComun
                    LEFT JOIN Turno t ON r.id_Turno = t.id_Turno
                    WHERE r.id_Propietario = @IdPropietario
                    ORDER BY r.fechaReserva DESC";

                SqlCommand comando = new SqlCommand(query, conexion);
                comando.Parameters.AddWithValue("@IdPropietario", idPropietario);

                SqlDataAdapter adaptador = new SqlDataAdapter(comando);
                DataTable dt = new DataTable();
                adaptador.Fill(dt);
                return dt;
            }
        }

        // --- NUEVOS MÉTODOS PARA RESERVAS FINALIZADAS E INCIDENTES ---

        // Actualiza automáticamente las reservas confirmadas cuya fecha ya pasó al estado "Finalizada"
        public void ActualizarReservasVencidas()
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();
                string query = @"
                    UPDATE Reserva 
                    SET estadoReserva = 'Finalizada' 
                    WHERE estadoReserva = 'Confirmada' AND fechaReserva < GETDATE()";

                SqlCommand comando = new SqlCommand(query, conexion);
                comando.ExecuteNonQuery();
            }
        }

        // Obtiene todos los incidentes para la grilla del panel de administración
        public DataTable ObtenerIncidentesAdmin()
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                string query = @"
                    SELECT 
                        i.id_Incidente AS Id_Incidente,
                        i.id_Reserva AS IdReserva,
                        ISNULL(e.nombre, 'Sin Espacio') AS Espacio,
                        ISNULL(u.Nombre + ' ' + u.Apellido, 'Propietario') AS Responsable,
                        i.Descripcion AS Descripcion,
                        CONVERT(VARCHAR(10), i.FechaIncidente, 103) AS Fecha,
                        ISNULL(i.EstadoIncidente, 'Pendiente') AS Estado
                    FROM Incidente i
                    LEFT JOIN Reserva r ON i.id_Reserva = r.id_Reserva
                    LEFT JOIN espacioComun e ON r.id_EspacioComun = e.id_espacioComun
                    LEFT JOIN Propietario p ON r.id_Propietario = p.id_Propietario
                    LEFT JOIN Usuarios u ON p.Id_Usuario = u.IdUsuario
                    ORDER BY i.FechaIncidente DESC";

                SqlDataAdapter adaptador = new SqlDataAdapter(query, conexion);
                DataTable dt = new DataTable();
                adaptador.Fill(dt);
                return dt;
            }
        }

        // Inserta un nuevo incidente vinculado a una reserva
        public void InsertarIncidente(int idReserva, string descripcion)
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();
                string query = @"
                    INSERT INTO Incidente (id_Reserva, Descripcion, FechaIncidente, EstadoIncidente)
                    VALUES (@IdReserva, @Descripcion, GETDATE(), 'Pendiente')";

                SqlCommand comando = new SqlCommand(query, conexion);
                comando.Parameters.AddWithValue("@IdReserva", idReserva);
                comando.Parameters.AddWithValue("@Descripcion", descripcion);

                comando.ExecuteNonQuery();
            }
        }

        // Actualiza el estado de un incidente (por ejemplo, a 'Solucionado')
        public void ActualizarEstadoIncidente(int idIncidente, string estado)
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();
                string query = "UPDATE Incidente SET EstadoIncidente = @Estado WHERE id_Incidente = @IdIncidente";

                SqlCommand comando = new SqlCommand(query, conexion);
                comando.Parameters.AddWithValue("@Estado", estado);
                comando.Parameters.AddWithValue("@IdIncidente", idIncidente);

                comando.ExecuteNonQuery();
            }
        }

        public DataTable ObtenerReservasFinalizadas()
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                string query = @"
            SELECT 
                r.id_Reserva AS IdReserva,
                ISNULL(e.nombre, 'Sin Espacio') AS Espacio,
                ISNULL('Depto ' + CAST(d.piso AS VARCHAR) + d.unidad, 'Sin Unidad') AS Unidad,
                ISNULL(u.Nombre + ' ' + u.Apellido, 'Propietario') AS Responsable,
                CONVERT(VARCHAR(10), r.fechaReserva, 103) AS Fecha,
                ISNULL(t.nombreTurno, 'Turno') AS Turno
            FROM Reserva r
            LEFT JOIN espacioComun e ON r.id_EspacioComun = e.id_espacioComun
            LEFT JOIN Propietario p ON r.id_Propietario = p.id_Propietario
            LEFT JOIN Departamento d ON p.id_Departamento = d.id_Departamento
            LEFT JOIN Usuarios u ON p.Id_Usuario = u.IdUsuario
            LEFT JOIN Turno t ON r.id_Turno = t.id_Turno
            WHERE r.estadoReserva = 'Finalizada'
            ORDER BY r.fechaReserva DESC";

                SqlDataAdapter adaptador = new SqlDataAdapter(query, conexion);
                DataTable dt = new DataTable();
                adaptador.Fill(dt);
                return dt;
            }
        }
    }
}