using System;
using System.ComponentModel;
using System.Data;
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
        private readonly EspacioComunNegocio _espacioNegocio = new EspacioComunNegocio();

        // Lista en memoria para manejar los espacios comunes antes de guardarlos
        private BindingList<EspacioComun> _listaEspacios = new BindingList<EspacioComun>();
        private int _idEdificioActual = 0;

        public EdificioView()
        {
            InitializeComponent();

            // Vinculamos la grilla a la lista en memoria
            DgEspaciosComunes.ItemsSource = _listaEspacios;

            CargarEdificioYEspacios();
        }

        // Carga los datos del edificio y sus espacios comunes asociados desde la BD
        private void CargarEdificioYEspacios()
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

                    // Cargamos los espacios comunes existentes vinculados a este edificio
                    var espaciosBD = _espacioNegocio.ObtenerEspaciosComunes();
                    _listaEspacios.Clear();

                    foreach (DataRow row in espaciosBD.Rows)
                    {
                        // En la vista de administración cargamos TODOS (Activos e Inactivos)
                        _listaEspacios.Add(new EspacioComun
                        {
                            IdEspacioComun = Convert.ToInt32(row["IdEspacioComun"]),
                            Nombre = row["Nombre"]?.ToString() ?? string.Empty,
                            Descripcion = row["Descripcion"] != DBNull.Value ? row["Descripcion"]?.ToString() ?? string.Empty : string.Empty,
                            Capacidad = Convert.ToInt32(row["Capacidad"]),
                            Estado = row["Estado"]?.ToString() ?? "Activo"
                        });
                    }
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

        // Botón para agregar un espacio común a la lista temporal
        private void BtnAgregarEspacio_Click(object sender, RoutedEventArgs e)
        {
            string nombre = TxtNombreEspacio.Text.Trim();
            if (string.IsNullOrEmpty(nombre))
            {
                MessageBox.Show("Debe ingresar el nombre del espacio común.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(TxtCapacidadEspacio.Text, out int capacidad) || capacidad <= 0)
            {
                MessageBox.Show("Ingrese una capacidad válida en números.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Creamos el objeto espacio común temporal
            EspacioComun nuevoEspacio = new EspacioComun
            {
                Nombre = nombre,
                Capacidad = capacidad,
                Estado = "Activo"
            };

            _listaEspacios.Add(nuevoEspacio);

            // Limpiamos los campos de entrada del espacio
            TxtNombreEspacio.Clear();
            TxtCapacidadEspacio.Clear();
        }

        // Botón para cambiar estado o quitar un espacio
        private void BtnQuitarEspacio_Click(object sender, RoutedEventArgs e)
        {
            if (DgEspaciosComunes.SelectedItem is EspacioComun espacioSeleccionado)
            {
                if (espacioSeleccionado.IdEspacioComun == 0)
                {
                    // Es un espacio nuevo que no se guardó, lo eliminamos de la memoria
                    _listaEspacios.Remove(espacioSeleccionado);
                }
                else
                {
                    // Si ya existe en BD, alternamos su estado
                    espacioSeleccionado.Estado = espacioSeleccionado.Estado == "Activo" ? "Inactivo" : "Activo";
                    DgEspaciosComunes.Items.Refresh(); // Refresca la grilla para ver el cambio

                    MessageBox.Show($"El espacio ha cambiado a estado: {espacioSeleccionado.Estado}.\nPresioná 'Guardar' para aplicar los cambios en el sistema.", "Cambio de Estado", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Seleccione un espacio de la tabla para quitar o cambiar su estado.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Guardar o actualizar el edificio junto con sus espacios comunes
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

                // 1. Guardamos el edificio en la BD
                _edificioNegocio.GuardarEdificio(edificio);

                if (_idEdificioActual <= 0)
                {
                    EDIFICIO edificioRecuperado = _edificioNegocio.ObtenerUnicoEdificio();
                    if (edificioRecuperado != null)
                    {
                        _idEdificioActual = edificioRecuperado.id_edificio;
                    }
                }

                // 2. Guardamos o actualizamos cada espacio común
                foreach (var esp in _listaEspacios)
                {
                    if (esp.IdEspacioComun == 0)
                    {
                        // Nuevo espacio
                        _espacioNegocio.GuardarEspacioComun(_idEdificioActual, esp);
                    }
                    else
                    {
                        // Espacio existente (para guardar el cambio a Inactivo/Activo o modificaciones)
                        _espacioNegocio.ActualizarEspacioComun(esp);
                    }
                }

                MessageBox.Show(
                    "Edificio y espacios guardados correctamente.",
                    "Operación Exitosa",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                CargarEdificioYEspacios();
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

        // Permitir únicamente números en los campos correspondientes
        private void SoloNumeros_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}