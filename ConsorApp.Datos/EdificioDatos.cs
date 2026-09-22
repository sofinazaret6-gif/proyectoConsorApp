using System;
using System.Data;
using Microsoft.Data.SqlClient;
using ConsorApp.Entidades;

namespace ConsorApp.Datos
{
    public class EdificioDatos
    {
        private readonly string cadenaConexion =
            "Server=.\\SQLEXPRESS;Database=consorAppDb;Integrated Security=True;TrustServerCertificate=True;";

        // Obtener el único edificio registrado (TOP 1)
        public EDIFICIO ObtenerUnicoEdificio()
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();

                string query = @"
                    SELECT TOP 1 
                        Id_edificio AS id_edificio,
                        Descripcion,
                        CantDepto,
                        CantPisos,
                        Ubicacion
                    FROM Edificio";

                SqlCommand comando = new SqlCommand(query, conexion);

                using (SqlDataReader reader = comando.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new EDIFICIO
                        {
                            id_edificio = Convert.ToInt32(reader["id_edificio"]),
                            Descripcion = reader["Descripcion"]?.ToString() ?? string.Empty,
                            CantPisos = Convert.ToInt32(reader["CantPisos"]),
                            CantDepto = Convert.ToInt32(reader["CantDepto"]),
                            Ubicacion = reader["Ubicacion"]?.ToString() ?? string.Empty
                        };
                    }
                }

                // Si aún no hay ningún edificio cargado, retorna null
                return default!;
            }
        }

        // Insertar un nuevo edificio
        public void InsertarEdificio(EDIFICIO edificio)
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();

                string query = @"
                    INSERT INTO Edificio (Descripcion, CantDepto, CantPisos, Ubicacion)
                    VALUES (@Descripcion, @CantDepto, @CantPisos, @Ubicacion)";

                SqlCommand comando = new SqlCommand(query, conexion);
                comando.Parameters.AddWithValue("@Descripcion", edificio.Descripcion);
                comando.Parameters.AddWithValue("@CantDepto", edificio.CantDepto);
                comando.Parameters.AddWithValue("@CantPisos", edificio.CantPisos);
                comando.Parameters.AddWithValue("@Ubicacion", edificio.Ubicacion);

                comando.ExecuteNonQuery();
            }
        }

        // Actualizar el edificio existente
        public void ActualizarEdificio(EDIFICIO edificio)
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();

                string query = @"
                    UPDATE Edificio
                    SET
                        Descripcion = @Descripcion,
                        CantDepto = @CantDepto,
                        CantPisos = @CantPisos,
                        Ubicacion = @Ubicacion
                    WHERE Id_edificio = @IdEdificio";

                SqlCommand comando = new SqlCommand(query, conexion);
                comando.Parameters.AddWithValue("@IdEdificio", edificio.id_edificio);
                comando.Parameters.AddWithValue("@Descripcion", edificio.Descripcion);
                comando.Parameters.AddWithValue("@CantDepto", edificio.CantDepto);
                comando.Parameters.AddWithValue("@CantPisos", edificio.CantPisos);
                comando.Parameters.AddWithValue("@Ubicacion", edificio.Ubicacion);

                comando.ExecuteNonQuery();
            }
        }

        public int ObtenerCantidadDepartamentos(int idEdificio)
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();
                string query = "SELECT COUNT(*) FROM Departamento WHERE id_Edificio = @IdEdificio";
                SqlCommand comando = new SqlCommand(query, conexion);
                comando.Parameters.AddWithValue("@IdEdificio", idEdificio);
                return Convert.ToInt32(comando.ExecuteScalar());
            }
        }

        public int ObtenerPisoMaximo(int idEdificio)
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();
                string query = @"
                    SELECT MAX(
                        CASE
                            WHEN ISNUMERIC(piso) = 1 THEN CAST(piso AS INT)
                            ELSE NULL
                        END
                    )
                    FROM Departamento WHERE id_Edificio = @IdEdificio";

                SqlCommand comando = new SqlCommand(query, conexion);
                comando.Parameters.AddWithValue("@IdEdificio", idEdificio);

                object resultado = comando.ExecuteScalar();
                if (resultado == DBNull.Value || resultado == null)
                    return 0;

                return Convert.ToInt32(resultado);
            }
        }
    }
}