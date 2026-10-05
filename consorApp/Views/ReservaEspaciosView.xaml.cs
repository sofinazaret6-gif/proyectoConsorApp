using System;
using System.Data;
using System.Windows;
using System.Windows.Input;
using ConsorApp.Negocio;

namespace ConsorApp.Views
{
    public partial class ReservaEspaciosView : Window
    {
        private readonly EspacioComunNegocio _espacioNegocio = new EspacioComunNegocio();
        private readonly ReservaEspacioNegocio _reservaNegocio = new ReservaEspacioNegocio();

        private int _idEspacioSeleccionado = 0;
        private int _idPropietarioActual = 1; // ID temporal del propietario logueado

        public ReservaEspaciosView()
        {
            InitializeComponent();
            CargarEspaciosComunes();
            CargarTurnos();
        }

        // Carga los espacios comunes para las tarjetas del panel derecho
        private void CargarEspaciosComunes()
        {
            try
            {
                DataTable dtEspacios = _espacioNegocio.ObtenerEspaciosComunes();
                if (dtEspacios != null)
                {
                    IcEspaciosComunes.ItemsSource = dtEspacios.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los espacios comunes: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Carga los turnos reales desde la base de datos hacia el ComboBox
        private void CargarTurnos()
        {
            try
            {
                DataTable dtTurnos = _reservaNegocio.ObtenerTurnos();
                if (dtTurnos != null)
                {
                    CmbTurnos.ItemsSource = dtTurnos.DefaultView;
                    CmbTurnos.DisplayMemberPath = "TextoTurno";
                    CmbTurnos.SelectedValuePath = "IdTurno";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los turnos: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Evento al hacer clic en una tarjeta de espacio común
        private void TarjetaEspacio_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender is FrameworkElement border && border.DataContext is DataRowView fila)
                {
                    if (fila.Row.Table.Columns.Contains("Nombre") && fila["Nombre"] != DBNull.Value)
                    {
                        TxtEspacioSeleccionado.Text = fila["Nombre"].ToString();
                    }

                    if (fila.Row.Table.Columns.Contains("IdEspacioComun") && fila["IdEspacioComun"] != DBNull.Value)
                    {
                        _idEspacioSeleccionado = Convert.ToInt32(fila["IdEspacioComun"]);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al seleccionar el espacio: " + ex.Message, "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Botón Limpiar
        private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            TxtEspacioSeleccionado.Clear();
            DpFechaReserva.SelectedDate = null;
            CmbTurnos.SelectedIndex = -1;
            _idEspacioSeleccionado = 0;
        }

        // Botón Guardar Reserva
        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_idEspacioSeleccionado <= 0)
                {
                    MessageBox.Show("Debe seleccionar un espacio común haciendo clic en una de las tarjetas.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!DpFechaReserva.SelectedDate.HasValue)
                {
                    MessageBox.Show("Debe seleccionar una fecha para la reserva.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (CmbTurnos.SelectedItem == null)
                {
                    MessageBox.Show("Debe seleccionar un turno u horario.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Extracción segura del ID del turno para evitar errores de conversión con DataRowView
                int idTurno = 0;
                if (CmbTurnos.SelectedItem is DataRowView rowTurno && rowTurno.Row.Table.Columns.Contains("IdTurno"))
                {
                    idTurno = Convert.ToInt32(rowTurno["IdTurno"]);
                }
                else if (CmbTurnos.SelectedValue != null)
                {
                    int.TryParse(CmbTurnos.SelectedValue.ToString(), out idTurno);
                }

                if (idTurno <= 0)
                {
                    MessageBox.Show("No se pudo obtener un identificador válido para el turno seleccionado.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Generamos código de seguimiento único
                string codigoSeguimiento = "RES-" + Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
                DateTime fechaReserva = DpFechaReserva.SelectedDate.Value;

                // Guardamos en la base de datos
                _reservaNegocio.GuardarReserva(_idEspacioSeleccionado, _idPropietarioActual, idTurno, fechaReserva, codigoSeguimiento);

                MessageBox.Show($"¡Reserva registrada con éxito!\nCódigo de seguimiento: {codigoSeguimiento}", "Operación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);

                BtnLimpiar_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar la reserva: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}