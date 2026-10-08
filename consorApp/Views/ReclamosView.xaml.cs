using System;
using System.Windows;
using ConsorApp.Negocio;
using System.Linq;

namespace consorApp.Views
{
    public partial class ReclamoInquilinoView : Window
    {
        private readonly int _idUsuario;
        private readonly ReclamoNegocio _reclamoNegocio;

        public ReclamoInquilinoView(int idUsuario)
        {
            InitializeComponent();
            CmbMotivoReclamo.ItemsSource = Motivos.Categorias.Where(c => c != Motivos.Otro).ToList();
            _idUsuario = idUsuario;
            _reclamoNegocio = new ReclamoNegocio();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnEnviarReclamo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string motivo = CmbMotivoReclamo.Text.Trim();
                string ubicacion = TxtUbicacion.Text.Trim();
                string descripcion = TxtDescripcion.Text.Trim();

                // Llamamos directamente a la capa de negocio
                bool registrado = _reclamoNegocio.RegistrarReclamo(_idUsuario, motivo, ubicacion, descripcion);

                if (registrado)
                {
                    MessageBox.Show(
                        "El reclamo fue registrado correctamente.",
                        "Reclamo registrado",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Aviso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
    }
}