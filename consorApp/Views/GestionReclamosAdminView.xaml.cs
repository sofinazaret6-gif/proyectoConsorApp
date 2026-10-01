using System;
using System.Data;
using System.Windows;
using ConsorApp.Negocio;

namespace consorApp.Views
{
    public partial class GestionReclamosAdminView : Window
    {
        private readonly ReclamoNegocio _reclamoNegocio;

        public GestionReclamosAdminView()
        {
            InitializeComponent();
            _reclamoNegocio = new ReclamoNegocio();

            CargarReclamosReales();

            // Asociamos los eventos de los botones
            BtnEnResolucion.Click += BtnEnResolucion_Click;
            BtnMarcarResuelto.Click += BtnMarcarResuelto_Click;
            BtnGuardarObservacion.Click += BtnGuardarObservacion_Click;
        }

        private void CargarReclamosReales()
        {
            try
            {
                DataTable dtReclamos = _reclamoNegocio.ObtenerReclamosAdmin();
                DgReclamosAdmin.ItemsSource = dtReclamos.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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