using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using ConsorApp.Entidades;
using ConsorApp.Negocio;

namespace consorApp.Views
{
    public partial class UsuarioView : Window
    {
        private int? idUsuarioSeleccionado = null;
        private DataView _vistaUsuariosCompleta = new DataView();
        private readonly UsuarioNegocio _usuarioNegocio = new UsuarioNegocio();

        public UsuarioView()
        {
            InitializeComponent();
            CargarPerfiles();
            CargarUsuarios();
        }

        private void CargarUsuarios()
        {
            try
            {
                DataTable dt = _usuarioNegocio.ObtenerUsuarios();
                _vistaUsuariosCompleta = dt.DefaultView;
                AplicarFiltroEstado();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los usuarios: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarPerfiles()
        {
            try
            {
                DataTable dt = _usuarioNegocio.ObtenerPerfiles();
                CmbPerfil.ItemsSource = dt.DefaultView;
                CmbPerfil.DisplayMemberPath = "NombrePerfil";
                CmbPerfil.SelectedValuePath = "IdPerfil";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los perfiles: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Usamos UsuarioSistema y Contrasenia tal como los definieron
                Usuario usuarioInput = new Usuario
                {
                    IdUsuario = idUsuarioSeleccionado ?? 0,
                    Nombre = TxtNombre.Text.Trim(),
                    Apellido = TxtApellido.Text.Trim(),
                    Dni = TxtDni.Text.Trim(),
                    Email = TxtEmail.Text.Trim(),
                    UsuarioSistema = TxtUsuarioSistema.Text.Trim(),
                    Contrasenia = TxtContrasenia.Password.Trim(),
                    IdPerfil = CmbPerfil.SelectedValue != null ? Convert.ToInt32(CmbPerfil.SelectedValue) : 0
                };

                // Ejecutamos la validación de DNI (8 dígitos) y Email (@)
                _usuarioNegocio.ValidarUsuario(usuarioInput);

                if (idUsuarioSeleccionado == null)
                {
                    _usuarioNegocio.GuardarUsuario(usuarioInput);
                    MessageBox.Show("Usuario registrado con éxito.", "Operación exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _usuarioNegocio.ActualizarUsuario(usuarioInput);
                    MessageBox.Show("Usuario actualizado con éxito.", "Operación exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                LimpiarCampos();
                CargarUsuarios();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Validación de Usuario", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

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

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (idUsuarioSeleccionado == null)
            {
                MessageBox.Show("Por favor, selecciona un usuario de la tabla para dar de baja.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("¿Estás seguro de que deseas dar de baja este usuario?", "Confirmar baja", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    _usuarioNegocio.CambiarEstadoUsuario(idUsuarioSeleccionado.Value, 0);
                    MessageBox.Show("Usuario dado de baja correctamente.", "Operación exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                    LimpiarCampos();
                    CargarUsuarios();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al dar de baja: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnActivar_Click(object sender, RoutedEventArgs e)
        {
            if (idUsuarioSeleccionado == null)
            {
                MessageBox.Show("Por favor, selecciona un usuario inactivo de la tabla para activar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("¿Estás seguro de que deseas reactivar este usuario?", "Confirmar activación", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    _usuarioNegocio.CambiarEstadoUsuario(idUsuarioSeleccionado.Value, 1);
                    MessageBox.Show("Usuario reactivado correctamente.", "Operación exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                    LimpiarCampos();
                    CargarUsuarios();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al activar: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CmbFiltroEstado_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AplicarFiltroEstado();
        }

        private void AplicarFiltroEstado()
        {
            // Validamos que tanto la vista como el ComboBox y el DataGrid no sean nulos
            if (_vistaUsuariosCompleta == null || CmbFiltroEstado == null || DgUsuarios == null) return;

            if (CmbFiltroEstado.SelectedItem is ComboBoxItem itemSeleccionado)
            {
                string filtro = itemSeleccionado.Content.ToString();

                if (filtro.Contains("Activos"))
                    _vistaUsuariosCompleta.RowFilter = "Estado = 1";
                else if (filtro.Contains("Inactivos"))
                    _vistaUsuariosCompleta.RowFilter = "Estado = 0";
                else
                    _vistaUsuariosCompleta.RowFilter = string.Empty;

                DgUsuarios.ItemsSource = _vistaUsuariosCompleta;
            }
        }

        private void SoloNumeros_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

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