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
        private int? idUsuarioSeleccionado = null; // para saber si estoy editando o creando uno nuevo
        private DataView _vistaUsuariosCompleta;   // guardo la tabla aca para poder filtrar despues

        public UsuarioView()
        {
            InitializeComponent();
            CargarPerfiles();
            CargarUsuarios();
        }

        // traigo los usuarios de la base para mostrar en la grilla
        private void CargarUsuarios()
        {
            try
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

                    // guardo la vista para el filtro
                    _vistaUsuariosCompleta = dt.DefaultView;

                    // aplico el filtro por si ya estaba seleccionado algo
                    AplicarFiltroEstado();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los usuarios: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // cargar los perfiles en el ComboBox del formulario
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

        // boton guardar (si id es null inserta con SP, sino hace un update)
        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNombre.Text) ||
                string.IsNullOrWhiteSpace(TxtApellido.Text) ||
                string.IsNullOrWhiteSpace(TxtDni.Text) ||
                string.IsNullOrWhiteSpace(TxtUsuarioSistema.Text) ||
                string.IsNullOrWhiteSpace(TxtContrasenia.Password) ||
                CmbPerfil.SelectedValue == null)
            {
                MessageBox.Show("Por favor, completa los campos obligatorios (Nombre, Apellido, DNI, Usuario, Contraseña y Perfil).", "Campos incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                        // es uno nuevo, uso el procedimiento almacenado
                        comando.CommandText = "sp_InsertarUsuario";
                        comando.CommandType = CommandType.StoredProcedure;

                        comando.Parameters.AddWithValue("@Nombre", TxtNombre.Text.Trim());
                        comando.Parameters.AddWithValue("@Apellido", TxtApellido.Text.Trim());
                        comando.Parameters.AddWithValue("@Dni", TxtDni.Text.Trim());
                        comando.Parameters.AddWithValue("@Telefono", string.IsNullOrWhiteSpace(TxtTelefono.Text) ? (object)DBNull.Value : TxtTelefono.Text.Trim());
                        comando.Parameters.AddWithValue("@Email", string.IsNullOrWhiteSpace(TxtEmail.Text) ? (object)DBNull.Value : TxtEmail.Text.Trim());
                        comando.Parameters.AddWithValue("@UsuarioSistema", TxtUsuarioSistema.Text.Trim());
                        comando.Parameters.AddWithValue("@Contrasenia", TxtContrasenia.Password.Trim());
                        comando.Parameters.AddWithValue("@IdPerfil", CmbPerfil.SelectedValue);
                    }
                    else
                    {
                        // estoy editando uno existente
                        comando.CommandText = @"UPDATE Usuarios 
                                                SET Nombre = @Nombre, Apellido = @Apellido, Dni = @Dni, 
                                                    Telefono = @Telefono, Email = @Email, UsuarioSistema = @UsuarioSistema, 
                                                    Contrasenia = @Contrasenia, IdPerfil = @IdPerfil 
                                                WHERE IdUsuario = @idUsuario";
                        comando.CommandType = CommandType.Text;

                        comando.Parameters.AddWithValue("@idUsuario", idUsuarioSeleccionado.Value);
                        comando.Parameters.AddWithValue("@Nombre", TxtNombre.Text.Trim());
                        comando.Parameters.AddWithValue("@Apellido", TxtApellido.Text.Trim());
                        comando.Parameters.AddWithValue("@Dni", TxtDni.Text.Trim());
                        comando.Parameters.AddWithValue("@Telefono", string.IsNullOrWhiteSpace(TxtTelefono.Text) ? (object)DBNull.Value : TxtTelefono.Text.Trim());
                        comando.Parameters.AddWithValue("@Email", string.IsNullOrWhiteSpace(TxtEmail.Text) ? (object)DBNull.Value : TxtEmail.Text.Trim());
                        comando.Parameters.AddWithValue("@UsuarioSistema", TxtUsuarioSistema.Text.Trim());
                        comando.Parameters.AddWithValue("@Contrasenia", TxtContrasenia.Password.Trim());
                        comando.Parameters.AddWithValue("@IdPerfil", CmbPerfil.SelectedValue);
                    }

                    comando.ExecuteNonQuery();

                    MessageBox.Show(idUsuarioSeleccionado == null ? "Usuario registrado con éxito." : "Usuario actualizado con éxito.", "Operación exitosa", MessageBoxButton.OK, MessageBoxImage.Information);

                    LimpiarCampos();
                    CargarUsuarios();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar el usuario (verifique si el DNI o el Usuario de Sistema ya existen): " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // cuando clickeo un usuario en la tabla para pasarlo a los textbox
        private void DgUsuarios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgUsuarios.SelectedItem is DataRowView row)
            {
                idUsuarioSeleccionado = Convert.ToInt32(row["IdUsuario"]);
                TxtNombre.Text = row["Nombre"].ToString();
                TxtApellido.Text = row["Apellido"].ToString();
                TxtDni.Text = row["Dni"].ToString();
                TxtTelefono.Text = row["Telefono"] != DBNull.Value ? row["Telefono"].ToString() : string.Empty;
                TxtEmail.Text = row["Email"] != DBNull.Value ? row["Email"].ToString() : string.Empty;
                TxtUsuarioSistema.Text = row["UsuarioSistema"].ToString();
                TxtContrasenia.Password = row["Contrasenia"].ToString();
                CmbPerfil.SelectedValue = row["IdPerfil"];
            }
        }

        // avisar que ya esta en modo edicion
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

        // baja logica (cambiar estado a 0 para no borrarlo posta)
        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (idUsuarioSeleccionado == null)
            {
                MessageBox.Show("Por favor, selecciona un usuario de la tabla para dar de baja.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var resultado = MessageBox.Show("¿Estás seguro de que deseas dar de baja este usuario?", "Confirmar baja", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (resultado == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection conexion = new SqlConnection(cadenaConexion))
                    {
                        conexion.Open();
                        string query = "UPDATE Usuarios SET Estado = 0 WHERE IdUsuario = @idUsuario";
                        SqlCommand comando = new SqlCommand(query, conexion);
                        comando.Parameters.AddWithValue("@idUsuario", idUsuarioSeleccionado.Value);
                        comando.ExecuteNonQuery();

                        MessageBox.Show("Usuario dado de baja correctamente.", "Operación exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                        LimpiarCampos();
                        CargarUsuarios();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al dar de baja el usuario: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // volver a activar al usuario (estado a 1)
        private void BtnActivar_Click(object sender, RoutedEventArgs e)
        {
            if (idUsuarioSeleccionado == null)
            {
                MessageBox.Show("Por favor, selecciona un usuario inactivo de la tabla para activar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var resultado = MessageBox.Show("¿Estás seguro de que deseas reactivar este usuario?", "Confirmar activación", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (resultado == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection conexion = new SqlConnection(cadenaConexion))
                    {
                        conexion.Open();
                        string query = "UPDATE Usuarios SET Estado = 1 WHERE IdUsuario = @idUsuario";
                        SqlCommand comando = new SqlCommand(query, conexion);
                        comando.Parameters.AddWithValue("@idUsuario", idUsuarioSeleccionado.Value);
                        comando.ExecuteNonQuery();

                        MessageBox.Show("Usuario reactivado correctamente.", "Operación exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                        LimpiarCampos();
                        CargarUsuarios();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al activar el usuario: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // filtrar la tablita segun lo que elija en el combo
        private void CmbFiltroEstado_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AplicarFiltroEstado();
        }

        private void AplicarFiltroEstado()
        {
            if (_vistaUsuariosCompleta == null || CmbFiltroEstado == null) return;

            if (CmbFiltroEstado.SelectedItem is ComboBoxItem itemSeleccionado)
            {
                string filtro = itemSeleccionado.Content.ToString();

                if (filtro.Contains("Activos"))
                {
                    _vistaUsuariosCompleta.RowFilter = "Estado = 1";
                }
                else if (filtro.Contains("Inactivos"))
                {
                    _vistaUsuariosCompleta.RowFilter = "Estado = 0";
                }
                else
                {
                    // mostrar todos
                    _vistaUsuariosCompleta.RowFilter = string.Empty;
                }

                DgUsuarios.ItemsSource = _vistaUsuariosCompleta;
            }
        }

        // validar que no metan letras en dni y telefono
        private void SoloNumeros_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        // por si pegan texto con ctrl+v que tenga letras
        private void Texto_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(DataFormats.Text))
            {
                string textoPuntual = e.DataObject.GetData(DataFormats.Text) as string;
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("[^0-9]+");

                if (regex.IsMatch(textoPuntual))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        // limpiar todo el formulario y deseleccionar la grilla
        private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarCampos();
        }

        private void LimpiarCampos()
        {
            idUsuarioSeleccionado = null;
            TxtNombre.Clear();
            TxtApellido.Clear();
            TxtDni.Clear();
            TxtTelefono.Clear();
            TxtEmail.Clear();
            TxtUsuarioSistema.Clear();
            TxtContrasenia.Clear();
            CmbPerfil.SelectedIndex = -1;
            DgUsuarios.UnselectAll();
        }
    }
}