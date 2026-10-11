using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using ConsorApp.Entidades;
using ConsorApp.Negocio;

namespace consorApp.Views
{
    public partial class MiDepartamentoView : Window
    {
        private readonly HistorialExpensaNegocio _historialNegocio = new HistorialExpensaNegocio();
        private readonly ExpensasNegocio _expensasNegocio = new ExpensasNegocio();
        private readonly GastoEdificioNegocio _gastoNegocio = new GastoEdificioNegocio();
        private readonly MiDepartamentoNegocio _miDepartamentoNegocio = new MiDepartamentoNegocio();
        private readonly ReclamoNegocio _reclamoNegocio = new ReclamoNegocio();
        private readonly ReservaEspacioNegocio _reservaNegocio = new ReservaEspacioNegocio(); // Agregado para reservas

        private static readonly CultureInfo Cultura = new CultureInfo("es-AR");

        private readonly int _idUsuario;
        private MiDepartamentoInfo? _info;

        public MiDepartamentoView(int idUsuario)
        {
            InitializeComponent();

            _idUsuario = idUsuario;

            CargarDatosPersonales();
            CargarDatosDepartamento(); // Esto carga _info, es vital que esté antes de las reservas
            CargarMisExpensas();
            CargarMisReclamos();
            CargarMisReservasReales(); // Llamada al método real
        }

        // ============================================================
        // TARJETAS: PROPIETARIO Y UNIDAD
        // ============================================================

        private void CargarDatosPersonales()
        {
            string nombre = string.Empty;
            string apellido = string.Empty;
            string dni = string.Empty;
            string email = string.Empty;
            string telefono = string.Empty;

            try
            {
                DataTable dt = new UsuarioNegocio().ObtenerUsuarios();

                DataRow? fila = dt.AsEnumerable()
                    .FirstOrDefault(r => Convert.ToInt32(r["IdUsuario"]) == _idUsuario);

                if (fila != null)
                {
                    nombre = LeerTexto(fila, "Nombre");
                    apellido = LeerTexto(fila, "Apellido");
                    dni = LeerTexto(fila, "Dni");
                    email = LeerTexto(fila, "Email", "Correo", "Mail");
                    telefono = LeerTexto(fila, "Telefono", "Teléfono", "Celular");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudieron cargar tus datos personales.\n\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            string nombreCompleto = $"{nombre} {apellido}".Trim();

            TxtPropietarioNombre.Text = string.IsNullOrWhiteSpace(nombreCompleto) ? "-" : nombreCompleto;
            TxtPropietarioDni.Text = $"DNI: {ValorOPlaceholder(dni)}";
            TxtPropietarioEmail.Text = $"Email: {ValorOPlaceholder(email)}";
            TxtPropietarioTelefono.Text = $"Teléfono: {ValorOPlaceholder(telefono)}";
        }

        private static string LeerTexto(DataRow fila, params string[] nombresColumna)
        {
            foreach (string nombre in nombresColumna)
            {
                if (fila.Table.Columns.Contains(nombre) && fila[nombre] != DBNull.Value)
                    return fila[nombre]?.ToString()?.Trim() ?? string.Empty;
            }
            return string.Empty;
        }

        private void CargarDatosDepartamento()
        {
            try
            {
                _info = _miDepartamentoNegocio.ObtenerInfo(_idUsuario);

                if (_info == null)
                {
                    TxtUnidad.Text = "Sin departamento asignado";
                    TxtUnidadDetalle.Text = string.Empty;
                    return;
                }

                TxtUnidad.Text = $"Piso {_info.Departamento.Piso} - Unidad {_info.Departamento.Unidad}";
                TxtUnidadDetalle.Text = $"Propietario desde {_info.Propietario.FechaDesde:dd/MM/yyyy}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudieron cargar los datos del departamento.\n\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string ValorOPlaceholder(string valor) =>
            string.IsNullOrWhiteSpace(valor) ? "No registrado" : valor;

        // ============================================================
        // MIS EXPENSAS
        // ============================================================

        private void CargarMisExpensas()
        {
            try
            {
                var expensas = _historialNegocio
                    .ObtenerHistorialPorUsuario(_idUsuario)
                    .Select(h => new ExpensaPropietarioItem(h))
                    .OrderByDescending(x => x.FechaVencimiento)
                    .ToList();

                DgMisExpensas.ItemsSource = expensas;
                ActualizarEstadoCuenta(expensas);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudieron cargar tus expensas.\n\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ActualizarEstadoCuenta(List<ExpensaPropietarioItem> expensas)
        {
            var atrasadas = expensas.Where(x => x.EstaAtrasada).ToList();

            if (atrasadas.Count == 0)
            {
                BrdEstadoCuenta.Background = new SolidColorBrush(Color.FromRgb(0x27, 0xAE, 0x60));
                TxtEstadoCuenta.Text = "Estado: Al Día";
                BrdAvisoAtrasadas.Visibility = Visibility.Collapsed;
                TabExpensas.Header = "Mis Expensas";
            }
            else
            {
                decimal deuda = atrasadas.Sum(x => x.MontoAPagar);

                BrdEstadoCuenta.Background = new SolidColorBrush(Color.FromRgb(0xE7, 0x4C, 0x3C));
                TxtEstadoCuenta.Text = atrasadas.Count == 1
                    ? "Estado: 1 expensa atrasada"
                    : $"Estado: {atrasadas.Count} expensas atrasadas";

                TxtAvisoAtrasadas.Text =
                    $"Tenés {atrasadas.Count} expensa(s) atrasada(s) por un total de " +
                    $"{deuda.ToString("C2", Cultura)}. Acercate a la administración para regularizar el pago.";
                BrdAvisoAtrasadas.Visibility = Visibility.Visible;

                TabExpensas.Header = $"Mis Expensas ({atrasadas.Count} atrasada(s))";
            }

            var ultima = expensas.FirstOrDefault();

            if (ultima == null)
            {
                TxtUltimaExpensaMonto.Text = "-";
                TxtUltimaExpensaDetalle.Text = "Sin expensas liquidadas";
                return;
            }

            TxtUltimaExpensaMonto.Text = ultima.MontoAPagar.ToString("C2", Cultura);
            TxtUltimaExpensaDetalle.Text = $"Período {ultima.Periodo} · Vence {ultima.FechaVencimiento:dd/MM/yyyy} ({ultima.Situacion})";

            TxtUltimaExpensaDetalle.Foreground =
                ultima.EstaPagada ? new SolidColorBrush(Color.FromRgb(0x27, 0xAE, 0x60)) :
                ultima.EstaAtrasada ? new SolidColorBrush(Color.FromRgb(0xE7, 0x4C, 0x3C)) :
                                      new SolidColorBrush(Color.FromRgb(0xF0, 0xAD, 0x4E));
        }

        private void BtnDescargarPdf_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not ExpensaPropietarioItem item)
                return;

            try
            {
                Expensa? expensa = _expensasNegocio.ObtenerPorPeriodo(item.Periodo);

                if (expensa == null)
                {
                    MessageBox.Show("No se encontró la liquidación de ese período.",
                        "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var dialogo = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Guardar expensa en PDF",
                    Filter = "Archivo PDF (*.pdf)|*.pdf",
                    FileName = $"Expensa_{item.Periodo.Replace('/', '-')}.pdf"
                };

                if (dialogo.ShowDialog() != true) return;

                var gastos = _gastoNegocio.ObtenerGastosPorPeriodo(item.Periodo).ToList();
                int cantidadDeptos = _expensasNegocio.ObtenerDetalles(expensa.Id_Expensa).Count;

                new LiquidacionPdfService().Generar(
                    dialogo.FileName,
                    expensa,
                    new List<HistorialExpensa> { item.Origen },
                    gastos,
                    cantidadDeptos,
                    LiquidacionPdfService.ObtenerNombreEdificio());

                var abrir = MessageBox.Show("El PDF se generó correctamente. ¿Querés abrirlo?",
                    "Expensa descargada", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (abrir == MessageBoxResult.Yes)
                {
                    Process.Start(new ProcessStartInfo(dialogo.FileName) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar el PDF:\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ============================================================
        // MIS RECLAMOS
        // ============================================================

        private void CargarMisReclamos()
        {
            try
            {
                if (_info == null)
                {
                    DgMisReclamos.ItemsSource = null;
                    TabReclamos.Header = "Mis Reclamos";
                    return;
                }

                DataTable dt = _reclamoNegocio
                    .ObtenerReclamosPorPropietario(_info.Propietario.IdPropietario);

                DgMisReclamos.ItemsSource = dt.DefaultView;

                int abiertos = dt.AsEnumerable().Count(r =>
                    !string.Equals(r["Estado"]?.ToString(), "Resuelto", StringComparison.OrdinalIgnoreCase));

                TabReclamos.Header = abiertos > 0 ? $"Mis Reclamos ({abiertos} abierto(s))" : "Mis Reclamos";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudieron cargar tus reclamos.\n\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnNuevoReclamo_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new ReclamoInquilinoView(_idUsuario) { Owner = this };
            ventana.ShowDialog();
            CargarMisReclamos();
        }

        // ============================================================
        // MIS RESERVAS (Real)
        // ============================================================

        private void CargarMisReservasReales()
        {
            try
            {
                if (_info == null || _info.Propietario == null)
                {
                    DgMisReservas.ItemsSource = null;
                    return;
                }

                // Busca las reservas reales. Si hay 0, la grilla simplemente quedará en blanco sin dar errores.
                DataTable dt = _reservaNegocio.ObtenerReservasPorPropietario(_info.Propietario.IdPropietario);
                DgMisReservas.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudieron cargar tus reservas.\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}