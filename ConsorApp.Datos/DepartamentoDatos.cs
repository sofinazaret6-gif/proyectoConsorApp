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
        /// Realiza una consulta con JOINs para obtener los departamentos,
        /// agregando el nombre del edificio y el nombre del propietario actual.
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
                        e.Descripcion AS NombreEdificio,

                        ISNULL(u.Nombre + ' ' + u.Apellido, 'Sin Asignar')
                            AS NombrePropietario

                    FROM Departamento d

                    INNER JOIN Edificio e
                        ON d.id_Edificio = e.id_edificio

                    LEFT JOIN Propietario p
                        ON d.id_Departamento = p.Id_Departamento
                        AND p.fechaHasta IS NULL

                    LEFT JOIN Usuarios u
                        ON p.Id_Usuario = u.idUsuario";

                SqlDataAdapter adaptador =
                    new SqlDataAdapter(query, conexion);

                DataTable dt = new DataTable();
                adaptador.Fill(dt);

                return dt;
            }
        }

        /// <summary>
        /// Obtiene la cantidad actual de departamentos que tiene un edificio.
        /// </summary>
        public int ObtenerCantidadDepartamentos(int idEdificio)
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();

                string query = @"
                    SELECT COUNT(*)
                    FROM Departamento
                    WHERE id_Edificio = @IdEdificio";

                SqlCommand comando =
                    new SqlCommand(query, conexion);

                comando.Parameters.AddWithValue("@IdEdificio", idEdificio);

                return Convert.ToInt32(comando.ExecuteScalar());
            }
        }

        /// <summary>
        /// Obtiene la cantidad máxima de departamentos permitidos
        /// para un edificio.
        /// </summary>
        public int ObtenerCantidadMaximaDepartamentos(int idEdificio)
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();

                string query = @"
                    SELECT CantDepto
                    FROM Edificio
                    WHERE id_edificio = @IdEdificio";

                SqlCommand comando =
                    new SqlCommand(query, conexion);

                comando.Parameters.AddWithValue("@IdEdificio", idEdificio);

                object resultado = comando.ExecuteScalar();

                if (resultado == null)
                {
                    throw new Exception("No se encontró el edificio seleccionado.");
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
                    INSERT INTO Departamento
                    (id_Edificio, piso, unidad)
                    VALUES
                    (@Id_edificio, @Piso, @Unidad);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                SqlCommand comando =
                    new SqlCommand(query, conexion);

                comando.Parameters.AddWithValue(
                    "@Id_edificio", depto.IdEdificio);

                comando.Parameters.AddWithValue(
                    "@Piso", depto.Piso);

                comando.Parameters.AddWithValue(
                    "@Unidad", depto.Unidad);

                // Ejecutamos y capturamos el ID que se acaba de crear en la tabla
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
                    INSERT INTO Propietario
                    (Id_Usuario, Id_Departamento, fechaDesde, estado)
                    VALUES
                    (@IdUsuario, @IdDepartamento, GETDATE(), 1)";

                SqlCommand comando =
                    new SqlCommand(query, conexion);

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

                SqlCommand comando =
                    new SqlCommand(query, conexion);

                comando.Parameters.AddWithValue(
                    "@IdDepartamento", depto.IdDepartamento);

                comando.Parameters.AddWithValue(
                    "@Id_edificio", depto.IdEdificio);

                comando.Parameters.AddWithValue(
                    "@Piso", depto.Piso);

                comando.Parameters.AddWithValue(
                    "@Unidad", depto.Unidad);

                comando.ExecuteNonQuery();
            }
        }
    }
}