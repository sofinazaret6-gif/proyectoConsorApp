using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.SqlClient;
using ConsorApp.Entidades;

namespace ConsorApp.Datos
{
    public class UsuarioDatos
    {
        // Reemplaza la cadena con el servidor/instancia que estés usando
        private string cadenaConexion = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ConsorAppDB;Integrated Security=True";

        // Agrega el signo '?' en la devolución del método
        public Usuario? ValidarCredenciales(string usuario, string password)
        {
            Usuario? usuarioAutenticado = null;

            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                string query = @"
            SELECT u.IdUsuario, u.Nombre, u.Apellido, u.IdPerfil, p.NombrePerfil 
            FROM Usuarios u
            INNER JOIN Perfiles p ON u.IdPerfil = p.IdPerfil
            WHERE u.UsuarioSistema = @usuario AND u.Contrasenia = @password";

                SqlCommand comando = new SqlCommand(query, conexion);
                comando.Parameters.AddWithValue("@usuario", usuario);
                comando.Parameters.AddWithValue("@password", password);

                try
                {
                    conexion.Open();
                    SqlDataReader reader = comando.ExecuteReader();

                    if (reader.Read())
                    {
                        usuarioAutenticado = new Usuario
                        {
                            IdUsuario = Convert.ToInt32(reader["IdUsuario"]),
                            Nombre = reader["Nombre"].ToString() ?? string.Empty,
                            Apellido = reader["Apellido"].ToString() ?? string.Empty,
                            IdPerfil = Convert.ToInt32(reader["IdPerfil"]),
                            Perfil = new Perfil
                            {
                                IdPerfil = Convert.ToInt32(reader["IdPerfil"]),
                                NombrePerfil = reader["NombrePerfil"].ToString() ?? string.Empty
                            }
                        };
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error en la conexión a la base de datos: " + ex.Message);
                }
            }

            return usuarioAutenticado;
        }
    }
}