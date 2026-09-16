using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using ConsorApp.Entidades;

namespace consorApp.Views
{
    public partial class UsuarioView : Window
    {
        private string cadenaConexion = "Server=.\\SQLEXPRESS;Database=consorAppDb;Integrated Security=True;TrustServerCertificate=True;";
        private int? idUsuarioSeleccionado = null; // para saber si estamos editando o creando uno nuevo

        public UsuarioView()
        {
            InitializeComponent();
            CargarPerfiles();
            CargarUsuarios();
        }

        // 1. Cargar la tabla con los usuarios de la base de datos
        private void CargarUsuarios()
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(cadenaConexion))
                {
                    string query = @"
                SELECT u.IdUsuario, u.Nombre, u.Apellido, u.UsuarioSistema, u.Contrasenia, u.IdPerfil, p.NombrePerfil AS NombrePerfil
                FROM Usuarios u
                INNER JOIN Perfiles p ON u.IdPerfil = p.IdPerfil";

                    SqlDataAdapter adaptador = new SqlDataAdapter(query, conexion);
                    DataTable dt = new DataTable();
                    adaptador.Fill(dt);

                    DgUsuarios.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los usuarios: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 2. Cargar los perfiles en el ComboBox
        private void CargarPerfiles()
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(cadenaConexion))
                {
                    string query = "SELECT IdPerfil, NombrePerfil FROM Perfiles";
                    SqlDataAdapter adaptador = new SqlDataAdapter(query, conexion);
                    DataTable dt = new DataTable();
                    adaptador.Fill(dt);

                    CmbPerfil.ItemsSource = dt.DefaultView;
                    CmbPerfil.DisplayMemberPath = "NombrePerfil";
                    CmbPerfil.SelectedValuePath = "IdPerfil";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los perfiles: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 3. Botón Guardar (Sirve tanto para Insertar como para Modificar)
        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNombre.Text) ||
                string.IsNullOrWhiteSpace(TxtApellido.Text) ||
                string.IsNullOrWhiteSpace(TxtUsuarioSistema.Text) ||
                string.IsNullOrWhiteSpace(TxtContrasenia.Password) ||
                CmbPerfil.SelectedValue == null)
            {
                MessageBox.Show("Por favor, completa todos los campos.", "Campos incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection conexion = new SqlConnection(cadenaConexion))
                {
                    conexion.Open();
                    SqlCommand comando = new SqlCommand();
                    comando.Connection = conexion;

                    if (idUsuarioSeleccionado == null)
                    {
                        // INSERTAR NUEVO USUARIO
                        comando.CommandText = @"INSERT INTO Usuarios (Nombre, Apellido, UsuarioSistema, Contrasenia, IdPerfil) 
                                                VALUES (@nombre, @apellido, @usuario, @password, @idPerfil)";
                    }
                    else
                    {
                        // MODIFICAR USUARIO EXISTENTE
                        comando.CommandText = @"UPDATE Usuarios 
                                                SET Nombre = @nombre, Apellido = @apellido, UsuarioSistema = @usuario, 
                                                    Contrasenia = @password, IdPerfil = @idPerfil 
                                                WHERE IdUsuario = @idUsuario";
                        comando.Parameters.AddWithValue("@idUsuario", idUsuarioSeleccionado.Value);
                    }

                    comando.Parameters.AddWithValue("@nombre", TxtNombre.Text.Trim());
                    comando.Parameters.AddWithValue("@apellido", TxtApellido.Text.Trim());
                    comando.Parameters.AddWithValue("@usuario", TxtUsuarioSistema.Text.Trim());
                    comando.Parameters.AddWithValue("@password", TxtContrasenia.Password.Trim());
                    comando.Parameters.AddWithValue("@idPerfil", CmbPerfil.SelectedValue);

                    comando.ExecuteNonQuery();

                    MessageBox.Show(idUsuarioSeleccionado == null ? "Usuario registrado con éxito." : "Usuario actualizado con éxito.", "Operación exitosa", MessageBoxButton.OK, MessageBoxImage.Information);

                    LimpiarCampos();
                    CargarUsuarios();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar el usuario: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 4. Seleccionar un usuario de la tabla para editarlo
        private void DgUsuarios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgUsuarios.SelectedItem is DataRowView row)
            {
                idUsuarioSeleccionado = Convert.ToInt32(row["IdUsuario"]);
                TxtNombre.Text = row["Nombre"].ToString();
                TxtApellido.Text = row["Apellido"].ToString();
                TxtUsuarioSistema.Text = row["UsuarioSistema"].ToString();
                TxtContrasenia.Password = row["Contrasenia"].ToString();
                CmbPerfil.SelectedValue = row["IdPerfil"];
            }
        }

        // 5. Botón Modificar (Activa el modo edición sobre el seleccionado)
        private void BtnModificar_Click(object sender, RoutedEventArgs e)
        {
            if (idUsuarioSeleccionado == null)
            {
                MessageBox.Show("Por favor, selecciona un usuario de la tabla para modificar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                MessageBox.Show("Modifica los datos en el panel izquierdo y presiona 'Guardar' para aplicar los cambios.", "Modo Edición", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // 6. Botón Eliminar
        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (idUsuarioSeleccionado == null)
            {
                MessageBox.Show("Por favor, selecciona un usuario de la tabla para eliminar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var resultado = MessageBox.Show("¿Estás seguro de que deseas eliminar este usuario?", "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (resultado == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection conexion = new SqlConnection(cadenaConexion))
                    {
                        conexion.Open();
                        string query = "DELETE FROM Usuarios WHERE IdUsuario = @idUsuario";
                        SqlCommand comando = new SqlCommand(query, conexion);
                        comando.Parameters.AddWithValue("@idUsuario", idUsuarioSeleccionado.Value);
                        comando.ExecuteNonQuery();

                        MessageBox.Show("Usuario eliminado correctamente.", "Eliminado", MessageBoxButton.OK, MessageBoxImage.Information);
                        LimpiarCampos();
                        CargarUsuarios();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al eliminar el usuario: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // 7. Botón Limpiar Campos
        private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarCampos();
        }

        private void LimpiarCampos()
        {
            idUsuarioSeleccionado = null;
            TxtNombre.Clear();
            TxtApellido.Clear();
            TxtUsuarioSistema.Clear();
            TxtContrasenia.Clear();
            CmbPerfil.SelectedIndex = -1;
            DgUsuarios.UnselectAll();
        }
    }
}