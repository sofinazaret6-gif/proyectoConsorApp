using System;
using ConsorApp.Entidades;
using Microsoft.Data.SqlClient;

namespace ConsorApp.Datos
{
    public class MiDepartamentoDatos
    {
        public Departamento? ObtenerPorId(int idDepartamento)
        {
            using SqlConnection conexion = Conexion.ObtenerConexion();

            string sql = @"
                SELECT
                    Id_Departamento,
                    Id_Edificio,
                    Piso,
                    Unidad
                FROM Departamento
                WHERE Id_Departamento = @IdDepartamento";

            using SqlCommand comando = new SqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@IdDepartamento", idDepartamento);

            try
            {
                conexion.Open();

                using SqlDataReader reader = comando.ExecuteReader();

                if (!reader.Read())
                    return null;

                return new Departamento
                {
                    IdDepartamento = Convert.ToInt32(reader["Id_Departamento"]),
                    IdEdificio = Convert.ToInt32(reader["Id_Edificio"]),
                    Piso = reader["Piso"]?.ToString() ?? string.Empty,
                    Unidad = reader["Unidad"]?.ToString() ?? string.Empty
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener el departamento: " + ex.Message);
            }
        }
    }
}