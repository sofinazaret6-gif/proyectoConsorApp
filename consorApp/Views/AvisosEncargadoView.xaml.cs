using System;
using System.Data;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using ConsorApp.Entidades;
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
        // Todos los avisos cargados (sin filtrar) y nombre de cada departamento
        private List<AvisoItem> _todosLosAvisos = new List<AvisoItem>();
        private readonly Dictionary<int, string> _nombresDepartamentos = new Dictionary<int, string>();

        // Evita que los filtros se disparen mientras se arma la ventana
        private bool _filtrosListos = false;
        public AvisosEncargadoView()
        {
            InitializeComponent();

            _avisoNegocio = new AvisoNegocio();
            _departamentoNegocio = new DepartamentoNegocio();

            CargarDestinatarios();
            CargarHistorialAvisos();

            _filtrosListos = true;     // <-- NUEVO
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

                _nombresDepartamentos.Clear();

                foreach (DataRow fila in departamentos.Rows)
                {
                    int idDepartamento = Convert.ToInt32(fila["IdDepartamento"]);

                    string piso = fila["Piso"]?.ToString() ?? "";
                    string unidad = fila["Unidad"]?.ToString() ?? "";
                    string propietario = fila["NombrePropietario"]?.ToString() ?? "";

                    string nombreDepartamento = $"Piso {piso} - Dpto {unidad}";

                    if (!string.IsNullOrWhiteSpace(propietario) && propietario != "Sin Asignar")
                    {
                        nombreDepartamento += $" ({propietario})";
                    }

                    // Se guarda para mostrar el destinatario en la lista de avisos
                    _nombresDepartamentos[idDepartamento] = nombreDepartamento;

                    ComboBoxItem itemDepartamento = new ComboBoxItem();
                    itemDepartamento.Content = "🏠 " + nombreDepartamento;
                    itemDepartamento.Tag = idDepartamento;

                    CmbDestinatario.Items.Add(itemDepartamento);
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
                var avisos = _avisoNegocio.ObtenerAvisos();

                _todosLosAvisos = avisos
                    .Select(a => new AvisoItem(a, ObtenerDestino(a)))
                    .ToList();

                AplicarFiltros();
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

        private string ObtenerDestino(Aviso aviso)
        {
            if (!aviso.id_Departamento.HasValue)
                return "Todo el edificio";

            return _nombresDepartamentos.TryGetValue(aviso.id_Departamento.Value, out string? nombre)
                ? nombre
                : $"Departamento {aviso.id_Departamento.Value}";
        }

        // ============================================================
        // FILTROS
        // ============================================================

        private void Filtro_Changed(object sender, RoutedEventArgs e)
        {
            if (!_filtrosListos) return;

            AplicarFiltros();
        }

        private void BtnLimpiarFiltros_Click(object sender, RoutedEventArgs e)
        {
            _filtrosListos = false;

            TxtBuscar.Clear();
            CmbFiltroTipo.SelectedIndex = 0;      // Todos
            CmbFiltroEstado.SelectedIndex = 0;    // Vigentes
            CmbOrden.SelectedIndex = 0;           // Más recientes
            _filtrosListos = true;

            AplicarFiltros();
        }

        private void AplicarFiltros()
        {
            IEnumerable<AvisoItem> avisos = _todosLosAvisos;

            // Búsqueda por texto (título, mensaje o destinatario)
            string texto = TxtBuscar.Text.Trim();
            if (texto.Length > 0)
            {
                avisos = avisos.Where(a =>
                    a.Titulo.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                    a.Mensaje.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                    a.Destino.Contains(texto, StringComparison.OrdinalIgnoreCase));
            }

            // Tipo: 0 = Todos, 1 = General, 2 = Individual
            if (CmbFiltroTipo.SelectedIndex == 1)
                avisos = avisos.Where(a => a.EsGeneral);
            else if (CmbFiltroTipo.SelectedIndex == 2)
                avisos = avisos.Where(a => !a.EsGeneral);

            // Estado: 0 = Vigentes, 1 = Archivados, 2 = Todos
            if (CmbFiltroEstado.SelectedIndex == 0)
                avisos = avisos.Where(a => a.Vigente);
            else if (CmbFiltroEstado.SelectedIndex == 1)
                avisos = avisos.Where(a => !a.Vigente);

            // Orden: 0 = más recientes primero, 1 = más antiguos primero
            avisos = CmbOrden.SelectedIndex == 1
                ? avisos.OrderBy(a => a.FechaPublicacion)
                : avisos.OrderByDescending(a => a.FechaPublicacion);

            var lista = avisos.ToList();

            ItemsAvisos.ItemsSource = lista;

            LblCantidad.Text = $"Mostrando {lista.Count} de {_todosLosAvisos.Count} avisos";

            TxtSinResultados.Visibility =
                lista.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
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

                    // El departamento pertenece al único edificio
                    idEdificio = _idEdificio;
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