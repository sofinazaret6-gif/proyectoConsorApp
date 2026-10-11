using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace ConsorApp.Negocio
{
    public class BloqueoEspacioNegocio
    {
        private readonly string cadenaConexion =
            "Server=.\\SQLEXPRESS;Database=consorAppDb;Integrated Security=True;TrustServerCertificate=True;";

        // Registra un nuevo bloqueo de espacio por mantenimiento
        public void RegistrarBloqueo(int idEspacioComun, DateTime? fechaInicio, DateTime? fechaFin, string motivo)
        {
            if (!fechaInicio.HasValue || !fechaFin.HasValue)
            {
                throw new Exception("Debe especificar las fechas de inicio y fin del bloqueo.");
            }

            if (fechaInicio.Value > fechaFin.Value)
            {
                throw new Exception("La fecha de inicio no puede ser posterior a la fecha de fin.");
            }

            if (string.IsNullOrWhiteSpace(motivo))
            {
                throw new Exception("Debe ingresar un motivo para el bloqueo.");
            }

            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();
                string query = @"
                    INSERT INTO BloqueoEspacio (id_espacioComun, fechaDesde, fechaHasta, motivo)
                    VALUES (@IdEspacio, @FechaDesde, @FechaHasta, @Motivo)";

                SqlCommand comando = new SqlCommand(query, conexion);
                comando.Parameters.AddWithValue("@IdEspacio", idEspacioComun);
                comando.Parameters.AddWithValue("@FechaDesde", fechaInicio.Value);
                comando.Parameters.AddWithValue("@FechaHasta", fechaFin.Value);
                comando.Parameters.AddWithValue("@Motivo", motivo);

                comando.ExecuteNonQuery();
            }
        }

        // Obtiene todos los bloqueos registrados para mostrarlos en la grilla del admin
        public DataTable ObtenerBloqueosAdmin()
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                string query = @"
                    SELECT 
                        b.id_Bloqueo AS IdBloqueo,
                        b.id_espacioComun AS IdEspacioComun,
                        e.nombre AS Espacio,
                        CONVERT(VARCHAR(10), b.fechaDesde, 103) AS FechaDesde,
                        CONVERT(VARCHAR(10), b.fechaHasta, 103) AS FechaHasta,
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

        // Modifica un bloqueo existente seleccionado
        public void ModificarBloqueo(int idBloqueo, int idEspacioComun, DateTime? fechaInicio, DateTime? fechaFin, string motivo)
        {
            if (!fechaInicio.HasValue || !fechaFin.HasValue)
            {
                throw new Exception("Debe especificar las fechas de inicio y fin del bloqueo.");
            }

            if (fechaInicio.Value > fechaFin.Value)
            {
                throw new Exception("La fecha de inicio no puede ser posterior a la fecha de fin.");
            }

            if (string.IsNullOrWhiteSpace(motivo))
            {
                throw new Exception("Debe ingresar un motivo para el bloqueo.");
            }

            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();
                string query = @"
                    UPDATE BloqueoEspacio 
                    SET id_espacioComun = @IdEspacio, 
                        fechaDesde = @FechaDesde, 
                        fechaHasta = @FechaHasta, 
                        motivo = @Motivo
                    WHERE id_Bloqueo = @IdBloqueo";

                SqlCommand comando = new SqlCommand(query, conexion);
                comando.Parameters.AddWithValue("@IdBloqueo", idBloqueo);
                comando.Parameters.AddWithValue("@IdEspacio", idEspacioComun);
                comando.Parameters.AddWithValue("@FechaDesde", fechaInicio.Value);
                comando.Parameters.AddWithValue("@FechaHasta", fechaFin.Value);
                comando.Parameters.AddWithValue("@Motivo", motivo);

                comando.ExecuteNonQuery();
            }
        }

        // Elimina o cancela un bloqueo registrado
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