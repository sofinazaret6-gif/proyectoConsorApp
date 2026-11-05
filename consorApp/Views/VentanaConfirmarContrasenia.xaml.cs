using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System;
using consorApp.Seguridad;
using ConsorApp.Negocio;

namespace ConsorApp.Views
{
    /// <summary>
    /// Pide la contraseña del administrador que tiene la sesión iniciada
    /// antes de una operación crítica (restaurar un respaldo).
    /// DialogResult = true solo si la contraseña es correcta.
    /// </summary>
    public partial class VentanaConfirmarContrasenia : Window
    {
        private const int MaxIntentos = 3;

        private readonly UsuarioNegocio _usuarios = new UsuarioNegocio();
        private int _intentos;

        public VentanaConfirmarContrasenia()
        {
            InitializeComponent();
            TxtUsuario.Text = SesionUsuario.UsuarioSistema;
            Loaded += (s, e) => TxtPassword.Focus();
        }

        private void BtnConfirmar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _usuarios.ValidarContraseniaActual(
                    SesionUsuario.UsuarioSistema, TxtPassword.Password, SesionUsuario.IdUsuario);

                DialogResult = true;
            }
            catch (Exception ex)
            {
                _intentos++;
                TxtPassword.Clear();

                if (_intentos >= MaxIntentos)
                {
                    MessageBox.Show(this,
                        "Se superó el máximo de intentos. La restauración fue cancelada.",
                        "Confirmación denegada", MessageBoxButton.OK, MessageBoxImage.Warning);
                    DialogResult = false;
                    return;
                }

                TxtError.Text = $"{ex.Message} (Intento {_intentos} de {MaxIntentos})";
                TxtPassword.Focus();
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}

