using System;
using System.Data;
using Microsoft.Data.SqlClient;
using ConsorApp.Entidades;

namespace ConsorApp.Datos
{
    public class UsuarioDatos
    {
        private readonly string cadenaConexion = "Server=.\\SQLEXPRESS;Database=consorAppDb;Integrated Security=True;TrustServerCertificate=True;";

        // 1. Obtiene la lista completa de usuarios con el nombre de su perfil
        public DataTable ObtenerUsuarios()
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                string query = @"
                    SELECT u.IdUsuario, u.Nombre, u.Apellido, u.Dni, u.Telefono, u.Email, 
                           u.UsuarioSistema, u.Contrasenia, u.IdPerfil, u.FechaAlta, u.Estado, 
                           p.NombrePerfil AS NombrePerfil
                    FROM Usuarios u
                    INNER JOIN Perfiles p ON u.IdPerfil = p.IdPerfil";

                SqlDataAdapter adaptador = new SqlDataAdapter(query, conexion);
                DataTable dt = new DataTable();
                adaptador.Fill(dt);
                return dt;
            }
        }

        // 2. Obtiene los perfiles para llenar el ComboBox
        public DataTable ObtenerPerfiles()
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                string query = "SELECT IdPerfil, NombrePerfil FROM Perfiles";
                SqlDataAdapter adaptador = new SqlDataAdapter(query, conexion);
                DataTable dt = new DataTable();
                adaptador.Fill(dt);
                return dt;
            }
        }

        // 3. Inserta un nuevo usuario en la BD
        public void InsertarUsuario(Usuario usuario)
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();
                string query = @"INSERT INTO Usuarios (Nombre, Apellido, Dni, Email, UsuarioSistema, Contrasenia, IdPerfil, Estado, FechaAlta) 
                                VALUES (@Nombre, @Apellido, @Dni, @Email, @UsuarioSistema, @Contrasenia, @IdPerfil, 1, GETDATE())";

                SqlCommand comando = new SqlCommand(query, conexion);
                comando.Parameters.AddWithValue("@Nombre", usuario.Nombre);
                comando.Parameters.AddWithValue("@Apellido", usuario.Apellido);
                comando.Parameters.AddWithValue("@Dni", usuario.Dni);
                comando.Parameters.AddWithValue("@Email", usuario.Email);
                comando.Parameters.AddWithValue("@UsuarioSistema", usuario.UsuarioSistema);
                comando.Parameters.AddWithValue("@Contrasenia", usuario.Contrasenia);
                comando.Parameters.AddWithValue("@IdPerfil", usuario.IdPerfil);

                comando.ExecuteNonQuery();
            }
        }

        // 4. Actualiza un usuario existente
        public void ActualizarUsuario(Usuario usuario)
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();
                string query = @"UPDATE Usuarios 
                                SET Nombre = @Nombre, Apellido = @Apellido, Dni = @Dni, 
                                    Email = @Email, UsuarioSistema = @UsuarioSistema, 
                                    Contrasenia = @Contrasenia, IdPerfil = @IdPerfil 
                                WHERE IdUsuario = @IdUsuario";

                SqlCommand comando = new SqlCommand(query, conexion);
                comando.Parameters.AddWithValue("@IdUsuario", usuario.IdUsuario);
                comando.Parameters.AddWithValue("@Nombre", usuario.Nombre);
                comando.Parameters.AddWithValue("@Apellido", usuario.Apellido);
                comando.Parameters.AddWithValue("@Dni", usuario.Dni);
                comando.Parameters.AddWithValue("@Email", usuario.Email);
                comando.Parameters.AddWithValue("@UsuarioSistema", usuario.UsuarioSistema);
                comando.Parameters.AddWithValue("@Contrasenia", usuario.Contrasenia);
                comando.Parameters.AddWithValue("@IdPerfil", usuario.IdPerfil);

                comando.ExecuteNonQuery();
            }
        }

        // 5. Cambia el estado (Alta/Baja lógica)
        public void CambiarEstadoUsuario(int idUsuario, int estado)
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();
                string query = "UPDATE Usuarios SET Estado = @Estado WHERE IdUsuario = @IdUsuario";
                SqlCommand comando = new SqlCommand(query, conexion);
                comando.Parameters.AddWithValue("@Estado", estado);
                comando.Parameters.AddWithValue("@IdUsuario", idUsuario);

                comando.ExecuteNonQuery();
            }
        }

        // 6. Validación de credenciales para login usando LEFT JOIN para evitar bloqueos
        public Usuario? ValidarCredenciales(string usuario, string password)
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();
                string query = @"SELECT u.IdUsuario, u.Nombre, u.Apellido, u.Dni, u.Email, 
                                u.UsuarioSistema, u.Contrasenia, u.IdPerfil, ISNULL(p.NombrePerfil, 'Usuario') AS NombrePerfil 
                        FROM Usuarios u
                        LEFT JOIN Perfiles p ON u.IdPerfil = p.IdPerfil
                        WHERE u.UsuarioSistema = @Usuario AND u.Contrasenia = @Password AND u.Estado = 1";

                SqlCommand comando = new SqlCommand(query, conexion);
                // Usamos .Trim() por las dudas si se filtran espacios accidentales al escribir
                comando.Parameters.AddWithValue("@Usuario", usuario.Trim());
                comando.Parameters.AddWithValue("@Password", password.Trim());

                using (SqlDataReader reader = comando.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Usuario
                        {
                            IdUsuario = Convert.ToInt32(reader["IdUsuario"]),
                            Nombre = reader["Nombre"].ToString() ?? string.Empty,
                            Apellido = reader["Apellido"].ToString() ?? string.Empty,
                            Dni = reader["Dni"].ToString() ?? string.Empty,
                            Email = reader["Email"].ToString() ?? string.Empty,
                            UsuarioSistema = reader["UsuarioSistema"].ToString() ?? string.Empty,
                            Contrasenia = reader["Contrasenia"].ToString() ?? string.Empty,
                            IdPerfil = Convert.ToInt32(reader["IdPerfil"]),
                            NombrePerfil = reader["NombrePerfil"].ToString() ?? string.Empty
                        };
                    }
                }
            }
            return null;
        }

        // 7. Obtiene únicamente los usuarios que tienen el rol de Propietario
        public DataTable ObtenerPropietarios()
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                string query = @"
                    SELECT u.IdUsuario, (u.Nombre + ' ' + u.Apellido) AS Nombre, u.Dni, u.Email
                    FROM Usuarios u
                    INNER JOIN Perfiles p ON u.IdPerfil = p.IdPerfil
                    WHERE p.NombrePerfil = 'Propietario' AND u.Estado = 1";

                SqlDataAdapter adaptador = new SqlDataAdapter(query, conexion);
                DataTable dt = new DataTable();
                adaptador.Fill(dt);
                return dt;
            }
        }
    }
}