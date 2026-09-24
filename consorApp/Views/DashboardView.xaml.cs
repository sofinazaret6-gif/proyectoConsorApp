using ConsorApp.Views;
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
                BtnReservas.Visibility = Visibility.Visible; // Reservas para residentes

                BtnEdificio.Visibility = Visibility.Visible;
                BtnExpensas.Visibility = Visibility.Visible;
                BtnUsuarios.Visibility = Visibility.Visible;
                BtnUnidades.Visibility = Visibility.Visible;
                BtnGestionReservasAdmin.Visibility = Visibility.Visible; // Control de reservas Admin/Encargado
                BtnGestionReclamosAdmin.Visibility = Visibility.Visible; // Control de reclamos Admin/Encargado
                return;
            }
            // 2. ENCARGADO / OPERARIO: Ve Departamentos, Avisos, Reservas, Reclamos y Controles
            else if (perfil.Contains("encargado") || perfil.Contains("operario"))
            {
                // Secciones visibles
                BtnAvisos.Visibility = Visibility.Visible;
                BtnReclamos.Visibility = Visibility.Visible;
                BtnReservas.Visibility = Visibility.Visible;

                HeaderGestion.Visibility = Visibility.Visible;
                BtnUnidades.Visibility = Visibility.Visible; // Departamentos
                BtnGestionReservasAdmin.Visibility = Visibility.Visible; // Control de reservas Admin/Encargado
                BtnGestionReclamosAdmin.Visibility = Visibility.Visible; // Control de reclamos Admin/Encargado

                // Secciones ocultas
                BtnMiDpto.Visibility = Visibility.Collapsed;
                BtnEdificio.Visibility = Visibility.Collapsed;
                BtnExpensas.Visibility = Visibility.Collapsed;
                BtnUsuarios.Visibility = Visibility.Collapsed;
            }
            // 3. PROPIETARIO / INQUILINO: Ve Mi Departamento, Avisos, Reclamos y Reservas (Sección gestión oculta)
            else if (perfil.Contains("propietario") || perfil.Contains("inquilino"))
            {
                // Secciones visibles
                BtnMiDpto.Visibility = Visibility.Visible;
                BtnAvisos.Visibility = Visibility.Visible;
                BtnReclamos.Visibility = Visibility.Visible;
                BtnReservas.Visibility = Visibility.Visible; // Único botón de reservas para ellos

                // Toda la sección de gestión oculta
                HeaderGestion.Visibility = Visibility.Collapsed;
                BtnEdificio.Visibility = Visibility.Collapsed;
                BtnExpensas.Visibility = Visibility.Collapsed;
                BtnUsuarios.Visibility = Visibility.Collapsed;
                BtnUnidades.Visibility = Visibility.Collapsed;
                BtnGestionReservasAdmin.Visibility = Visibility.Collapsed;
                BtnGestionReclamosAdmin.Visibility = Visibility.Collapsed;
            }
        }

        // --- NAVEGACIÓN GENERAL ---

        private void BtnMiDpto_Click(object sender, RoutedEventArgs e)
        {
            MiDepartamentoView ventanaMiDpto = new MiDepartamentoView();
            ventanaMiDpto.ShowDialog();
        }

        private void BtnReclamos_Click(object sender, RoutedEventArgs e)
        {
            ReclamoInquilinoView ventanaReclamo = new ReclamoInquilinoView();
            ventanaReclamo.ShowDialog();
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

        private void BtnReservas_Click(object sender, RoutedEventArgs e)
        {
            ReservaEspaciosView ventanaReservas = new ReservaEspaciosView();
            ventanaReservas.ShowDialog();
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

        private void BtnGestionReservasAdmin_Click(object sender, RoutedEventArgs e)
        {
            GestionReservasAdminView ventanaAdminReservas = new GestionReservasAdminView();
            ventanaAdminReservas.ShowDialog();
        }

        private void BtnGestionReclamosAdmin_Click(object sender, RoutedEventArgs e)
        {
            GestionReclamosAdminView ventanaAdminReclamos = new GestionReclamosAdminView();
            ventanaAdminReclamos.ShowDialog();
        }

        private void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            LoginView login = new LoginView();
            login.Show();
            this.Close();
        }
    }
}