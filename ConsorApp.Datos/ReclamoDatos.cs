using ConsorApp.Entidades;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows;

namespace ConsorApp.Datos
{
    public class ReclamoDatos
    {
        public bool RegistrarReclamo(Reclamo reclamo)
        {
            using SqlConnection conexion = Conexion.ObtenerConexion();

            string sql = @"
    INSERT INTO Reclamo
    (Id_UsuarioDepartamento, Motivo, Categoria, Ubicacion, Descripcion, Observacion, estadoReclamo, fechaReclamo)
    VALUES
    (@Id_UsuarioDepartamento, @Motivo, @Categoria, @Ubicacion, @Descripcion, @Observacion, @estadoReclamo, @fechaReclamo)";

            using SqlCommand comando = new SqlCommand(sql, conexion);

            comando.Parameters.AddWithValue("@Id_UsuarioDepartamento", reclamo.Id_UsuarioDepartamento);
            comando.Parameters.AddWithValue("@Motivo", reclamo.Motivo);
            comando.Parameters.AddWithValue("@Categoria", reclamo.Categoria);
            comando.Parameters.AddWithValue("@Ubicacion", reclamo.Ubicacion);
            comando.Parameters.AddWithValue("@Descripcion", reclamo.Descripcion);
            comando.Parameters.AddWithValue("@Observacion", reclamo.Observacion);
            comando.Parameters.AddWithValue("@estadoReclamo", reclamo.estadoReclamo);
            comando.Parameters.AddWithValue("@fechaReclamo", reclamo.fechaReclamo);

            try
            {
                conexion.Open();
                comando.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al registrar el reclamo:\n\n" + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return false;
            }
        }

        public DataTable ObtenerReclamosParaAdmin()
        {
            DataTable dt = new DataTable();

            using SqlConnection conexion = Conexion.ObtenerConexion();

            string sql = @"
        SELECT 
            r.Id_Reclamo AS IdReclamo,
            r.Motivo,
            ISNULL(r.Categoria, 'Otro') AS Categoria,
            r.Ubicacion,

            ISNULL(
                u.Nombre + ' ' + u.Apellido,
                'Sin responsable'
            ) AS Responsable,

            ISNULL(
                'Piso ' + CAST(d.piso AS VARCHAR(10)) 
                + ' - Unidad ' + d.unidad,
                'Sin departamento'
            ) AS Departamento,

            CONVERT(VARCHAR(10), r.fechaReclamo, 103) AS Fecha,
            YEAR(r.fechaReclamo)  AS Anio,
            MONTH(r.fechaReclamo) AS Mes,
            r.estadoReclamo AS Estado,
            ISNULL(r.Observacion, '') AS Observacion

        FROM Reclamo r

        LEFT JOIN Propietario p
            ON r.Id_UsuarioDepartamento = p.Id_Propietario

        LEFT JOIN Usuarios u
            ON p.Id_Usuario = u.idUsuario

        LEFT JOIN Departamento d
            ON p.Id_Departamento = d.id_Departamento

        ORDER BY r.fechaReclamo DESC";

            using SqlCommand comando = new SqlCommand(sql, conexion);
            using SqlDataAdapter adaptador = new SqlDataAdapter(comando);

            try
            {
                conexion.Open();
                adaptador.Fill(dt);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error al obtener los reclamos de la base de datos: "
                    + ex.Message);
            }

            return dt;
        }
        public bool ActualizarEstadoYObservacion(int idReclamo, string nuevoEstado, string observacion)
        {
            using SqlConnection conexion = Conexion.ObtenerConexion();

            string sql = @"
                UPDATE Reclamo 
                SET estadoReclamo = @estadoReclamo, 
                    Observacion = @Observacion 
                WHERE Id_Reclamo = @Id_Reclamo";

            using SqlCommand comando = new SqlCommand(sql, conexion);

            comando.Parameters.AddWithValue("@estadoReclamo", nuevoEstado);
            comando.Parameters.AddWithValue("@Observacion", string.IsNullOrEmpty(observacion) ? (object)DBNull.Value : observacion);
            comando.Parameters.AddWithValue("@Id_Reclamo", idReclamo);

            try
            {
                conexion.Open();
                comando.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al actualizar el reclamo:\n\n" + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return false;
            }
        }

        public bool ActualizarObservacion(int idReclamo, string observacion)
        {
            using SqlConnection conexion = Conexion.ObtenerConexion();

            string sql = @"
        UPDATE Reclamo
        SET Observacion = @Observacion
        WHERE Id_Reclamo = @Id_Reclamo";

            using SqlCommand comando = new SqlCommand(sql, conexion);

            comando.Parameters.AddWithValue(
                "@Observacion",
                string.IsNullOrWhiteSpace(observacion)
                    ? (object)DBNull.Value
                    : observacion
            );

            comando.Parameters.AddWithValue("@Id_Reclamo", idReclamo);

            try
            {
                conexion.Open();

                int filasAfectadas = comando.ExecuteNonQuery();

                return filasAfectadas > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al guardar la observación:\n\n" + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return false;
            }
        }

        public DataTable ObtenerReclamosPorPropietario(int idPropietario)
        {
            DataTable dt = new DataTable();

            using SqlConnection conexion = Conexion.ObtenerConexion();

            string sql = @"
        SELECT
            r.Id_Reclamo AS IdReclamo,
            r.Motivo,
            r.Ubicacion,
            CONVERT(VARCHAR(10), r.fechaReclamo, 103) AS Fecha,
            r.estadoReclamo AS Estado,
            ISNULL(r.Observacion, '') AS Observacion
        FROM Reclamo r
        WHERE r.Id_UsuarioDepartamento = @IdPropietario
        ORDER BY r.fechaReclamo DESC";

            using SqlCommand comando = new SqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@IdPropietario", idPropietario);

            using SqlDataAdapter adaptador = new SqlDataAdapter(comando);

            try
            {
                conexion.Open();
                adaptador.Fill(dt);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error al obtener los reclamos del propietario: " + ex.Message);
            }

            return dt;
        }

    }
}