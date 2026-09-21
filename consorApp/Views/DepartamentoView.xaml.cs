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
    /// en formato de Tarjetas. Permite agregar, filtrar y asociar propietarios.
    /// </summary>
    public partial class DepartamentoView : Window
    {
        private readonly DepartamentoNegocio _deptoNegocio =
            new DepartamentoNegocio();

        private readonly EdificioNegocio _edificioNegocio =
            new EdificioNegocio();

        // Instancia para manejar la lógica de usuarios y buscar a los propietarios
        private readonly UsuarioNegocio _usuarioNegocio =
            new UsuarioNegocio();

        // Guarda el ID del departamento seleccionado para edición.
        // Si es null, se crea uno nuevo.
        private int? _idDepartamentoSeleccionado = null;

        // Guarda todos los departamentos obtenidos de la base.
        // Se utiliza para realizar los filtros.
        private DataTable? _departamentos;


        public DepartamentoView()
        {
            InitializeComponent();

            // Cargamos los desplegables y la grilla de tarjetas al iniciar la vista
            CargarDesplegableEdificios();
            CargarFiltroEdificios();
            CargarDesplegablePropietarios();
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
        // CARGAR PROPIETARIOS PARA EL FORMULARIO
        // =========================================================

        /// <summary>
        /// Busca y carga en el ComboBox únicamente a los usuarios que tienen rol de propietario.
        /// </summary>
        private void CargarDesplegablePropietarios()
        {
            try
            {
                DataTable dtPropietarios = _usuarioNegocio.ObtenerPropietarios();

                CmbPropietario.ItemsSource = dtPropietarios.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo cargar la lista de propietarios: " + ex.Message,
                    "Aviso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
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

                // Seleccionamos "Todos los edificios" por defecto
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
        /// Obtiene y muestra todos los departamentos en las tarjetas.
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
        /// Filtra las tarjetas según el edificio que elegimos en el combo superior.
        /// </summary>
        private void CmbFiltroEdificio_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (_departamentos == null)
                return;

            if (CmbFiltroEdificio.SelectedValue == null)
            {
                LstDepartamentos.ItemsSource =
                    _departamentos.DefaultView;

                return;
            }

            int idEdificio =
                Convert.ToInt32(
                    CmbFiltroEdificio.SelectedValue);

            if (idEdificio == 0)
            {
                LstDepartamentos.ItemsSource =
                    _departamentos.DefaultView;

                return;
            }

            DataView vista =
                new DataView(_departamentos);

            vista.RowFilter =
                $"IdEdificio = {idEdificio}";

            LstDepartamentos.ItemsSource = vista;
        }


        // =========================================================
        // GUARDAR
        // =========================================================

        /// <summary>
        /// Valida los campos, guarda el departamento y le asigna el propietario opcional si se eligió uno.
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
                        "Che, te faltó seleccionar un edificio.",
                        "Validación",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                if (string.IsNullOrWhiteSpace(TxtPiso.Text) ||
                    string.IsNullOrWhiteSpace(TxtUnidad.Text))
                {
                    MessageBox.Show(
                        "Completá el piso y la unidad antes de guardar.",
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

                int idDeptoGuardado = _deptoNegocio.GuardarDepartamento(depto);

                if (CmbPropietario.SelectedValue != null)
                {
                    int idUsuarioPropietario = Convert.ToInt32(CmbPropietario.SelectedValue);

                    _deptoNegocio.AsignarPropietarioADepartamento(idDeptoGuardado, idUsuarioPropietario);
                }

                MessageBox.Show(
                    "¡Departamento guardado con éxito!",
                    "Excelente",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                LimpiarCampos();

                CargarDepartamentos();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Hubo un problema al guardar: " + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // SELECCIONAR TARJETA
        // =========================================================

        /// <summary>
        /// Carga los datos del departamento en el formulario al hacerle clic a su tarjeta.
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

        private void BtnLimpiar_Click(
            object sender,
            RoutedEventArgs e)
        {
            LimpiarCampos();
        }


        /// <summary>
        /// Resetea los controles del formulario y cancela la selección actual.
        /// </summary>
        private void LimpiarCampos()
        {
            _idDepartamentoSeleccionado = null;

            CmbEdificio.SelectedIndex = -1;
            CmbPropietario.SelectedIndex = -1;

            TxtPiso.Clear();
            TxtUnidad.Clear();
        }
    }
}