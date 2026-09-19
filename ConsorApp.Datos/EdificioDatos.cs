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

        // Obtener todos los edificios
        public DataTable ObtenerEdificios()
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                string query = @"
                    SELECT 
                        Id_edificio AS IdEdificio,
                        Descripcion,
                        CantDepto,
                        CantPisos,
                        Ubicacion
                    FROM Edificio";

                SqlDataAdapter adaptador =
                    new SqlDataAdapter(query, conexion);

                DataTable dt = new DataTable();
                adaptador.Fill(dt);

                return dt;
            }
        }

        // Insertar un nuevo edificio
        public void InsertarEdificio(EDIFICIO edificio)
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();

                string query = @"
                    INSERT INTO Edificio
                    (
                        Descripcion,
                        CantDepto,
                        CantPisos,
                        Ubicacion
                    )
                    VALUES
                    (
                        @Descripcion,
                        @CantDepto,
                        @CantPisos,
                        @Ubicacion
                    )";

                SqlCommand comando =
                    new SqlCommand(query, conexion);

                comando.Parameters.AddWithValue(
                    "@Descripcion",
                    edificio.Descripcion);

                comando.Parameters.AddWithValue(
                    "@CantDepto",
                    edificio.CantDepto);

                comando.Parameters.AddWithValue(
                    "@CantPisos",
                    edificio.CantPisos);

                comando.Parameters.AddWithValue(
                    "@Ubicacion",
                    edificio.Ubicacion);

                comando.ExecuteNonQuery();
            }
        }

        // Actualizar un edificio existente
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

                SqlCommand comando =
                    new SqlCommand(query, conexion);

                comando.Parameters.AddWithValue(
                    "@IdEdificio",
                    edificio.id_edificio);

                comando.Parameters.AddWithValue(
                    "@Descripcion",
                    edificio.Descripcion);

                comando.Parameters.AddWithValue(
                    "@CantDepto",
                    edificio.CantDepto);

                comando.Parameters.AddWithValue(
                    "@CantPisos",
                    edificio.CantPisos);

                comando.Parameters.AddWithValue(
                    "@Ubicacion",
                    edificio.Ubicacion);

                comando.ExecuteNonQuery();
            }
        }

        // Obtener cuántos departamentos tiene actualmente un edificio
        public int ObtenerCantidadDepartamentos(int idEdificio)
        {
            using (SqlConnection conexion =
                new SqlConnection(cadenaConexion))
            {
                conexion.Open();

                string query = @"
                    SELECT COUNT(*)
                    FROM Departamento
                    WHERE id_Edificio = @IdEdificio";

                SqlCommand comando =
                    new SqlCommand(query, conexion);

                comando.Parameters.AddWithValue(
                    "@IdEdificio",
                    idEdificio);

                return Convert.ToInt32(
                    comando.ExecuteScalar());
            }
        }

        // Obtener el piso más alto que tiene departamentos registrados
        public int ObtenerPisoMaximo(int idEdificio)
        {
            using (SqlConnection conexion =
                new SqlConnection(cadenaConexion))
            {
                conexion.Open();

                string query = @"
                    SELECT MAX(
                        CASE
                            WHEN ISNUMERIC(piso) = 1
                            THEN CAST(piso AS INT)
                            ELSE NULL
                        END
                    )
                    FROM Departamento
                    WHERE id_Edificio = @IdEdificio";

                SqlCommand comando =
                    new SqlCommand(query, conexion);

                comando.Parameters.AddWithValue(
                    "@IdEdificio",
                    idEdificio);

                object resultado =
                    comando.ExecuteScalar();

                if (resultado == DBNull.Value || resultado == null)
                    return 0;

                return Convert.ToInt32(resultado);
            }
        }
    }
}