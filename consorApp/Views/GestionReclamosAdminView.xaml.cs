using ConsorApp.Negocio;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;
using System.Linq;

namespace consorApp.Views
{
    public partial class GestionReclamosAdminView : Window
    {
        private readonly ReclamoNegocio _reclamoNegocio;
        private const string TodosLosMotivos = "Todos los motivos";
        private const string TodosLosMeses = "Todos los meses";
        private const string TodosLosAnios = "Todos";
        private DataView? _vista;

        public GestionReclamosAdminView()
        {
            InitializeComponent();
            _reclamoNegocio = new ReclamoNegocio();

            CargarFiltroMotivos();
            CargarFiltroMeses();
            CargarReclamosReales();

            // Los eventos se asocian al final para que no se disparen durante la carga
            CmbFiltroMotivo.SelectionChanged += Filtro_SelectionChanged;
            CmbFiltroEstado.SelectionChanged += Filtro_SelectionChanged;
            CmbFiltroMes.SelectionChanged += Filtro_SelectionChanged;   // <-- NUEVO
            CmbFiltroAnio.SelectionChanged += Filtro_SelectionChanged;   // <-- NUEVO

            BtnEnResolucion.Click += BtnEnResolucion_Click;
            BtnMarcarResuelto.Click += BtnMarcarResuelto_Click;
            BtnGuardarObservacion.Click += BtnGuardarObservacion_Click;
        }

        private void CargarFiltroMotivos()
        {
            var items = new List<string> { TodosLosMotivos };
            items.AddRange(Motivos.Categorias);

            CmbFiltroMotivo.ItemsSource = items;
            CmbFiltroMotivo.SelectedIndex = 0;
        }

        private void CargarReclamosReales()
        {
            try
            {
                DataTable dtReclamos = _reclamoNegocio.ObtenerReclamosAdmin();
                _vista = dtReclamos.DefaultView;
                DgReclamosAdmin.ItemsSource = _vista;
                CargarFiltroAnios(dtReclamos);
                AplicarFiltros();   // mantiene los filtros activos después de actualizar
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void CargarFiltroMeses()
        {
            var meses = new List<string> { TodosLosMeses };

            meses.AddRange(
                CultureInfo.GetCultureInfo("es-AR").DateTimeFormat.MonthNames
                    .Take(12)
                    .Select(m => char.ToUpper(m[0]) + m.Substring(1)));

            CmbFiltroMes.ItemsSource = meses;
            CmbFiltroMes.SelectedIndex = DateTime.Now.Month;   // posición 1 = enero ... 12 = diciembre
        }

        private void CargarFiltroAnios(DataTable dt)
        {
            var seleccionado = CmbFiltroAnio.SelectedItem as string;

            var anios = dt.AsEnumerable()
                .Select(r => Convert.ToInt32(r["Anio"]))
                .Distinct()
                .OrderByDescending(a => a)
                .Select(a => a.ToString())
                .ToList();

            anios.Insert(0, TodosLosAnios);

            CmbFiltroAnio.ItemsSource = anios;
            CmbFiltroAnio.SelectedItem =
                seleccionado != null && anios.Contains(seleccionado) ? seleccionado : TodosLosAnios;
        }
        private void Filtro_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AplicarFiltros();
        }

        private void AplicarFiltros()
        {
            if (_vista == null) return;

            var condiciones = new List<string>();

            if (CmbFiltroMotivo.SelectedItem is string categoria && categoria != TodosLosMotivos)
                condiciones.Add($"Categoria = '{categoria.Replace("'", "''")}'");

            if (CmbFiltroEstado.SelectedItem is ComboBoxItem item
                && item.Content?.ToString() is string estado && estado != "Todos")
                condiciones.Add($"Estado = '{estado.Replace("'", "''")}'");

            _vista.RowFilter = string.Join(" AND ", condiciones);
            if (CmbFiltroMes.SelectedIndex > 0)
                condiciones.Add($"Mes = {CmbFiltroMes.SelectedIndex}");

            if (CmbFiltroAnio.SelectedItem is string anio
                && anio != TodosLosAnios
                && int.TryParse(anio, out int anioNumero))
                condiciones.Add($"Anio = {anioNumero}");
        }
        private void BtnEnResolucion_Click(object sender, RoutedEventArgs e)
        {
            ActualizarEstadoBD("En Resolución");
        }

        private void BtnMarcarResuelto_Click(object sender, RoutedEventArgs e)
        {
            ActualizarEstadoBD("Resuelto");
        }

        private void ActualizarEstadoBD(string nuevoEstado)
        {
            if (DgReclamosAdmin.SelectedItem is DataRowView filaSeleccionada)
            {
                try
                {
                    int idReclamo = Convert.ToInt32(filaSeleccionada["IdReclamo"]);
                    string observacionEscrita = TxtObservacionAdmin.Text.Trim();

                    // Llamamos a la capa de negocio (que valida la observación y actualiza SQL)
                    bool actualizado = _reclamoNegocio.ActualizarEstadoYObservacion(idReclamo, nuevoEstado, observacionEscrita);

                    if (actualizado)
                    {
                        MessageBox.Show($"El reclamo fue marcado como '{nuevoEstado}' con éxito.", "Actualización", MessageBoxButton.OK, MessageBoxImage.Information);

                        TxtObservacionAdmin.Clear();

                        // Recargamos la tabla para ver reflejados los cambios en tiempo real
                        CargarReclamosReales();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Por favor, seleccione un reclamo de la tabla.", "Selección requerida", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnGuardarObservacion_Click(object sender, RoutedEventArgs e)
        {
            if (DgReclamosAdmin.SelectedItem is DataRowView filaSeleccionada)
            {
                try
                {
                    int idReclamo = Convert.ToInt32(filaSeleccionada["IdReclamo"]);
                    string observacion = TxtObservacionAdmin.Text.Trim();

                    if (string.IsNullOrWhiteSpace(observacion))
                    {
                        MessageBox.Show(
                            "Escriba una observación antes de guardar.",
                            "Observación requerida",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        return;
                    }

                    bool actualizado = _reclamoNegocio.ActualizarObservacion(
                        idReclamo,
                        observacion
                    );

                    if (actualizado)
                    {
                        MessageBox.Show(
                            "La observación fue guardada correctamente.",
                            "Actualización",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        TxtObservacionAdmin.Clear();
                        CargarReclamosReales();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        ex.Message,
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show(
                    "Seleccione un reclamo de la tabla.",
                    "Selección requerida",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

    }
}