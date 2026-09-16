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
using ConsorApp.Negocio;
using ConsorApp.Entidades;

namespace consorApp.Views
{
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private void BtnIngresar_Click(object sender, RoutedEventArgs e)
        {
            string usuario = TxtUsuario.Text;
            string password = TxtPassword.Password;

            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Por favor, ingresa usuario y contraseña.", "Campos vacíos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            UsuarioNegocio negocio = new UsuarioNegocio();

            // Agregamos '?' a Usuario? para indicar explícitamente que el resultado puede ser nulo
            Usuario? usuarioLogueado = negocio.IniciarSesion(usuario, password);

            if (usuarioLogueado != null)
            {
                string nombreCompleto = $"{usuarioLogueado.Nombre} {usuarioLogueado.Apellido}";
                string nombrePerfil = usuarioLogueado.Perfil?.NombrePerfil ?? "Usuario";

                DashboardView dashboard = new DashboardView(nombreCompleto, nombrePerfil);
                dashboard.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Usuario o contraseña incorrectos.", "Error de Autenticación", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}