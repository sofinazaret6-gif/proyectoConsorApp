using System;
using System.Windows;

namespace consorApp.Views
{
    public partial class DashboardView : Window
    {
        public DashboardView()
        {
            InitializeComponent();
        }

        public DashboardView(string nombreUsuario, string nombrePerfil) : this()
        {
            TxtNombreUsuario.Text = nombreUsuario;
            TxtRolUsuario.Text = nombrePerfil;

            AplicarPermisosPorPerfil(nombrePerfil);
        }

        private void AplicarPermisosPorPerfil(string nombrePerfil)
        {
            string perfil = nombrePerfil.ToLower();

            // Administrador: Tiene acceso completo a todo (incluyendo Datos Edificio)
            if (perfil.Contains("admin"))
            {
                BtnEdificio.Visibility = Visibility.Visible;
                return;
            }
            // Encargado: Se le oculta Edificio, Expensas y Usuarios
            else if (perfil.Contains("encargado"))
            {
                BtnEdificio.Visibility = Visibility.Collapsed;
                BtnExpensas.Visibility = Visibility.Collapsed;
                BtnUsuarios.Visibility = Visibility.Collapsed;
            }
            // Propietario / Inquilino: Se oculta toda la sección de gestión
            else if (perfil.Contains("propietario") || perfil.Contains("inquilino"))
            {
                HeaderGestion.Visibility = Visibility.Collapsed;
                BtnEdificio.Visibility = Visibility.Collapsed;
                BtnExpensas.Visibility = Visibility.Collapsed;
                BtnUsuarios.Visibility = Visibility.Collapsed;
                BtnUnidades.Visibility = Visibility.Collapsed;
            }
        }

        // Abrir la ventana de Datos Edificio
        private void BtnEdificio_Click(object sender, RoutedEventArgs e)
        {
            EdificioView ventanaEdificio = new EdificioView();
            ventanaEdificio.ShowDialog();
        }

        private void BtnUsuarios_Click(object sender, RoutedEventArgs e)
        {
            UsuarioView ventanaUsuarios = new UsuarioView();
            ventanaUsuarios.ShowDialog();
        }
        private void BtnUnidades_Click(object sender, RoutedEventArgs e)
        {
            DepartamentoView ventanaDepartamentos = new DepartamentoView();
            ventanaDepartamentos.ShowDialog();
        }
        private void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            LoginView login = new LoginView();
            login.Show();
            this.Close();
        }
    }
}