using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ConsorApp.Entidades;
using ConsorApp.Negocio;

namespace consorApp.Views
{
    public partial class DepartamentoView : Window
    {
        private readonly DepartamentoNegocio _deptoNegocio = new DepartamentoNegocio();
        private readonly UsuarioNegocio _usuarioNegocio = new UsuarioNegocio();
        private readonly EdificioNegocio _edificioNegocio = new EdificioNegocio();

        private int? _idDepartamentoSeleccionado = null;
        private int _idEdificioUnico = 1; // Valor por defecto si falla la carga

        public DepartamentoView()
        {
            InitializeComponent();
            CargarEdificioUnico();
            CargarDesplegablePropietarios();
            CargarDepartamentos();
        }

        /// <summary>
        /// Obtiene la información del único edificio cargado y setea el nombre en la UI
        /// </summary>
        private void CargarEdificioUnico()
        {
            try
            {
                EDIFICIO edificio = _edificioNegocio.ObtenerUnicoEdificio();
                if (edificio != null)
                {
                    _idEdificioUnico = edificio.id_edificio;
                    TxtEdificioNombre.Text = edificio.Descripcion;
                }
            }
            catch
            {
                TxtEdificioNombre.Text = "Edificio Principal";
            }
        }

        private void CargarDesplegablePropietarios()
        {
            try
            {
                DataTable dtPropietarios = _usuarioNegocio.ObtenerPropietarios();

                // Creamos una fila vacía para representar el valor nulo o "Sin Asignar"
                DataRow filaVacia = dtPropietarios.NewRow();
                filaVacia["idUsuario"] = DBNull.Value; // O 0 dependiendo de tu estructura, pero DBNull maneja el nulo perfecto
                filaVacia["Nombre"] = "(Sin Propietario / Vacío)";

                // Insertamos la opción vacía al principio de todo
                dtPropietarios.Rows.InsertAt(filaVacia, 0);

                CmbPropietario.ItemsSource = dtPropietarios.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo cargar la lista de propietarios: " + ex.Message, "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CargarDepartamentos()
        {
            try
            {
                LstDepartamentos.ItemsSource = _deptoNegocio.ObtenerDepartamentos().DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los departamentos: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TxtPiso.Text) || string.IsNullOrWhiteSpace(TxtUnidad.Text))
                {
                    MessageBox.Show("Completá el piso y la unidad antes de guardar.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Departamento depto = new Departamento
                {
                    IdDepartamento = _idDepartamentoSeleccionado ?? 0,
                    IdEdificio = _idEdificioUnico,
                    Piso = TxtPiso.Text.Trim(),
                    Unidad = TxtUnidad.Text.Trim()
                };

                int idDeptoGuardado = _deptoNegocio.GuardarDepartamento(depto);

                // Verificamos si seleccionó un propietario válido o eligió el vacío (null)
                int? idUsuarioPropietario = null;
                if (CmbPropietario.SelectedValue != null && CmbPropietario.SelectedValue != DBNull.Value)
                {
                    idUsuarioPropietario = Convert.ToInt32(CmbPropietario.SelectedValue);
                }

                _deptoNegocio.AsignarPropietarioADepartamento(idDeptoGuardado, idUsuarioPropietario);

                MessageBox.Show("¡Departamento guardado con éxito!", "Excelente", MessageBoxButton.OK, MessageBoxImage.Information);

                LimpiarCampos();
                CargarDepartamentos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hubo un problema al guardar: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TarjetaDepartamento_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is DataRowView row)
            {
                _idDepartamentoSeleccionado = Convert.ToInt32(row["IdDepartamento"]);
                TxtPiso.Text = row["Piso"].ToString();
                TxtUnidad.Text = row["Unidad"].ToString();

                // Intentamos buscar si la fila tiene el IdPropietario asociado
                if (row.Row.Table.Columns.Contains("IdPropietario") && row["IdPropietario"] != DBNull.Value)
                {
                    CmbPropietario.SelectedValue = row["IdPropietario"];
                }
                else if (row.Row.Table.Columns.Contains("IdUsuarioPropietario") && row["IdUsuarioPropietario"] != DBNull.Value)
                {
                    CmbPropietario.SelectedValue = row["IdUsuarioPropietario"];
                }
                else
                {
                    // Si no tiene propietario, seleccionamos el ítem vacío (índice 0)
                    CmbPropietario.SelectedIndex = 0;
                }
            }
        }

        private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarCampos();
        }

        private void LimpiarCampos()
        {
            _idDepartamentoSeleccionado = null;
            CmbPropietario.SelectedIndex = 0; // Selecciona la opción vacía por defecto
            TxtPiso.Clear();
            TxtUnidad.Clear();
        }
    }
}