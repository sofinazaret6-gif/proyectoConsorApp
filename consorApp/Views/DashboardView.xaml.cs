using System;
using System.Windows;

namespace consorApp.Views
{
    public partial class DashboardView : Window
    {
        private readonly string rolUsuarioActual = string.Empty;

        public DashboardView()
        {
            InitializeComponent();
        }

        public DashboardView(string nombreUsuario, string nombrePerfil) : this()
        {
            TxtNombreUsuario.Text = nombreUsuario ?? "";
            TxtRolUsuario.Text = nombrePerfil ?? "";
            rolUsuarioActual = nombrePerfil ?? string.Empty;

            AplicarPermisosPorPerfil(nombrePerfil ?? "");
        }

        private void AplicarPermisosPorPerfil(string nombrePerfil)
        {
            if (string.IsNullOrEmpty(nombrePerfil)) return;

            string perfil = nombrePerfil.ToLower().Trim();

            // 1. ADMINISTRADOR: Ve absolutamente todo
            if (perfil.Contains("admin"))
            {
                HeaderGestion.Visibility = Visibility.Visible;
                BtnMiDpto.Visibility = Visibility.Visible;
                BtnAvisos.Visibility = Visibility.Visible;
                BtnReclamos.Visibility = Visibility.Visible;
                BtnReservas.Visibility = Visibility.Visible;

                BtnEdificio.Visibility = Visibility.Visible;
                BtnExpensas.Visibility = Visibility.Visible;
                BtnUsuarios.Visibility = Visibility.Visible;
                BtnUnidades.Visibility = Visibility.Visible;
                return;
            }
            // 2. ENCARGADO / OPERARIO: Ve Departamentos, Avisos, Reservas y Reclamos
            else if (perfil.Contains("encargado") || perfil.Contains("operario"))
            {
                // Secciones visibles
                BtnAvisos.Visibility = Visibility.Visible;
                BtnReclamos.Visibility = Visibility.Visible;
                BtnReservas.Visibility = Visibility.Visible;

                HeaderGestion.Visibility = Visibility.Visible;
                BtnUnidades.Visibility = Visibility.Visible; // Departamentos

                // Secciones ocultas
                BtnMiDpto.Visibility = Visibility.Collapsed;
                BtnEdificio.Visibility = Visibility.Collapsed;
                BtnExpensas.Visibility = Visibility.Collapsed;
                BtnUsuarios.Visibility = Visibility.Collapsed;
            }
            // 3. PROPIETARIO / INQUILINO: Ve Mi Departamento, Avisos, Reclamos y Reservas
            else if (perfil.Contains("propietario") || perfil.Contains("inquilino"))
            {
                // Secciones visibles
                BtnMiDpto.Visibility = Visibility.Visible;
                BtnAvisos.Visibility = Visibility.Visible;
                BtnReclamos.Visibility = Visibility.Visible;
                BtnReservas.Visibility = Visibility.Visible;

                // Toda la sección de gestión oculta
                HeaderGestion.Visibility = Visibility.Collapsed;
                BtnEdificio.Visibility = Visibility.Collapsed;
                BtnExpensas.Visibility = Visibility.Collapsed;
                BtnUsuarios.Visibility = Visibility.Collapsed;
                BtnUnidades.Visibility = Visibility.Collapsed;
            }
        }

        // --- NAVEGACIÓN GENERAL ---

        private void BtnMiDpto_Click(object sender, RoutedEventArgs e)
        {
            MiDepartamentoView ventanaMiDpto = new MiDepartamentoView();
            ventanaMiDpto.ShowDialog();
        }

        private void BtnAvisos_Click(object sender, RoutedEventArgs e)
        {
            string perfil = rolUsuarioActual.ToLower().Trim();

            if (perfil.Contains("encargado") || perfil.Contains("operario") || perfil.Contains("admin"))
            {
                AvisosEncargadoView ventanaAvisosEncargado = new AvisosEncargadoView();
                ventanaAvisosEncargado.ShowDialog();
            }
            else
            {
                AvisosPropietarioView ventanaAvisosPropietario = new AvisosPropietarioView();
                ventanaAvisosPropietario.ShowDialog();
            }
        }

        // --- NAVEGACIÓN GESTIÓN ---

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