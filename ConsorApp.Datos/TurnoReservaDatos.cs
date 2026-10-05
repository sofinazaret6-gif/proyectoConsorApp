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

        // Obtiene las reservas para el panel de administración con LEFT JOIN seguros
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
                        ISNULL(r.estadoReserva, 'Confirmada') AS Estado
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

        // Cancela (o elimina/actualiza estado) una reserva por su ID
        public void CancelarReserva(int idReserva)
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();
                // Opción A: Actualizar estado a 'Cancelada'
                string query = "UPDATE Reserva SET estadoReserva = 'Cancelada' WHERE id_Reserva = @IdReserva";

                // Opción B (si prefieres borrarla físicamente): 
                // string query = "DELETE FROM Reserva WHERE id_Reserva = @IdReserva";

                SqlCommand comando = new SqlCommand(query, conexion);
                comando.Parameters.AddWithValue("@IdReserva", idReserva);
                comando.ExecuteNonQuery();
            }
        }

        // Verifica si un espacio está bloqueado en una fecha específica
        public bool EstaEspacioBloqueo(int idEspacioComun, DateTime fechaReserva)
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();
                string query = @"
                    SELECT COUNT(1) 
                    FROM BloqueoEspacio 
                    WHERE id_espacioComun = @IdEspacio 
                      AND @FechaReserva >= fechaDesde 
                      AND @FechaReserva <= fechaHasta";

                SqlCommand comando = new SqlCommand(query, conexion);
                comando.Parameters.AddWithValue("@IdEspacio", idEspacioComun);
                comando.Parameters.AddWithValue("@FechaReserva", fechaReserva.Date);

                int resultado = Convert.ToInt32(comando.ExecuteScalar());
                return resultado > 0; // Retorna true si hay un bloqueo vigente en esa fecha
            }
        }
    }
}