using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ConsorApp.Entidades;
using ConsorApp.Negocio;

namespace consorApp.Views
{
    public partial class EdificioView : Window
    {
        private readonly EdificioNegocio _edificioNegocio =
            new EdificioNegocio();

        private int? _idEdificioSeleccionado = null;

        public EdificioView()
        {
            InitializeComponent();
            CargarEdificios();
        }

        // Cargar edificios en la tabla
        private void CargarEdificios()
        {
            try
            {
                DgEdificios.ItemsSource =
                    _edificioNegocio
                    .ObtenerEdificios()
                    .DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar los edificios: "
                    + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Guardar o modificar
        private void BtnGuardar_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                int.TryParse(
                    TxtCantPisos.Text,
                    out int pisos);

                int.TryParse(
                    TxtCantDepto.Text,
                    out int deptos);

                EDIFICIO edificio = new EDIFICIO
                {
                    id_edificio =
                        _idEdificioSeleccionado ?? 0,

                    Descripcion =
                        TxtDescripcion.Text.Trim(),

                    Ubicacion =
                        TxtUbicacion.Text.Trim(),

                    CantPisos =
                        pisos,

                    CantDepto =
                        deptos
                };

                _edificioNegocio.GuardarEdificio(
                    edificio);

                MessageBox.Show(
                    "Edificio guardado correctamente.",
                    "Operación Exitosa",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                LimpiarCampos();
                CargarEdificios();
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

        // Al seleccionar un edificio de la tabla,
        // cargar sus datos en el formulario
        private void DgEdificios_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (DgEdificios.SelectedItem is DataRowView row)
            {
                _idEdificioSeleccionado =
                    Convert.ToInt32(
                        row["IdEdificio"]);

                TxtDescripcion.Text =
                    row["Descripcion"].ToString();

                TxtUbicacion.Text =
                    row["Ubicacion"].ToString();

                TxtCantPisos.Text =
                    row["CantPisos"].ToString();

                TxtCantDepto.Text =
                    row["CantDepto"].ToString();
            }
        }

        // Limpiar
        private void BtnLimpiar_Click(
            object sender,
            RoutedEventArgs e)
        {
            LimpiarCampos();
        }

        private void LimpiarCampos()
        {
            _idEdificioSeleccionado = null;

            TxtDescripcion.Clear();
            TxtUbicacion.Clear();
            TxtCantPisos.Clear();
            TxtCantDepto.Clear();

            DgEdificios.UnselectAll();
        }

        // Permitir únicamente números
        private void SoloNumeros_PreviewTextInput(
            object sender,
            TextCompositionEventArgs e)
        {
            Regex regex =
                new Regex("[^0-9]+");

            e.Handled =
                regex.IsMatch(e.Text);
        }
    }
}