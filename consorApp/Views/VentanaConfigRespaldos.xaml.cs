using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using ConsorApp.Negocio;

namespace ConsorApp.Vistas
{
    public partial class VentanaConfigRespaldos : Window
    {
        private readonly RespaldoService _servicio = new RespaldoService();

        public VentanaConfigRespaldos()
        {
            InitializeComponent();
            Loaded += (s, e) => CargarDatos();
        }

        private void CargarDatos()
        {
            try
            {
                var cfg = _servicio.ObtenerConfiguracion();
                TxtBaseDatos.Text = cfg.BaseDatos;
                TxtRutaServidor.Text = cfg.RutaServidor;
                TxtRutaCompartida.Text = cfg.RutaCompartida;

                TxtUltimaSubida.Text = string.IsNullOrWhiteSpace(cfg.UltimoArchivoSubido)
                    ? "Todavía no se subió ningún respaldo."
                    : $"Último archivo resguardado: {cfg.UltimoArchivoSubido} ({cfg.UltimaSubidaFecha:dd/MM/yyyy HH:mm})";
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Configuración de respaldos",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            ActualizarEstadoDrive();
        }

        private void ActualizarEstadoDrive()
        {
            bool conectado = _servicio.DriveConectado;
            BtnDrive.Content = conectado ? "Desconectar Google Drive" : "Conectar con Google Drive";
            TxtEstadoDrive.Text = conectado ? "✔ Conectado con Google Drive en esta PC." : "No conectado.";
        }

        private async void BtnDrive_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_servicio.DriveConectado)
                {
                    var r = MessageBox.Show(this, "¿Desconectar Google Drive en esta PC?", "Google Drive",
                        MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (r == MessageBoxResult.Yes) _servicio.DesconectarDrive();
                }
                else
                {
                    BtnDrive.IsEnabled = false;
                    TxtEstadoDrive.Text = "Esperando la autorización en el navegador...";
                    await _servicio.ConectarDriveAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Google Drive", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                BtnDrive.IsEnabled = true;
                ActualizarEstadoDrive();
            }
        }

        private void BtnExaminarServidor_Click(object sender, RoutedEventArgs e) => Examinar(TxtRutaServidor);

        private void BtnExaminarCompartida_Click(object sender, RoutedEventArgs e) => Examinar(TxtRutaCompartida);

        private static void Examinar(TextBox destino)
        {
            var dialogo = new Microsoft.Win32.OpenFolderDialog { Title = "Seleccionar carpeta" };
            if (!string.IsNullOrWhiteSpace(destino.Text) && Directory.Exists(destino.Text))
                dialogo.InitialDirectory = destino.Text;

            if (dialogo.ShowDialog() == true)
                destino.Text = dialogo.FolderName;
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtRutaServidor.Text))
            {
                MessageBox.Show(this, "Indicá la ruta en el servidor.", "Configuración de respaldos",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _servicio.GuardarRutas(TxtRutaServidor.Text, TxtRutaCompartida.Text);
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Configuración de respaldos",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
