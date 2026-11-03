using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using ConsorApp.Entidades;
using ConsorApp.Negocio;

namespace ConsorApp.Vistas
{
    public partial class VentanaRespaldos : Window
    {
        private readonly RespaldoService _servicio = new RespaldoService();
        private readonly ObservableCollection<ArchivoRespaldo> _archivos = new ObservableCollection<ArchivoRespaldo>();
        private bool _ocupado;

        public VentanaRespaldos()
        {
            InitializeComponent();
            GridRespaldos.ItemsSource = _archivos;
            Loaded += async (s, e) => await ActualizarListaAsync();
        }

        private ArchivoRespaldo ArchivoSeleccionado => GridRespaldos.SelectedItem as ArchivoRespaldo;

        // ---------- Botones ----------

        private async void BtnHacer_Click(object sender, RoutedEventArgs e)
        {
            await EjecutarAsync("Creando y verificando el respaldo...", async () =>
            {
                string nombre = await _servicio.HacerBackupAsync();
                return $"Respaldo creado y verificado: {nombre}";
            });
        }

        private async void BtnProbar_Click(object sender, RoutedEventArgs e)
        {
            var archivo = ArchivoSeleccionado;
            if (archivo == null) return;

            await EjecutarAsync("Verificando el respaldo...", async () =>
            {
                await _servicio.ProbarBackupAsync(archivo);
                return $"El respaldo es válido: {archivo.Nombre}";
            });
        }

        private async void BtnRestaurar_Click(object sender, RoutedEventArgs e)
        {
            var archivo = ArchivoSeleccionado;
            if (archivo == null) return;

            var respuesta = MessageBox.Show(this,
                $"Vas a REEMPLAZAR todos los datos actuales por los del respaldo:\n\n{archivo.Nombre}\n\n" +
                "Los cambios hechos después de esa fecha se perderán. ¿Querés continuar?",
                "Restaurar respaldo", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);

            if (respuesta != MessageBoxResult.Yes) return;

            await EjecutarAsync("Restaurando la base de datos...", async () =>
            {
                await _servicio.RestaurarAsync(archivo);
                return "Restauración completada. Cerrá y volvé a abrir la aplicación para ver los datos restaurados.";
            });
        }

        private async void BtnActualizar_Click(object sender, RoutedEventArgs e)
        {
            await ActualizarListaAsync();
        }

        private async void BtnConfig_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new VentanaConfigRespaldos { Owner = this };
            ventana.ShowDialog();
            await ActualizarListaAsync();
        }

        private void GridRespaldos_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ActualizarBotones();
        }

        // ---------- Auxiliares ----------

        private async Task ActualizarListaAsync()
        {
            try
            {
                var lista = await Task.Run(() => _servicio.ListarRespaldos());
                _archivos.Clear();
                foreach (var a in lista) _archivos.Add(a);
                TxtEstado.Text = _archivos.Count == 0
                    ? "No hay respaldos todavía. Usá 'Hacer backup' para crear el primero."
                    : $"{_archivos.Count} respaldo(s) encontrados.";
            }
            catch (Exception ex)
            {
                _archivos.Clear();
                TxtEstado.Text = ex.Message;
            }

            ActualizarBotones();
        }

        private async Task EjecutarAsync(string mensajeProgreso, Func<Task<string>> accion)
        {
            SetOcupado(true, mensajeProgreso);
            try
            {
                string resultado = await accion();
                TxtEstado.Text = resultado;
                MessageBox.Show(this, resultado, "Respaldos", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                TxtEstado.Text = "Error: " + ex.Message;
                MessageBox.Show(this, ex.Message, "Respaldos", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SetOcupado(false, null);
            }

            await ActualizarListaAsync();
        }

        private void SetOcupado(bool ocupado, string mensaje)
        {
            _ocupado = ocupado;
            PbOcupado.Visibility = ocupado ? Visibility.Visible : Visibility.Collapsed;
            if (mensaje != null) TxtEstado.Text = mensaje;
            ActualizarBotones();
        }

        private void ActualizarBotones()
        {
            bool haySeleccion = ArchivoSeleccionado != null;

            BtnHacer.IsEnabled = !_ocupado;
            BtnActualizar.IsEnabled = !_ocupado;
            BtnConfig.IsEnabled = !_ocupado;
            BtnProbar.IsEnabled = !_ocupado && haySeleccion;
            BtnRestaurar.IsEnabled = !_ocupado && haySeleccion;
        }
    }
}