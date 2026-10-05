using System;
using System.Data;
using System.Windows;
using ConsorApp.Negocio;

namespace ConsorApp.Views
{
    public partial class GestionReservasAdminView : Window
    {
        private readonly ReservaEspacioNegocio _reservaNegocio = new ReservaEspacioNegocio();
        private readonly EspacioComunNegocio _espacioComunNegocio = new EspacioComunNegocio();
        private readonly BloqueoEspacioNegocio _bloqueoNegocio = new BloqueoEspacioNegocio();

        public GestionReservasAdminView()
        {
            InitializeComponent();
            CargarDatosIniciales();
        }

        private void CargarDatosIniciales()
        {
            CargarReservasReales();
            CargarCombosEspacios();
        }

        // Carga las reservas reales desde la base de datos de forma segura
        private void CargarReservasReales()
        {
            try
            {
                DataTable dtReservas = _reservaNegocio.ObtenerReservasAdmin();
                if (dtReservas != null)
                {
                    DgReservasAdmin.ItemsSource = dtReservas.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar las reservas: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Carga los espacios comunes en los ComboBox (Filtro y Bloqueo)
        private void CargarCombosEspacios()
        {
            try
            {
                DataTable dtEspacios = _espacioComunNegocio.ObtenerEspaciosComunes();
                if (dtEspacios != null)
                {
                    // 1. Combo de Filtro superior
                    CmbFiltroEspacio.ItemsSource = dtEspacios.DefaultView;
                    CmbFiltroEspacio.DisplayMemberPath = "Nombre";
                    CmbFiltroEspacio.SelectedValuePath = "IdEspacioComun";

                    // 2. Combo de Bloqueo de espacios
                    CmbEspacioBloqueo.ItemsSource = dtEspacios.DefaultView;
                    CmbEspacioBloqueo.DisplayMemberPath = "Nombre";
                    CmbEspacioBloqueo.SelectedValuePath = "IdEspacioComun";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los espacios en los selectores: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Evento del botón para aplicar bloqueo por mantenimiento
        private void BtnBloquearEspacio_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CmbEspacioBloqueo.SelectedValue == null)
                {
                    MessageBox.Show("Debe seleccionar un espacio común para bloquear.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!DpFechaDesde.SelectedDate.HasValue || !DpFechaHasta.SelectedDate.HasValue)
                {
                    MessageBox.Show("Debe seleccionar el rango de fechas (Desde y Hasta) para el bloqueo.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (DpFechaDesde.SelectedDate.Value > DpFechaHasta.SelectedDate.Value)
                {
                    MessageBox.Show("La fecha 'Desde' no puede ser posterior a la fecha 'Hasta'.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int idEspacio = Convert.ToInt32(CmbEspacioBloqueo.SelectedValue);
                DateTime desde = DpFechaDesde.SelectedDate.Value;
                DateTime hasta = DpFechaHasta.SelectedDate.Value;
                string motivo = TxtMotivoBloqueo.Text.Trim();

                if (string.IsNullOrEmpty(motivo))
                {
                    MessageBox.Show("Debe ingresar un motivo para el bloqueo por mantenimiento.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Llamamos a la capa de negocio para registrar el bloqueo
                _bloqueoNegocio.RegistrarBloqueo(idEspacio, desde, hasta, motivo);

                MessageBox.Show("Espacio bloqueado correctamente por mantenimiento.", "Operación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);

                // Limpiar campos de bloqueo y recargar la grilla
                DpFechaDesde.SelectedDate = null;
                DpFechaHasta.SelectedDate = null;
                TxtMotivoBloqueo.Clear();
                CmbEspacioBloqueo.SelectedIndex = -1;
                CargarReservasReales();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Validación de Bloqueo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Evento del botón para cancelar la reserva seleccionada en la grilla
        private void BtnCancelarReserva_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DgReservasAdmin.SelectedItem == null)
                {
                    MessageBox.Show("Debe seleccionar una reserva de la tabla para cancelar.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DataRowView filaSeleccionada = (DataRowView)DgReservasAdmin.SelectedItem;
                int idReserva = Convert.ToInt32(filaSeleccionada["IdReserva"]);

                MessageBoxResult resultado = MessageBox.Show(
                    "¿Está seguro de que desea cancelar esta reserva?",
                    "Confirmación de Cancelación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (resultado == MessageBoxResult.Yes)
                {
                    _reservaNegocio.CancelarReserva(idReserva);
                    MessageBox.Show("La reserva ha sido cancelada exitosamente.", "Operación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Recargamos la grilla para reflejar el cambio
                    CargarReservasReales();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cancelar la reserva: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}