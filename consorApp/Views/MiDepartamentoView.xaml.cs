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
using System.Windows.Controls;

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
        // Una expensa, reserva o reclamo es "reciente" si es de los últimos N días en adelante
        private const int DiasRecientes = 30;

        private List<ExpensaPropietarioItem> _todasLasExpensas = new List<ExpensaPropietarioItem>();
        private DataView? _vistaReservas;
        private DataView? _vistaReclamos;

        // Evita que los filtros se disparen mientras se arma la ventana
        private bool _filtrosListos = false;
        private readonly int _idUsuario;
        private MiDepartamentoInfo? _info;

        public MiDepartamentoView(int idUsuario)
        {
            InitializeComponent();

            _idUsuario = idUsuario;

            CargarDatosPersonales();
            CargarDatosDepartamento();
            CargarMisExpensas();
            CargarMisReclamos();
            CargarMisReservasReales();

            // Ayuda al pasar el mouse sobre los filtros
            string ayuda = $"Recientes: de los últimos {DiasRecientes} días en adelante.\n" +
                           "Antiguas: anteriores a esa fecha.";
            CmbFiltroExpensas.ToolTip = ayuda;
            CmbFiltroReservas.ToolTip = ayuda;
            CmbFiltroReclamos.ToolTip = ayuda;

            _filtrosListos = true;     // <-- NUEVO
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
                _todasLasExpensas = _historialNegocio
                    .ObtenerHistorialPorUsuario(_idUsuario)
                    .Select(h => new ExpensaPropietarioItem(h))
                    .ToList();

                // El estado de cuenta se calcula siempre con TODAS las expensas
                ActualizarEstadoCuenta(_todasLasExpensas.OrderByDescending(x => x.FechaVencimiento).ToList());

                AplicarFiltroExpensas();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudieron cargar tus expensas.\n\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AplicarFiltroExpensas()
        {
            DateTime corte = DateTime.Today.AddDays(-DiasRecientes);

            IEnumerable<ExpensaPropietarioItem> lista;

            switch (CmbFiltroExpensas.SelectedIndex)
            {
                case 1:   // Recientes
                    lista = _todasLasExpensas
                        .Where(x => x.FechaVencimiento.Date >= corte)
                        .OrderByDescending(x => x.FechaVencimiento);
                    break;

                case 2:   // Antiguas
                    lista = _todasLasExpensas
                        .Where(x => x.FechaVencimiento.Date < corte)
                        .OrderBy(x => x.FechaVencimiento);
                    break;

                default:  // Todas
                    lista = _todasLasExpensas
                        .OrderByDescending(x => x.FechaVencimiento);
                    break;
            }

            var resultado = lista.ToList();

            DgMisExpensas.ItemsSource = resultado;
            LblCantidadExpensas.Text = $"Mostrando {resultado.Count} de {_todasLasExpensas.Count}";
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
                    _vistaReclamos = null;
                    DgMisReclamos.ItemsSource = null;
                    LblCantidadReclamos.Text = string.Empty;
                    TabReclamos.Header = "Mis Reclamos";
                    return;
                }

                DataTable dt = _reclamoNegocio
                    .ObtenerReclamosPorPropietario(_info.Propietario.IdPropietario);

                AgregarColumnaFecha(dt, "Fecha");

                _vistaReclamos = dt.DefaultView;
                AplicarFiltroReclamos();

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
                    _vistaReservas = null;
                    DgMisReservas.ItemsSource = null;
                    LblCantidadReservas.Text = string.Empty;
                    return;
                }

                DataTable dt = _reservaNegocio.ObtenerReservasPorPropietario(_info.Propietario.IdPropietario);

                AgregarColumnaFecha(dt, "Fecha");

                _vistaReservas = dt.DefaultView;
                AplicarFiltroReservas();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudieron cargar tus reservas.\n\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ============================================================
        // FILTROS DE FECHA (RESERVAS Y RECLAMOS)
        // ============================================================

        // Las fechas de estas tablas llegan como texto (dd/MM/yyyy).
        // Se agrega una columna oculta con la fecha real para poder filtrar y ordenar bien.
        private static void AgregarColumnaFecha(DataTable dt, string columnaOrigen)
        {
            if (!dt.Columns.Contains("FechaOrden"))
                dt.Columns.Add("FechaOrden", typeof(DateTime));

            if (!dt.Columns.Contains(columnaOrigen))
                return;

            foreach (DataRow fila in dt.Rows)
            {
                object valor = fila[columnaOrigen];

                if (valor is DateTime fecha)
                    fila["FechaOrden"] = fecha;
                else if (DateTime.TryParse(valor?.ToString(), Cultura, DateTimeStyles.None, out DateTime parseada))
                    fila["FechaOrden"] = parseada;
                else
                    fila["FechaOrden"] = DBNull.Value;
            }
        }

        private void AplicarFiltroFecha(DataView? vista, ComboBox combo, DataGrid grilla, TextBlock lblCantidad)
        {
            if (vista == null) return;

            string corte = DateTime.Today
                .AddDays(-DiasRecientes)
                .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);

            switch (combo.SelectedIndex)
            {
                case 1:   // Recientes
                    vista.RowFilter = $"FechaOrden >= #{corte}#";
                    vista.Sort = "FechaOrden DESC";
                    break;

                case 2:   // Antiguas
                    vista.RowFilter = $"FechaOrden < #{corte}#";
                    vista.Sort = "FechaOrden ASC";
                    break;

                default:  // Todas
                    vista.RowFilter = string.Empty;
                    vista.Sort = "FechaOrden DESC";
                    break;
            }

            grilla.ItemsSource = vista;
            lblCantidad.Text = $"Mostrando {vista.Count} de {vista.Table?.Rows.Count ?? 0}";
        }

        private void AplicarFiltroReservas() =>
            AplicarFiltroFecha(_vistaReservas, CmbFiltroReservas, DgMisReservas, LblCantidadReservas);

        private void AplicarFiltroReclamos() =>
            AplicarFiltroFecha(_vistaReclamos, CmbFiltroReclamos, DgMisReclamos, LblCantidadReclamos);

        private void FiltroExpensas_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (!_filtrosListos) return;
            AplicarFiltroExpensas();
        }

        private void FiltroReservas_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (!_filtrosListos) return;
            AplicarFiltroReservas();
        }

        private void FiltroReclamos_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (!_filtrosListos) return;
            AplicarFiltroReclamos();
        }
    }
}