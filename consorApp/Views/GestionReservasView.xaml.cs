using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using ConsorApp.Negocio;

namespace ConsorApp.Views
{
    public partial class GestionReservasAdminView : Window
    {
        private readonly ReservaEspacioNegocio _reservaNegocio = new ReservaEspacioNegocio();
        private readonly EspacioComunNegocio _espacioComunNegocio = new EspacioComunNegocio();
        private readonly BloqueoEspacioNegocio _bloqueoNegocio = new BloqueoEspacioNegocio();

        // Variable para almacenar la tabla original y filtrar sobre ella
        private DataTable? _dtReservasOriginal = null;
        private int? idBloqueoSeleccionado = null;

        public GestionReservasAdminView()
        {
            InitializeComponent();
            CargarDatosIniciales();
        }

        private void CargarDatosIniciales()
        {
            // Actualizamos automáticamente a "Finalizada" las reservas vencidas antes de cargar
            try
            {
                _reservaNegocio.ActualizarReservasFinalizadas();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualizar reservas finalizadas: " + ex.Message, "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            CargarReservasReales();
            CargarBloqueosReales();
            CargarIncidentesReales();
            CargarReservasFinalizadasReales(); // Carga la grilla superior de la pestaña de incidentes
            CargarCombosEspacios();
        }

        // Carga las reservas y guarda la tabla original (Pestaña 1)
        private void CargarReservasReales()
        {
            try
            {
                _dtReservasOriginal = _reservaNegocio.ObtenerReservasAdmin();
                if (_dtReservasOriginal != null)
                {
                    DgReservasAdmin.ItemsSource = _dtReservasOriginal.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar las reservas: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Lógica para filtrar las reservas por espacio y fecha combinados
        private void AplicarFiltros()
        {
            if (_dtReservasOriginal == null) return;

            try
            {
                DataView dv = _dtReservasOriginal.DefaultView;
                string filtro = "";

                // Filtro por Espacio Común
                if (CmbFiltroEspacio.SelectedItem is DataRowView espacioRow)
                {
                    string nombreEspacio = espacioRow["Nombre"]?.ToString() ?? string.Empty;
                    if (!string.IsNullOrEmpty(nombreEspacio))
                    {
                        filtro += $"Espacio = '{nombreEspacio}'";
                    }
                }

                // Filtro por Fecha
                if (DpFiltroFecha.SelectedDate.HasValue)
                {
                    string fechaStr = DpFiltroFecha.SelectedDate.Value.ToString("dd/MM/yyyy");
                    if (!string.IsNullOrEmpty(filtro))
                    {
                        filtro += " AND ";
                    }
                    filtro += $"Fecha = '{fechaStr}'";
                }

                dv.RowFilter = filtro;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al filtrar: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Eventos de los filtros
        private void CmbFiltroEspacio_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AplicarFiltros();
        }

        private void DpFiltroFecha_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            AplicarFiltros();
        }

        // Botón para limpiar filtros y ver todo nuevamente
        private void BtnLimpiarFiltros_Click(object sender, RoutedEventArgs e)
        {
            CmbFiltroEspacio.SelectedIndex = -1;
            DpFiltroFecha.SelectedDate = null;

            if (_dtReservasOriginal != null)
            {
                _dtReservasOriginal.DefaultView.RowFilter = string.Empty;
            }
        }

        // Carga los bloqueos existentes
        private void CargarBloqueosReales()
        {
            try
            {
                DataTable dtBloqueos = _bloqueoNegocio.ObtenerBloqueosAdmin();
                if (dtBloqueos != null)
                {
                    DgBloqueosAdmin.ItemsSource = dtBloqueos.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los bloqueos: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Carga los incidentes registrados
        private void CargarIncidentesReales()
        {
            try
            {
                DataTable dtIncidentes = _reservaNegocio.ObtenerIncidentesAdmin();
                if (dtIncidentes != null)
                {
                    DgIncidentesAdmin.ItemsSource = dtIncidentes.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los incidentes: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Carga únicamente las reservas con estado "Finalizada" para la Pestaña 3
        private void CargarReservasFinalizadasReales()
        {
            try
            {
                DataTable dtFinalizadas = _reservaNegocio.ObtenerReservasFinalizadas();
                if (dtFinalizadas != null)
                {
                    DgReservasFinalizadas.ItemsSource = dtFinalizadas.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar las reservas finalizadas: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Carga los espacios en los ComboBox
        private void CargarCombosEspacios()
        {
            try
            {
                DataTable dtEspacios = _espacioComunNegocio.ObtenerEspaciosComunes();
                if (dtEspacios != null)
                {
                    CmbFiltroEspacio.ItemsSource = dtEspacios.DefaultView;
                    CmbFiltroEspacio.DisplayMemberPath = "Nombre";
                    CmbFiltroEspacio.SelectedValuePath = "IdEspacioComun";

                    CmbEspacioBloqueo.ItemsSource = dtEspacios.DefaultView;
                    CmbEspacioBloqueo.DisplayMemberPath = "Nombre";
                    CmbEspacioBloqueo.SelectedValuePath = "IdEspacioComun";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los espacios: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBloquearEspacio_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ValidarFormularioBloqueo(out int idEspacio, out DateTime desde, out DateTime hasta, out string motivo);
                _bloqueoNegocio.RegistrarBloqueo(idEspacio, desde, hasta, motivo);

                MessageBox.Show("Espacio bloqueado correctamente por mantenimiento.", "Operación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                LimpiarFormularioBloqueo();
                CargarBloqueosReales();
                CargarReservasReales();
                CargarReservasFinalizadasReales();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Validación de Bloqueo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DgBloqueosAdmin_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgBloqueosAdmin.SelectedItem is DataRowView fila)
            {
                idBloqueoSeleccionado = Convert.ToInt32(fila["IdBloqueo"]);
                CmbEspacioBloqueo.SelectedValue = fila["IdEspacioComun"];
                DpFechaDesde.SelectedDate = Convert.ToDateTime(fila["FechaDesde"]);
                DpFechaHasta.SelectedDate = Convert.ToDateTime(fila["FechaHasta"]);
                TxtMotivoBloqueo.Text = fila["Motivo"].ToString();
            }
        }

        private void BtnModificarBloqueo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!idBloqueoSeleccionado.HasValue)
                {
                    MessageBox.Show("Debe seleccionar un bloqueo de la tabla para modificar.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                ValidarFormularioBloqueo(out int idEspacio, out DateTime desde, out DateTime hasta, out string motivo);
                _bloqueoNegocio.ModificarBloqueo(idBloqueoSeleccionado.Value, idEspacio, desde, hasta, motivo);

                MessageBox.Show("El bloqueo ha sido actualizado exitosamente.", "Operación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                LimpiarFormularioBloqueo();
                CargarBloqueosReales();
                CargarReservasReales();
                CargarReservasFinalizadasReales();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Validación de Modificación", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnEliminarBloqueo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DgBloqueosAdmin.SelectedItem == null)
                {
                    MessageBox.Show("Debe seleccionar un bloqueo de la tabla para eliminar.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DataRowView fila = (DataRowView)DgBloqueosAdmin.SelectedItem;
                int idBloqueo = Convert.ToInt32(fila["IdBloqueo"]);

                MessageBoxResult resultado = MessageBox.Show(
                    "¿Está seguro de que desea eliminar este bloqueo de mantenimiento?",
                    "Confirmación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (resultado == MessageBoxResult.Yes)
                {
                    _bloqueoNegocio.EliminarBloqueo(idBloqueo);
                    MessageBox.Show("El bloqueo fue eliminado correctamente.", "Operación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                    LimpiarFormularioBloqueo();
                    CargarBloqueosReales();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar el bloqueo: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancelarReserva_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DgReservasAdmin.SelectedItem == null)
                {
                    MessageBox.Show("Debe seleccionar una reserva de la tabla para cancelar.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string motivoCancelacion = TxtMotivoCancelacion.Text.Trim();
                string? motivoFinal = string.IsNullOrEmpty(motivoCancelacion) ? null : motivoCancelacion;

                DataRowView filaSeleccionada = (DataRowView)DgReservasAdmin.SelectedItem;
                int idReserva = Convert.ToInt32(filaSeleccionada["IdReserva"]);

                MessageBoxResult resultado = MessageBox.Show(
                    "¿Está seguro de que desea cancelar esta reserva?",
                    "Confirmación de Cancelación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (resultado == MessageBoxResult.Yes)
                {
                    _reservaNegocio.CancelarReserva(idReserva, motivoFinal);
                    MessageBox.Show("La reserva ha sido cancelada exitosamente.", "Operación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);

                    TxtMotivoCancelacion.Clear();
                    CargarReservasReales();
                    CargarReservasFinalizadasReales();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cancelar la reserva: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- EVENTOS DE LA PESTAÑA DE INCIDENTES ---

        private void BtnRegistrarIncidente_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DgReservasFinalizadas.SelectedItem == null)
                {
                    MessageBox.Show("Debe seleccionar una reserva finalizada de la tabla superior para asociar el incidente.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DataRowView filaReserva = (DataRowView)DgReservasFinalizadas.SelectedItem;
                int idReserva = Convert.ToInt32(filaReserva["IdReserva"]);
                string descripcion = TxtDescripcionIncidente.Text.Trim();

                _reservaNegocio.RegistrarIncidente(idReserva, descripcion);

                MessageBox.Show("El incidente o daño fue registrado correctamente.", "Operación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                TxtDescripcionIncidente.Clear();
                CargarIncidentesReales();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Validación de Incidente", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnResolverIncidente_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DgIncidentesAdmin.SelectedItem == null)
                {
                    MessageBox.Show("Debe seleccionar un incidente de la tabla para marcarlo como solucionado.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DataRowView filaIncidente = (DataRowView)DgIncidentesAdmin.SelectedItem;
                int idIncidente = Convert.ToInt32(filaIncidente["Id_Incidente"]);

                _reservaNegocio.MarcarIncidenteSolucionado(idIncidente);

                MessageBox.Show("El incidente ha sido marcado como solucionado.", "Operación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                CargarIncidentesReales();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al resolver el incidente: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ValidarFormularioBloqueo(out int idEspacio, out DateTime desde, out DateTime hasta, out string motivo)
        {
            if (CmbEspacioBloqueo.SelectedValue == null)
                throw new Exception("Debe seleccionar un espacio común.");

            if (!DpFechaDesde.SelectedDate.HasValue || !DpFechaHasta.SelectedDate.HasValue)
                throw new Exception("Debe seleccionar el rango de fechas (Desde y Hasta).");

            if (DpFechaDesde.SelectedDate.Value.Date < DateTime.Now.Date)
                throw new Exception("No se puede registrar un bloqueo de mantenimiento para una fecha pasada.");

            if (DpFechaDesde.SelectedDate.Value > DpFechaHasta.SelectedDate.Value)
                throw new Exception("La fecha 'Desde' no puede ser posterior a la fecha 'Hasta'.");

            motivo = TxtMotivoBloqueo.Text.Trim();
            if (string.IsNullOrEmpty(motivo))
                throw new Exception("Debe ingresar un motivo para el bloqueo.");

            idEspacio = Convert.ToInt32(CmbEspacioBloqueo.SelectedValue);
            desde = DpFechaDesde.SelectedDate.Value;
            hasta = DpFechaHasta.SelectedDate.Value;
        }

        private void LimpiarFormularioBloqueo()
        {
            idBloqueoSeleccionado = null;
            DpFechaDesde.SelectedDate = null;
            DpFechaHasta.SelectedDate = null;
            TxtMotivoBloqueo.Clear();
            CmbEspacioBloqueo.SelectedIndex = -1;
            DgBloqueosAdmin.UnselectAll();
        }
    }
}