using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ConsorApp.Entidades;
using ConsorApp.Negocio;

namespace consorApp.Views
{
    /// <summary>
    /// Lógica de interacción para la vista de gestión de Departamentos
    /// en formato de Tarjetas.
    /// </summary>
    public partial class DepartamentoView : Window
    {
        private readonly DepartamentoNegocio _deptoNegocio =
            new DepartamentoNegocio();

        private readonly EdificioNegocio _edificioNegocio =
            new EdificioNegocio();

        // Guarda el ID del departamento seleccionado para edición.
        // Si es null, se crea uno nuevo.
        private int? _idDepartamentoSeleccionado = null;

        // Guarda todos los departamentos obtenidos de la base.
        // Se utiliza para realizar los filtros.
        private DataTable? _departamentos;


        public DepartamentoView()
        {
            InitializeComponent();

            CargarDesplegableEdificios();

            CargarFiltroEdificios();

            CargarDepartamentos();
        }


        // =========================================================
        // CARGAR EDIFICIOS PARA EL FORMULARIO
        // =========================================================

        /// <summary>
        /// Llena el ComboBox de Edificios del formulario.
        /// </summary>
        private void CargarDesplegableEdificios()
        {
            try
            {
                DataTable dtEdificios =
                    _edificioNegocio.ObtenerEdificios();

                CmbEdificio.ItemsSource =
                    dtEdificios.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar la lista de edificios: "
                    + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // CARGAR EDIFICIOS PARA EL FILTRO
        // =========================================================

        /// <summary>
        /// Llena el ComboBox utilizado para filtrar
        /// los departamentos por edificio.
        /// </summary>
        private void CargarFiltroEdificios()
        {
            try
            {
                DataTable dtEdificios =
                    _edificioNegocio.ObtenerEdificios();

                // Creamos una nueva fila para "Todos los edificios"
                DataRow filaTodos =
                    dtEdificios.NewRow();
                filaTodos["IdEdificio"] = 0;
                filaTodos["Descripcion"] = "Todos los edificios";

                dtEdificios.Rows.InsertAt(filaTodos, 0);

                CmbFiltroEdificio.ItemsSource =
                    dtEdificios.DefaultView;

                // Seleccionamos "Todos los edificios"
                CmbFiltroEdificio.SelectedValue = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar el filtro de edificios: "
                    + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // CARGAR DEPARTAMENTOS
        // =========================================================

        /// <summary>
        /// Obtiene y muestra todos los departamentos.
        /// </summary>
        private void CargarDepartamentos()
        {
            try
            {
                _departamentos =
                    _deptoNegocio.ObtenerDepartamentos();

                LstDepartamentos.ItemsSource =
                    _departamentos.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar los departamentos: "
                    + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // FILTRAR DEPARTAMENTOS
        // =========================================================

        /// <summary>
        /// Filtra las tarjetas según el edificio seleccionado.
        /// </summary>
        private void CmbFiltroEdificio_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            // Si todavía no cargamos los departamentos,
            // no hacemos nada.
            if (_departamentos == null)
                return;

            // Si no hay selección, mostramos todos.
            if (CmbFiltroEdificio.SelectedValue == null)
            {
                LstDepartamentos.ItemsSource =
                    _departamentos.DefaultView;

                return;
            }

            int idEdificio =
                Convert.ToInt32(
                    CmbFiltroEdificio.SelectedValue);

            // 0 significa "Todos los edificios"
            if (idEdificio == 0)
            {
                LstDepartamentos.ItemsSource =
                    _departamentos.DefaultView;

                return;
            }

            // Creamos una vista sobre los datos originales.
            DataView vista =
                new DataView(_departamentos);

            // Filtramos por edificio.
            vista.RowFilter =
                $"IdEdificio = {idEdificio}";

            // Mostramos solamente los departamentos
            // del edificio seleccionado.
            LstDepartamentos.ItemsSource = vista;
        }


        // =========================================================
        // GUARDAR
        // =========================================================

        /// <summary>
        /// Guarda o actualiza un departamento.
        /// </summary>
        private void BtnGuardar_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                if (CmbEdificio.SelectedValue == null)
                {
                    MessageBox.Show(
                        "Debe seleccionar un edificio.",
                        "Validación",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                if (string.IsNullOrWhiteSpace(TxtPiso.Text) ||
                    string.IsNullOrWhiteSpace(TxtUnidad.Text))
                {
                    MessageBox.Show(
                        "Debe completar los campos de Piso y Unidad.",
                        "Validación",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }


                Departamento depto = new Departamento
                {
                    IdDepartamento =
                        _idDepartamentoSeleccionado ?? 0,

                    IdEdificio =
                        Convert.ToInt32(
                            CmbEdificio.SelectedValue),

                    Piso =
                        TxtPiso.Text.Trim(),

                    Unidad =
                        TxtUnidad.Text.Trim()
                };


                _deptoNegocio.GuardarDepartamento(depto);


                MessageBox.Show(
                    "Departamento guardado correctamente.",
                    "Éxito",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);


                LimpiarCampos();

                // Volvemos a cargar los departamentos
                // para actualizar las tarjetas.
                CargarDepartamentos();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al guardar: " + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // SELECCIONAR TARJETA
        // =========================================================

        /// <summary>
        /// Se dispara al hacer clic sobre cualquier tarjeta
        /// de departamento.
        /// </summary>
        private void TarjetaDepartamento_MouseDown(
            object sender,
            MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element &&
                element.DataContext is DataRowView row)
            {
                _idDepartamentoSeleccionado =
                    Convert.ToInt32(
                        row["IdDepartamento"]);

                CmbEdificio.SelectedValue =
                    row["IdEdificio"];

                TxtPiso.Text =
                    row["Piso"].ToString();

                TxtUnidad.Text =
                    row["Unidad"].ToString();
            }
        }


        // =========================================================
        // LIMPIAR
        // =========================================================

        /// <summary>
        /// Evento del botón Limpiar.
        /// </summary>
        private void BtnLimpiar_Click(
            object sender,
            RoutedEventArgs e)
        {
            LimpiarCampos();
        }


        /// <summary>
        /// Restablece los controles del formulario
        /// y cancela la selección actual.
        /// </summary>
        private void LimpiarCampos()
        {
            _idDepartamentoSeleccionado = null;

            CmbEdificio.SelectedIndex = -1;

            TxtPiso.Clear();

            TxtUnidad.Clear();
        }
    }
}