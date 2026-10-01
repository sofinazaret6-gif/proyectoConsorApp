using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

using consorApp.Seguridad;

using ConsorApp.Negocio;

namespace consorApp.Views
{
    public partial class AvisosEncargadoView : Window
    {
        private readonly AvisoNegocio _avisoNegocio;
        private readonly DepartamentoNegocio _departamentoNegocio;

        // ID del único edificio del sistema
        private int _idEdificio;

        public AvisosEncargadoView()
        {
            InitializeComponent();

            _avisoNegocio = new AvisoNegocio();
            _departamentoNegocio = new DepartamentoNegocio();

            CargarDestinatarios();

            CargarHistorialAvisos();
        }


        // =========================================================
        // CARGAR DESTINATARIOS
        // =========================================================

        private void CargarDestinatarios()
        {
            try
            {
                CmbDestinatario.Items.Clear();

                // Obtenemos todos los departamentos
                DataTable departamentos =
                    _departamentoNegocio.ObtenerDepartamentos();

                // Verificamos que exista al menos un departamento
                if (departamentos.Rows.Count == 0)
                {
                    throw new Exception(
                        "No hay departamentos registrados en el edificio.");
                }

                // Como el sistema trabaja con un solo edificio,
                // tomamos el edificio de los departamentos.
                _idEdificio =
                    Convert.ToInt32(
                        departamentos.Rows[0]["IdEdificio"]);


                // -------------------------------------------------
                // OPCIÓN: TODO EL EDIFICIO
                // -------------------------------------------------

                ComboBoxItem itemEdificio = new ComboBoxItem();

                itemEdificio.Content =
                    "📢 Todo el edificio";

                // Guardamos el ID del edificio en el Tag
                itemEdificio.Tag = _idEdificio;

                CmbDestinatario.Items.Add(itemEdificio);


                // -------------------------------------------------
                // OPCIONES: DEPARTAMENTOS
                // -------------------------------------------------

                foreach (DataRow fila in departamentos.Rows)
                {
                    int idDepartamento =
                        Convert.ToInt32(
                            fila["IdDepartamento"]);

                    string piso =
                        fila["Piso"]?.ToString() ?? "";

                    string unidad =
                        fila["Unidad"]?.ToString() ?? "";

                    string propietario =
                        fila["NombrePropietario"]?.ToString()
                        ?? "";


                    ComboBoxItem itemDepartamento =
                        new ComboBoxItem();


                    string textoDepartamento =
                        $"🏠 Piso {piso} - Dpto {unidad}";


                    // Si tiene propietario, lo mostramos
                    if (!string.IsNullOrWhiteSpace(propietario) &&
                        propietario != "Sin Asignar")
                    {
                        textoDepartamento +=
                            $" ({propietario})";
                    }


                    itemDepartamento.Content =
                        textoDepartamento;


                    // Guardamos el ID del departamento
                    itemDepartamento.Tag =
                        idDepartamento;


                    CmbDestinatario.Items.Add(
                        itemDepartamento);
                }


                // Seleccionar "Todo el edificio"
                if (CmbDestinatario.Items.Count > 0)
                {
                    CmbDestinatario.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar los destinatarios: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // CAMBIO DE DESTINATARIO
        // =========================================================

        private void CmbDestinatario_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            // Por ahora no necesitamos hacer nada especial.
            //
            // El Tag del elemento seleccionado nos permite saber
            // si se eligió el edificio o un departamento.
        }


        // =========================================================
        // CARGAR HISTORIAL
        // =========================================================

        private void CargarHistorialAvisos()
        {
            try
            {
                var listaAvisos =
                    _avisoNegocio.ObtenerAvisos();

                ItemsAvisos.ItemsSource =
                    listaAvisos;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar el historial: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // PUBLICAR AVISO
        // =========================================================

        private void BtnPublicar_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                // -------------------------------------------------
                // VERIFICAR SESIÓN Y PERMISOS
                // -------------------------------------------------

                if (!SesionUsuario.PuedePublicarAvisos ||
                    SesionUsuario.IdUsuario <= 0)
                {
                    MessageBox.Show(
                        "No tenés permisos para publicar avisos.",
                        "Acceso denegado",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }


                // -------------------------------------------------
                // OBTENER DATOS DEL FORMULARIO
                // -------------------------------------------------

                string titulo = TxtTitulo.Text.Trim();

                string mensaje = TxtMensaje.Text.Trim();


                // -------------------------------------------------
                // VERIFICAR DESTINATARIO
                // -------------------------------------------------

                if (CmbDestinatario.SelectedItem == null)
                {
                    MessageBox.Show(
                        "Seleccioná a quién querés dirigir el aviso.",
                        "Destinatario",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }


                ComboBoxItem seleccionado =
                    (ComboBoxItem)CmbDestinatario.SelectedItem;


                // -------------------------------------------------
                // VARIABLES PARA EL AVISO
                // -------------------------------------------------

                int? idEdificio = null;

                int? idDepartamento = null;


                // -------------------------------------------------
                // DETERMINAR DESTINATARIO
                // -------------------------------------------------

                if (CmbDestinatario.SelectedIndex == 0)
                {
                    // =============================================
                    // TODO EL EDIFICIO
                    // =============================================

                    idEdificio = _idEdificio;

                    idDepartamento = null;
                }
                else
                {
                    // =============================================
                    // DEPARTAMENTO ESPECÍFICO
                    // =============================================

                    if (seleccionado.Tag == null)
                    {
                        throw new Exception(
                            "No se pudo identificar el departamento seleccionado.");
                    }

                    idDepartamento =
                        Convert.ToInt32(
                            seleccionado.Tag);

                    idEdificio = null;
                }


                // -------------------------------------------------
                // REGISTRAR AVISO
                // -------------------------------------------------

                _avisoNegocio.RegistrarAviso(
                    titulo,
                    mensaje,
                    SesionUsuario.IdUsuario,
                    idEdificio,
                    idDepartamento);


                // -------------------------------------------------
                // MENSAJE DE ÉXITO
                // -------------------------------------------------

                MessageBox.Show(
                    "¡Aviso publicado con éxito!",
                    "Éxito",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);


                // -------------------------------------------------
                // LIMPIAR FORMULARIO
                // -------------------------------------------------

                TxtTitulo.Clear();

                TxtMensaje.Clear();

                CmbDestinatario.SelectedIndex = 0;


                // Actualizar historial
                CargarHistorialAvisos();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Advertencia",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }


        // =========================================================
        // ELIMINAR AVISO
        // =========================================================

        private void BtnEliminar_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btn &&
                    btn.Tag is int idAviso)
                {
                    var resultado = MessageBox.Show(
                                 "¿Está seguro de archivar este aviso?\n\nEl aviso no se eliminará de la base de datos.",
                                   "Confirmar archivo",
                    MessageBoxButton.YesNo,
                       MessageBoxImage.Question
                        );


                    if (resultado ==
                        MessageBoxResult.Yes)
                    {
                        _avisoNegocio.ArchivarAviso(idAviso);

                        CargarHistorialAvisos();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al eliminar el aviso: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}