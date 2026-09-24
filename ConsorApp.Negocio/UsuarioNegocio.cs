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

        /// <summary>
        /// Obtiene una lista con los usuarios que tienen rol de propietario.
        /// </summary>
        public DataTable ObtenerPropietarios()
        {
            // Corregido: usamos la variable 'datos' que ya está definida arriba
            return datos.ObtenerPropietarios();
        }

        public void ValidarUsuario(Usuario usuario)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario), "El usuario no puede ser nulo.");

            // Validación de Nombre: Solo letras y espacios (permite acentos)
            if (string.IsNullOrWhiteSpace(usuario.Nombre) || !Regex.IsMatch(usuario.Nombre, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
            {
                throw new Exception("El nombre no es válido. Solo debe contener letras.");
            }

            // Validación de Apellido: Solo letras y espacios (permite acentos)
            if (string.IsNullOrWhiteSpace(usuario.Apellido) || !Regex.IsMatch(usuario.Apellido, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
            {
                throw new Exception("El apellido no es válido. Solo debe contener letras.");
            }

            // Validación de DNI: Obligatorio y estrictamente solo números (7 u 8 dígitos)
            if (string.IsNullOrWhiteSpace(usuario.Dni) || !Regex.IsMatch(usuario.Dni, @"^\d{7,8}$"))
            {
                throw new Exception("El DNI es inválido. Debe contener solo números (7 u 8 dígitos).");
            }

            // VALIDACIÓN DE DNI ÚNICO EN EL SISTEMA
            DataTable dtUsuarios = datos.ObtenerUsuarios();
            if (dtUsuarios != null)
            {
                foreach (DataRow row in dtUsuarios.Rows)
                {
                    int idExistente = Convert.ToInt32(row["IdUsuario"]);
                    string dniExistente = row["Dni"]?.ToString() ?? string.Empty;

                    // Si el DNI coincide y el ID es diferente, significa que pertenece a otro usuario
                    if (dniExistente == usuario.Dni && idExistente != usuario.IdUsuario)
                    {
                        throw new Exception("El DNI ingresado ya se encuentra registrado en el sistema para otro usuario.");
                    }
                }
            }

            // Validación de Teléfono: Opcional, pero si se completa, solo debe contener números
            if (!string.IsNullOrWhiteSpace(usuario.Telefono) && !Regex.IsMatch(usuario.Telefono, @"^\d+$"))
            {
                throw new Exception("El teléfono solo debe contener números.");
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