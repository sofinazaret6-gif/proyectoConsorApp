using System.Data;
using Microsoft.Data.SqlClient;
using ConsorApp.Entidades;

namespace ConsorApp.Datos
{
    public class EspacioComunDatos
    {
        private readonly string cadenaConexion =
            "Server=.\\SQLEXPRESS;Database=consorAppDb;Integrated Security=True;TrustServerCertificate=True;";

        public void InsertarEspacioComun(int idEdificio, EspacioComun espacio)
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();
                string query = @"
                    INSERT INTO espacioComun (id_edificio, nombre, descripcion, capacidad, estado)
                    VALUES (@IdEdificio, @Nombre, @Descripcion, @Capacidad, @Estado)";

                SqlCommand comando = new SqlCommand(query, conexion);
                comando.Parameters.AddWithValue("@IdEdificio", idEdificio);
                comando.Parameters.AddWithValue("@Nombre", espacio.Nombre);
                comando.Parameters.AddWithValue("@Descripcion", (object)espacio.Descripcion ?? DBNull.Value);
                comando.Parameters.AddWithValue("@Capacidad", espacio.Capacidad);
                comando.Parameters.AddWithValue("@Estado", string.IsNullOrEmpty(espacio.Estado) ? "Activo" : espacio.Estado);

                comando.ExecuteNonQuery();
            }
        }

        public DataTable ObtenerEspaciosComunes()
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                string query = @"
                    SELECT 
                        id_espacioComun AS IdEspacioComun,
                        id_edificio AS IdEdificio,
                        nombre AS Nombre,
                        descripcion AS Descripcion,
                        capacidad AS Capacidad,
                        estado AS Estado
                    FROM espacioComun
                    WHERE estado = 'Activo'";

                SqlDataAdapter adaptador = new SqlDataAdapter(query, conexion);
                DataTable dt = new DataTable();
                adaptador.Fill(dt);
                return dt;
            }
        }
    }
}