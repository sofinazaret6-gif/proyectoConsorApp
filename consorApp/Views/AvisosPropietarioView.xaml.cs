using System;
using System.Windows;
using System.Linq;
using consorApp.Seguridad;

using ConsorApp.Negocio;

namespace consorApp.Views
{
    /// <summary>
    /// Lógica de interacción para AvisosPropietarioView.xaml
    /// </summary>
    public partial class AvisosPropietarioView : Window
    {
        private readonly AvisoNegocio _avisoNegocio;

        public AvisosPropietarioView()
        {
            InitializeComponent();

            _avisoNegocio = new AvisoNegocio();

            CargarAvisos();
        }


        // =========================================================
        // CARGAR AVISOS DEL PROPIETARIO
        // =========================================================

        private void CargarAvisos()
        {
            try
            {
                // Verificamos que exista una sesión iniciada
                if (SesionUsuario.IdUsuario <= 0)
                {
                    MessageBox.Show(
                        "No se encontró un usuario con sesión iniciada.",
                        "Sesión",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }


                // Obtenemos solamente los avisos
                // correspondientes a este usuario.
                var avisos =
                      _avisoNegocio
                           .ObtenerAvisosParaPropietario(SesionUsuario.IdUsuario)
                             .Where(a => a.estado == 1)
                            .ToList();

                // Mostramos los avisos
                ItemsAvisos.ItemsSource = avisos;


                // Si no hay avisos, mostramos el mensaje.
                if (avisos.Count == 0)
                {
                    TxtSinAvisos.Visibility =
                        Visibility.Visible;
                }
                else
                {
                    TxtSinAvisos.Visibility =
                        Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar los avisos: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}