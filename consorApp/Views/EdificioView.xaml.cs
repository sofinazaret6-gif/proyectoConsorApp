using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using ConsorApp.Entidades;
using ConsorApp.Negocio;

namespace consorApp.Views
{
    public partial class EdificioView : Window
    {
        private readonly EdificioNegocio _edificioNegocio = new EdificioNegocio();
        private int _idEdificioActual = 0;

        public EdificioView()
        {
            InitializeComponent();
            CargarEdificio();
        }

        // Carga los datos del único edificio registrado en el sistema
        private void CargarEdificio()
        {
            try
            {
                EDIFICIO edificio = _edificioNegocio.ObtenerUnicoEdificio();

                if (edificio != null)
                {
                    _idEdificioActual = edificio.id_edificio;
                    TxtDescripcion.Text = edificio.Descripcion;
                    TxtUbicacion.Text = edificio.Ubicacion;
                    TxtCantPisos.Text = edificio.CantPisos.ToString();
                    TxtCantDepto.Text = edificio.CantDepto.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar los datos del edificio: " + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Guardar o actualizar el edificio
        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int.TryParse(TxtCantPisos.Text, out int pisos);
                int.TryParse(TxtCantDepto.Text, out int deptos);

                EDIFICIO edificio = new EDIFICIO
                {
                    id_edificio = _idEdificioActual,
                    Descripcion = TxtDescripcion.Text.Trim(),
                    Ubicacion = TxtUbicacion.Text.Trim(),
                    CantPisos = pisos,
                    CantDepto = deptos
                };

                _edificioNegocio.GuardarEdificio(edificio);

                MessageBox.Show(
                    "Edificio guardado correctamente.",
                    "Operación Exitosa",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                CargarEdificio();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Validación de Edificio",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        // Limpiar o recargar los datos originales
        private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            CargarEdificio();
        }

        // Permitir únicamente números en los campos correspondientes
        private void SoloNumeros_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}