using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace ConsorApp.Negocio
{
    public class BloqueoEspacioNegocio
    {
        private readonly string cadenaConexion =
            "Server=.\\SQLEXPRESS;Database=consorAppDb;Integrated Security=True;TrustServerCertificate=True;";

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
                // Usamos los nombres reales de las columnas: fechaDesde y fechaHasta
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
    }
}