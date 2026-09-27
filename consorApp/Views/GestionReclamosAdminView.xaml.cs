using System;
using System.Data;
using System.Windows;

namespace consorApp.Views
{
    public partial class GestionReclamosAdminView : Window
    {
        public GestionReclamosAdminView()
        {
            InitializeComponent();
            CargarReclamosEjemplo();

            // Asociamos los eventos de los botones
            BtnEnResolucion.Click += BtnEnResolucion_Click;
            BtnMarcarResuelto.Click += BtnMarcarResuelto_Click;
        }

        private void CargarReclamosEjemplo()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("IdReclamo", typeof(int));
            dt.Columns.Add("Motivo", typeof(string));
            dt.Columns.Add("Ubicacion", typeof(string));
            dt.Columns.Add("Responsable", typeof(string));
            dt.Columns.Add("Fecha", typeof(string));
            dt.Columns.Add("Estado", typeof(string));
            dt.Columns.Add("Observacion", typeof(string));

            // Datos de prueba con observaciones iniciales
            dt.Rows.Add(1, "Ruidos molestos", "Departamento 2B", "Carlos Gómez", "24/09/2026", "Pendiente", "Sin observaciones aún.");
            dt.Rows.Add(2, "Gotera / Filtración", "Palier Piso 3", "María Pérez", "23/09/2026", "En Resolución", "El plomero pasará mañana.");

            DgReclamosAdmin.ItemsSource = dt.DefaultView;
        }

        private void BtnEnResolucion_Click(object sender, RoutedEventArgs e)
        {
            ActualizarEstadoYObservacion("En Resolución");
        }

        private void BtnMarcarResuelto_Click(object sender, RoutedEventArgs e)
        {
            ActualizarEstadoYObservacion("Resuelto");
        }

        private void ActualizarEstadoYObservacion(string nuevoEstado)
        {
            if (DgReclamosAdmin.SelectedItem is DataRowView filaSeleccionada)
            {
                string observacionEscrita = TxtObservacionAdmin.Text.Trim();

                if (string.IsNullOrEmpty(observacionEscrita))
                {
                    MessageBox.Show("Por favor, escriba una observación o motivo para este cambio de estado.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Actualizamos los valores en la fila seleccionada
                filaSeleccionada["Estado"] = nuevoEstado;
                filaSeleccionada["Observacion"] = observacionEscrita;
                filaSeleccionada.EndEdit();

                MessageBox.Show($"El reclamo fue marcado como '{nuevoEstado}' con éxito.", "Actualización", MessageBoxButton.OK, MessageBoxImage.Information);

                // Limpiamos la caja de texto después de guardar
                TxtObservacionAdmin.Clear();
            }
            else
            {
                MessageBox.Show("Por favor, seleccione un reclamo de la tabla.", "Selección requerida", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}