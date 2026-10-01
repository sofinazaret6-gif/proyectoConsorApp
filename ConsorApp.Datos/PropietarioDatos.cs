using System;
using Microsoft.Data.SqlClient;
using ConsorApp.Entidades;

namespace ConsorApp.Datos
{
    public class PropietarioDatos
    {
        public Propietario? ObtenerPorUsuario(int idUsuario)
        {
            Propietario? propietario = null;

            using SqlConnection conexion = Conexion.ObtenerConexion();

            string sql = @"
             SELECT
                Id_Propietario AS IdPropietario,
                   Id_Usuario AS IdUsuario,
                Id_Departamento AS IdDepartamento,
              FechaDesde,
              FechaHasta,
              Estado
             FROM Propietario
             WHERE Id_Usuario = @IdUsuario
            AND Estado = 1";

            using SqlCommand comando = new SqlCommand(sql, conexion);

            comando.Parameters.AddWithValue("@IdUsuario", idUsuario);

            try
            {
                conexion.Open();

                using SqlDataReader reader = comando.ExecuteReader();

                if (reader.Read())
                {
                    propietario = new Propietario
                    {
                        IdPropietario = Convert.ToInt32(reader["IdPropietario"]),
                        IdUsuario = Convert.ToInt32(reader["IdUsuario"]),
                        IdDepartamento = Convert.ToInt32(reader["IdDepartamento"]),
                        FechaDesde = Convert.ToDateTime(reader["FechaDesde"]),
                        FechaHasta = reader["FechaHasta"] == DBNull.Value
                            ? null
                            : Convert.ToDateTime(reader["FechaHasta"]),
                        Estado = reader["Estado"].ToString() ?? ""
                    };
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    "Error al obtener los datos del propietario:\n\n" + ex.Message,
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }

            return propietario;
        }
    }
}