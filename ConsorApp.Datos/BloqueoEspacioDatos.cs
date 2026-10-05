using System;
using System.Data;
using Microsoft.Data.SqlClient;
using ConsorApp.Entidades;

namespace ConsorApp.Datos
{
    /// <summary>
    /// Gestiona las operaciones de persistencia y consultas SQL para los bloqueos de espacios comunes por mantenimiento.
    /// </summary>
    public class BloqueoEspacioDatos
    {
        private readonly string cadenaConexion =
            "Server=.\\SQLEXPRESS;Database=consorAppDb;Integrated Security=True;TrustServerCertificate=True;";

        /// <summary>
        /// Inserta un nuevo rango de fechas de bloqueo para un espacio común específico.
        /// </summary>
        public void InsertarBloqueo(int idEspacioComun, DateTime fechaDesde, DateTime fechaHasta, string motivo)
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();

                string query = @"
                    INSERT INTO BloqueoEspacio (id_espacioComun, fechaDesde, fechaHasta, motivo)
                    VALUES (@IdEspacioComun, @FechaDesde, @FechaHasta, @Motivo)";

                SqlCommand comando = new SqlCommand(query, conexion);
                comando.Parameters.AddWithValue("@IdEspacioComun", idEspacioComun);
                comando.Parameters.AddWithValue("@FechaDesde", fechaDesde);
                comando.Parameters.AddWithValue("@FechaHasta", fechaHasta);
                comando.Parameters.AddWithValue("@Motivo", motivo);

                comando.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Obtiene todos los bloqueos activos o registrados para mostrarlos en la administración.
        /// </summary>
        public DataTable ObtenerBloqueos()
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                string query = @"
                    SELECT 
                        b.id_Bloqueo AS IdBloqueo,
                        b.id_espacioComun AS IdEspacioComun,
                        e.nombre AS NombreEspacio,
                        b.fechaDesde AS FechaDesde,
                        b.fechaHasta AS FechaHasta,
                        b.motivo AS Motivo
                    FROM BloqueoEspacio b
                    INNER JOIN espacioComun e ON b.id_espacioComun = e.id_espacioComun
                    ORDER BY b.fechaDesde DESC";

                SqlDataAdapter adaptador = new SqlDataAdapter(query, conexion);
                DataTable dt = new DataTable();
                adaptador.Fill(dt);
                return dt;
            }
        }

        /// <summary>
        /// Elimina un bloqueo específico si el mantenimiento finalizó antes de lo previsto (Desbloquear).
        /// </summary>
        public void EliminarBloqueo(int idBloqueo)
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();

                string query = "DELETE FROM BloqueoEspacio WHERE id_Bloqueo = @IdBloqueo";
                SqlCommand comando = new SqlCommand(query, conexion);
                comando.Parameters.AddWithValue("@IdBloqueo", idBloqueo);

                comando.ExecuteNonQuery();
            }
        }
    }
}