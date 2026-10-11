using consorApp.Seguridad;
using ConsorApp.Entidades;
using ConsorApp.Views;
using System;
using System.Windows;

namespace consorApp.Views
{
    public partial class DashboardView : Window
    {
        private readonly string rolUsuarioActual = string.Empty;

        // 1. Restauramos el constructor vacío original que el programa necesita
        public DashboardView()
        {
            InitializeComponent();
        }

        // Este constructor ahora funcionará perfecto porque el de arriba ya existe
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
                BtnMiDpto.Visibility = Visibility.Collapsed;
                BtnAvisos.Visibility = Visibility.Visible;
                BtnReclamos.Visibility = Visibility.Collapsed;
                BtnReservas.Visibility = Visibility.Collapsed; // Reservas para residentes

                BtnEdificio.Visibility = Visibility.Visible;
                BtnLiquidacionExpensas.Visibility = Visibility.Visible;
                BtnUsuarios.Visibility = Visibility.Visible;
                BtnUnidades.Visibility = Visibility.Visible;
                BtnGestionReservasAdmin.Visibility = Visibility.Visible;
                BtnGestionReclamosAdmin.Visibility = Visibility.Visible;
                return;
            }
            // 2. ENCARGADO / OPERARIO
            else if (perfil.Contains("encargado") || perfil.Contains("operario"))
            {
                BtnAvisos.Visibility = Visibility.Visible;
                HeaderGestion.Visibility = Visibility.Visible;
                BtnUnidades.Visibility = Visibility.Visible;
                BtnGestionReservasAdmin.Visibility = Visibility.Visible;
                BtnGestionReclamosAdmin.Visibility = Visibility.Visible;

                BtnMiDpto.Visibility = Visibility.Collapsed;
                BtnEdificio.Visibility = Visibility.Collapsed;
                BtnLiquidacionExpensas.Visibility = Visibility.Collapsed;
                BtnUsuarios.Visibility = Visibility.Collapsed;
                BtnReclamos.Visibility = Visibility.Collapsed;
                BtnReservas.Visibility = Visibility.Collapsed;
                BtnMiDpto.Visibility = Visibility.Collapsed;
            }
            // 3. PROPIETARIO / INQUILINO
            else if (perfil.Contains("propietario") || perfil.Contains("inquilino"))
            {
                BtnMiDpto.Visibility = Visibility.Visible;
                BtnAvisos.Visibility = Visibility.Visible;
                BtnReclamos.Visibility = Visibility.Visible;
                BtnReservas.Visibility = Visibility.Visible;

                HeaderGestion.Visibility = Visibility.Collapsed;
                BtnEdificio.Visibility = Visibility.Collapsed;
                BtnLiquidacionExpensas.Visibility = Visibility.Collapsed;
                BtnUsuarios.Visibility = Visibility.Collapsed;
                BtnUnidades.Visibility = Visibility.Collapsed;
                BtnGestionReservasAdmin.Visibility = Visibility.Collapsed;
                BtnGestionReclamosAdmin.Visibility = Visibility.Collapsed;
            }
        }

        // --- NAVEGACIÓN GENERAL ---

        private void BtnMiDpto_Click(object sender, RoutedEventArgs e)
        {
            MiDepartamentoView ventanaMiDpto = new MiDepartamentoView(SesionUsuario.IdUsuario);
            ventanaMiDpto.ShowDialog();
        }

        private void BtnReclamos_Click(object sender, RoutedEventArgs e)
        {
            ReclamoInquilinoView ventanaReclamo = new ReclamoInquilinoView(SesionUsuario.IdUsuario);
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
            // 2. Usamos directamente SesionUsuario.IdUsuario como en los otros botones
            ReservaEspaciosView ventanaReservas = new ReservaEspaciosView(SesionUsuario.IdUsuario);
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

        private void BtnLiquidacionExpensas_Click(object sender, RoutedEventArgs e)
        {
            LiquidacionExpensasView ventanaExpensas = new LiquidacionExpensasView();
            ventanaExpensas.ShowDialog();
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
            SesionUsuario.CerrarSesion();

            LoginView login = new LoginView();
            login.Show();

            this.Close();
        }
    }
}