using consorApp.Seguridad;
using ConsorApp.Entidades;
using ConsorApp.Negocio; // <-- 1. Asegúrate de tener este using para acceder a las clases de negocio
using ConsorApp.Views;
using ConsorApp.Vistas;
using System;
using System.Windows;

namespace consorApp.Views
{
    public partial class DashboardView : Window
    {
        private readonly string rolUsuarioActual = string.Empty;
        private readonly AvisoNegocio _avisoNegocio = new AvisoNegocio(); // Instancia para consultar avisos

        public DashboardView()
        {
            InitializeComponent();

            // Opcional: Verificar avisos al cargar la ventana
            Loaded += (s, e) => VerificarNotificacionesPendientes();
        }

        public DashboardView(string nombreUsuario, string nombrePerfil) : this()
        {
            TxtNombreUsuario.Text = nombreUsuario ?? "";
            TxtRolUsuario.Text = nombrePerfil ?? "";
            rolUsuarioActual = nombrePerfil ?? string.Empty;

            AplicarPermisosPorPerfil(nombrePerfil ?? "");
        }

        // Método para verificar y mostrar u ocultar el puntito rojo de avisos pendientes
        public void VerificarNotificacionesPendientes()
        {
            try
            {
                // Verificamos si el usuario actual tiene avisos en su bandeja
                var avisos = _avisoNegocio.ObtenerAvisosParaPropietario(SesionUsuario.IdUsuario);

                // Si la lista tiene elementos, mostramos el punto rojo (puedes ajustar esta condición si manejas un campo de "Leído")
                bool tieneAvisos = avisos != null && avisos.Count > 0;

                BadgeAvisosMenu.Visibility = tieneAvisos ? Visibility.Visible : Visibility.Collapsed;
            }
            catch
            {
                // Si ocurre algún error consultando los avisos, ocultamos el indicador por seguridad
                BadgeAvisosMenu.Visibility = Visibility.Collapsed;
            }
        }

        // Método para mostrar u ocultar el puntito rojo manualmente si lo necesitas desde otra parte
        public void ActualizarContadorAvisos(bool tieneAvisosNoLeidos)
        {
            BadgeAvisosMenu.Visibility = tieneAvisosNoLeidos ? Visibility.Visible : Visibility.Collapsed;
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
                BtnReservas.Visibility = Visibility.Collapsed;

                BtnEdificio.Visibility = Visibility.Visible;
                BtnExpensas.Visibility = Visibility.Visible;
                BtnUsuarios.Visibility = Visibility.Visible;
                BtnUnidades.Visibility = Visibility.Visible;
                BtnGestionReservasAdmin.Visibility = Visibility.Visible;
                BtnGestionReclamosAdmin.Visibility = Visibility.Visible;

                BtnReportesAdmin.Visibility = Visibility.Visible;
                BtnRespaldos.Visibility = Visibility.Visible;
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
                BtnExpensas.Visibility = Visibility.Collapsed;
                BtnUsuarios.Visibility = Visibility.Collapsed;
                BtnReclamos.Visibility = Visibility.Collapsed;
                BtnReservas.Visibility = Visibility.Collapsed;

                BtnReportesAdmin.Visibility = Visibility.Collapsed;
                BtnRespaldos.Visibility = Visibility.Collapsed;
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
                BtnExpensas.Visibility = Visibility.Collapsed;
                BtnUsuarios.Visibility = Visibility.Collapsed;
                BtnUnidades.Visibility = Visibility.Collapsed;
                BtnGestionReservasAdmin.Visibility = Visibility.Collapsed;
                BtnGestionReclamosAdmin.Visibility = Visibility.Collapsed;

                BtnReportesAdmin.Visibility = Visibility.Collapsed;
                BtnRespaldos.Visibility = Visibility.Collapsed;
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

            // Al cerrar la ventana de avisos, ocultamos el punto rojo para que no quede encendido
            BadgeAvisosMenu.Visibility = Visibility.Collapsed;
        }

        private void BtnReservas_Click(object sender, RoutedEventArgs e)
        {
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

        private void BtnExpensas_Click(object sender, RoutedEventArgs e)
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

        private void BtnReportesAdmin_Click(object sender, RoutedEventArgs e)
        {
            ReportesEstadisticasView ventanaReportes = new ReportesEstadisticasView();
            ventanaReportes.ShowDialog();
        }

        private void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            SesionUsuario.CerrarSesion();

            LoginView login = new LoginView();
            login.Show();

            this.Close();
        }

        private void BtnRespaldos_Click(object sender, RoutedEventArgs e)
        {
            VentanaRespaldos ventanaRespaldos = new VentanaRespaldos { Owner = this };
            ventanaRespaldos.ShowDialog();
        }
    }
}