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

namespace consorApp.Views
{
    public partial class DashboardView : Window
    {
        public DashboardView()
        {
            InitializeComponent();
        }

        // Recibe el nombre completo del usuario y el NombrePerfil tal como viene de la tabla dbo.Perfiles
        public DashboardView(string nombreUsuario, string nombrePerfil) : this()
        {
            TxtNombreUsuario.Text = nombreUsuario;
            TxtRolUsuario.Text = nombrePerfil;

            AplicarPermisosPorPerfil(nombrePerfil);
        }

        private void AplicarPermisosPorPerfil(string nombrePerfil)
        {
            // Convertimos a minúsculas para comparar de forma segura sin importar mayúsculas/minúsculas
            string perfil = nombrePerfil.ToLower();

            // Administrador: Ve todo, no ocultamos nada
            if (perfil.Contains("admin"))
            {
                return;
            }
            // Encargado / Mantenimiento: Se oculta gestión financiera y de usuarios
            else if (perfil.Contains("encargado"))
            {
                BtnExpensas.Visibility = Visibility.Collapsed;
                BtnUsuarios.Visibility = Visibility.Collapsed;
            }
            // Propietario / Inquilino: Se oculta todo el bloque de gestión
            else if (perfil.Contains("propietario"))
            {
                HeaderGestion.Visibility = Visibility.Collapsed;
                BtnExpensas.Visibility = Visibility.Collapsed;
                BtnUsuarios.Visibility = Visibility.Collapsed;
                BtnUnidades.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            LoginView login = new LoginView();
            login.Show();
            this.Close();
        }
    }
}
