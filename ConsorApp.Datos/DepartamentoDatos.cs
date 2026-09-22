using System;
using System.Data;
using Microsoft.Data.SqlClient;
using ConsorApp.Entidades;

namespace ConsorApp.Datos
{
    /// <summary>
    /// Gestiona las operaciones de persistencia y consultas SQL directas
    /// sobre la tabla Departamento y sus relaciones.
    /// </summary>
    public class DepartamentoDatos
    {
        // Cadena de conexión hacia la instancia local de SQL Server
        private readonly string cadenaConexion =
            "Server=.\\SQLEXPRESS;Database=consorAppDb;Integrated Security=True;TrustServerCertificate=True;";

        /// <summary>
        /// Obtiene los departamentos con el nombre de su propietario actual.
        /// Se omitió el JOIN con la tabla Edificio al manejar un único edificio en el sistema.
        /// </summary>
        public DataTable ObtenerDepartamentos()
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                string query = @"
                    SELECT
                        d.id_Departamento AS IdDepartamento,
                        d.id_Edificio AS IdEdificio,
                        d.piso AS Piso,
                        d.unidad AS Unidad,
                        ISNULL(u.Nombre + ' ' + u.Apellido, 'Sin Asignar') AS NombrePropietario
                    FROM Departamento d
                    LEFT JOIN Propietario p
                        ON d.id_Departamento = p.Id_Departamento
                        AND p.fechaHasta IS NULL
                    LEFT JOIN Usuarios u
                        ON p.Id_Usuario = u.idUsuario";

                SqlDataAdapter adaptador = new SqlDataAdapter(query, conexion);
                DataTable dt = new DataTable();
                adaptador.Fill(dt);
                return dt;
            }
        }

        /// <summary>
        /// Obtiene la cantidad actual de departamentos registrados.
        /// Si se pasa idEdificio <= 0, cuenta el total sin filtrar.
        /// </summary>
        public int ObtenerCantidadDepartamentos(int idEdificio = 0)
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();

                string query = idEdificio > 0
                    ? "SELECT COUNT(*) FROM Departamento WHERE id_Edificio = @IdEdificio"
                    : "SELECT COUNT(*) FROM Departamento";

                SqlCommand comando = new SqlCommand(query, conexion);

                if (idEdificio > 0)
                {
                    comando.Parameters.AddWithValue("@IdEdificio", idEdificio);
                }

                return Convert.ToInt32(comando.ExecuteScalar());
            }
        }

        /// <summary>
        /// Obtiene la cantidad máxima de departamentos permitidos para un edificio.
        /// Si no se especifica ID, toma el límite del primer edificio cargado en la base de datos.
        /// </summary>
        public int ObtenerCantidadMaximaDepartamentos(int idEdificio = 0)
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();

                string query = idEdificio > 0
                    ? "SELECT CantDepto FROM Edificio WHERE id_edificio = @IdEdificio"
                    : "SELECT TOP 1 CantDepto FROM Edificio";

                SqlCommand comando = new SqlCommand(query, conexion);

                if (idEdificio > 0)
                {
                    comando.Parameters.AddWithValue("@IdEdificio", idEdificio);
                }

                object resultado = comando.ExecuteScalar();

                if (resultado == null)
                {
                    throw new Exception("No se encontró la configuración del edificio.");
                }

                return Convert.ToInt32(resultado);
            }
        }

        /// <summary>
        /// Inserta un nuevo departamento en la base de datos y devuelve el ID generado.
        /// </summary>
        public int InsertarDepartamento(Departamento depto)
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();

                string query = @"
                    INSERT INTO Departamento (id_Edificio, piso, unidad)
                    VALUES (@Id_edificio, @Piso, @Unidad);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                SqlCommand comando = new SqlCommand(query, conexion);

                comando.Parameters.AddWithValue("@Id_edificio", depto.IdEdificio);
                comando.Parameters.AddWithValue("@Piso", depto.Piso);
                comando.Parameters.AddWithValue("@Unidad", depto.Unidad);

                return Convert.ToInt32(comando.ExecuteScalar());
            }
        }

        /// <summary>
        /// Registra la relación entre un departamento y un usuario en la tabla intermedia Propietario.
        /// </summary>
        public void AsignarPropietario(int idDepartamento, int idUsuarioPropietario)
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();

                string query = @"
                    INSERT INTO Propietario (Id_Usuario, Id_Departamento, fechaDesde, estado)
                    VALUES (@IdUsuario, @IdDepartamento, GETDATE(), 1)";

                SqlCommand comando = new SqlCommand(query, conexion);

                comando.Parameters.AddWithValue("@IdUsuario", idUsuarioPropietario);
                comando.Parameters.AddWithValue("@IdDepartamento", idDepartamento);

                comando.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Actualiza un departamento existente identificado por su IdDepartamento.
        /// </summary>
        public void ActualizarDepartamento(Departamento depto)
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();

                string query = @"
                    UPDATE Departamento
                    SET id_Edificio = @Id_edificio,
                        piso = @Piso,
                        unidad = @Unidad
                    WHERE id_Departamento = @IdDepartamento";

                SqlCommand comando = new SqlCommand(query, conexion);

                comando.Parameters.AddWithValue("@IdDepartamento", depto.IdDepartamento);
                comando.Parameters.AddWithValue("@Id_edificio", depto.IdEdificio);
                comando.Parameters.AddWithValue("@Piso", depto.Piso);
                comando.Parameters.AddWithValue("@Unidad", depto.Unidad);

                comando.ExecuteNonQuery();
            }
        }
    }
}