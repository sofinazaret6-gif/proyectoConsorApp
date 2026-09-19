using System;
using System.Data;
using System.Text.RegularExpressions;
using ConsorApp.Datos;
using ConsorApp.Entidades;

namespace ConsorApp.Negocio
{
    public class UsuarioNegocio
    {
        private UsuarioDatos datos = new UsuarioDatos();

        // 1. Obtener todos los usuarios desde la capa de Datos
        public DataTable ObtenerUsuarios()
        {
            return datos.ObtenerUsuarios();
        }

        // 2. Obtener la lista de perfiles desde la capa de Datos
        public DataTable ObtenerPerfiles()
        {
            return datos.ObtenerPerfiles();
        }

        // 3. Insertar un nuevo usuario
        public void GuardarUsuario(Usuario usuario)
        {
            datos.InsertarUsuario(usuario);
        }

        // 4. Modificar datos de un usuario existente
        public void ActualizarUsuario(Usuario usuario)
        {
            datos.ActualizarUsuario(usuario);
        }

        // 5. Alta o baja lógica del usuario (Estado: 1 o 0)
        public void CambiarEstadoUsuario(int idUsuario, int estado)
        {
            datos.CambiarEstadoUsuario(idUsuario, estado);
        }

        // Métodos de autenticación y validación
        public Usuario? IniciarSesion(string usuario, string password)
        {
            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(password))
            {
                throw new Exception("Debe ingresar usuario y contraseña.");
            }

            return datos.ValidarCredenciales(usuario, password);
        }

        public void ValidarUsuario(Usuario usuario)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario), "El usuario no puede ser nulo.");

            if (string.IsNullOrWhiteSpace(usuario.Nombre))
                throw new Exception("El nombre no puede estar vacío.");

            if (string.IsNullOrWhiteSpace(usuario.Apellido))
                throw new Exception("El apellido no puede estar vacío.");

            // Validación DNI de 8 dígitos
            if (string.IsNullOrWhiteSpace(usuario.Dni) || !Regex.IsMatch(usuario.Dni, @"^\d{8}$"))
            {
                throw new Exception("El DNI es inválido. Debe contener exactamente 8 dígitos numéricos.");
            }

            // Validación de correo con @
            string patronEmail = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (string.IsNullOrWhiteSpace(usuario.Email) || !Regex.IsMatch(usuario.Email, patronEmail))
            {
                throw new Exception("El correo electrónico ingresado no tiene un formato válido.");
            }

            if (string.IsNullOrWhiteSpace(usuario.Contrasenia) || usuario.Contrasenia.Length < 4)
            {
                throw new Exception("La contraseña debe tener al menos 4 caracteres.");
            }

            if (string.IsNullOrWhiteSpace(usuario.UsuarioSistema))
            {
                throw new Exception("El nombre de usuario es obligatorio.");
            }
        }
    }
}
