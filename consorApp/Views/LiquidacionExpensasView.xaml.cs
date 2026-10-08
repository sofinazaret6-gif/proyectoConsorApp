using ConsorApp.Entidades;
using ConsorApp.Negocio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace consorApp.Views
{
    public partial class LiquidacionExpensasView : Window
    {
        private readonly ExpensasNegocio _expensaNegocio;
        private readonly HistorialExpensaNegocio _historialNegocio;
        private readonly ConceptoGastoNegocio _conceptoGastoNegocio =
            new ConceptoGastoNegocio();

        private readonly GastoEdificioNegocio _gastoEdificioNegocio =
            new GastoEdificioNegocio();
        private decimal _expensaCalculada = 0;
        private int _cantidadDepartamentos = 0;
        private Expensa? _expensaActual;


        public LiquidacionExpensasView()
        {
            InitializeComponent();
            DpFechaVencimiento.SelectedDate =
               DateTime.Now.AddDays(10);
            _expensaNegocio = new ExpensasNegocio();
            _historialNegocio = new HistorialExpensaNegocio();
            DpFechaGasto.SelectedDate = DateTime.Now;

            TxtFiltroPeriodoGasto.Text = DateTime.Now.ToString("MM/yyyy");

            TxtPeriodoGasto.Text = DateTime.Now.ToString("MM/yyyy");
            string periodoActual = DateTime.Now.ToString("MM/yyyy");
            TxtPeriodoLiquidar.Text = periodoActual;
            TxtPeriodoDetalle.Text = periodoActual;

            CargarConceptosGasto();

            ActualizarGrillaGastos();
            CargarExpensas();
            CargarHistorial();

        }


        // ============================================================
        // EXPENSAS
        // ============================================================

        private void CargarExpensas()
        {
            try
            {
                List<Expensa> expensas =
                    _expensaNegocio.ObtenerExpensas();

                DgExpensas.ItemsSource = expensas;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar las expensas:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        private void BtnCalcular_Click(
      object sender,
      RoutedEventArgs e)
        {
            try
            {
                string? periodo = NormalizarPeriodo(TxtPeriodoLiquidar.Text);

                if (periodo == null)
                {
                    MessageBox.Show("Ingrese el período con formato MM/aaaa. Ejemplo: 10/2026.",
                        "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                TxtPeriodoLiquidar.Text = periodo;

                if (_expensaNegocio.ObtenerPorPeriodo(periodo) != null)
                {
                    MessageBox.Show(
                        $"El período {periodo} ya fue liquidado. Podés verlo en la pestaña 'Detalle de Expensas'.",
                        "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // =====================================================
                // 1. OBTENER TOTAL DE GASTOS DEL PERÍODO
                // =====================================================

                decimal totalGastos =
                    _expensaNegocio.ObtenerTotalGastosPorPeriodo(periodo);

                if (totalGastos <= 0)
                {
                    LblDeptosPropietario.Text = "0";
                    LblExpensaIndividual.Text = "$ 0,00";
                    TxtMontoTotalGastos.Clear();
                    _expensaCalculada = 0;
                    MessageBox.Show(
                        $"No existen gastos registrados para el período {periodo}.",
                        "Información",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    return;
                }

                // Mostramos el total calculado automáticamente.
                TxtMontoTotalGastos.Text =
                    totalGastos.ToString("N2");


                // =====================================================
                // 2. OBTENER DEPARTAMENTOS CON PROPIETARIO
                // =====================================================

                List<Departamento> departamentos =
                    _expensaNegocio
                        .ObtenerDepartamentosConPropietario();

                _cantidadDepartamentos =
                    departamentos.Count;

                LblDeptosPropietario.Text =
                    _cantidadDepartamentos.ToString();


                if (_cantidadDepartamentos == 0)
                {
                    _expensaCalculada = 0;

                    LblExpensaIndividual.Text =
                        "$ 0,00";

                    MessageBox.Show(
                        "No hay departamentos con propietario activo.",
                        "Información",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    return;
                }


                // =====================================================
                // 3. CALCULAR EXPENSA INDIVIDUAL
                // =====================================================

                _expensaCalculada =
                    Math.Round(
                        totalGastos / _cantidadDepartamentos,
                        2);

                LblExpensaIndividual.Text =
                    $"$ {_expensaCalculada:N2}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al calcular la expensa:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // ============================================================
        // GUARDAR LIQUIDACIÓN
        // ============================================================

        private void BtnGuardarLiquidacion_Click(
      object sender,
      RoutedEventArgs e)
        {
            try
            {
                string? periodo = NormalizarPeriodo(TxtPeriodoLiquidar.Text);

                if (periodo == null)
                {
                    MessageBox.Show("Ingrese el período con formato MM/aaaa. Ejemplo: 10/2026.",
                        "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Evita liquidar dos veces el mismo período
                if (_expensaNegocio.ObtenerPorPeriodo(periodo) != null)
                {
                    MessageBox.Show($"El período {periodo} ya fue liquidado.",
                        "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!DpFechaVencimiento.SelectedDate.HasValue)
                {
                    MessageBox.Show(
                        "Seleccione una fecha de vencimiento.",
                        "Atención",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }


                // =====================================================
                // 1. VOLVEMOS A OBTENER LOS GASTOS DESDE LA BASE
                // =====================================================

                decimal totalGastos =
                    _expensaNegocio.ObtenerTotalGastosPorPeriodo(periodo);

                if (totalGastos <= 0)
                {
                    MessageBox.Show(
                        $"No existen gastos registrados para el período {periodo}.",
                        "Atención",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }


                // =====================================================
                // 2. OBTENER DEPARTAMENTOS CON PROPIETARIO
                // =====================================================

                List<Departamento> departamentos =
                    _expensaNegocio
                        .ObtenerDepartamentosConPropietario();

                if (departamentos.Count == 0)
                {
                    MessageBox.Show(
                        "No existen departamentos con propietario para generar la liquidación.",
                        "Atención",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }


                // =====================================================
                // 3. CALCULAR VALOR INDIVIDUAL
                // =====================================================

                decimal valorIndividual =
                    Math.Round(
                        totalGastos / departamentos.Count,
                        2);


                // =====================================================
                // 4. OBTENER EL EDIFICIO
                // =====================================================

                int idEdificio =
                    departamentos.First().IdEdificio;


                // Verificamos que todos pertenezcan al mismo edificio.
                bool mismoEdificio =
                    departamentos.All(
                        d => d.IdEdificio == idEdificio);

                if (!mismoEdificio)
                {
                    MessageBox.Show(
                        "Se encontraron departamentos pertenecientes a distintos edificios. Revisá los datos de la base.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    return;
                }


                // =====================================================
                // 5. CREAR EXPENSA
                // =====================================================

                Expensa nuevaExpensa = new Expensa
                {
                    Id_Edificio =
                        idEdificio,

                    Periodo =
                        periodo,

                    MontoTotalGastos =
                        totalGastos,

                    ValorExpensaIndividual =
                        valorIndividual,

                    FechaEmision =
                        DateTime.Now
                };


                // =====================================================
                // 6. OBTENER IDS DE DEPARTAMENTOS
                // =====================================================

                List<int> idsDepartamentos =
                    departamentos
                        .Select(d => d.IdDepartamento)
                        .ToList();


                // =====================================================
                // 7. GUARDAR TODO
                // =====================================================

                int idNuevaExpensa =
                    _expensaNegocio.GuardarLiquidacionCompleta(
                        nuevaExpensa,
                        idsDepartamentos,
                        DpFechaVencimiento.SelectedDate.Value);


                MessageBox.Show(
                    $"Liquidación guardada correctamente.\n\n" +
                    $"Período: {periodo}\n" +
                    $"Total de gastos: ${totalGastos:N2}\n" +
                    $"Departamentos con propietario: {departamentos.Count}\n" +
                    $"Expensa individual: ${valorIndividual:N2}",
                    "Éxito",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);


                // =====================================================
                // 8. LIMPIAR
                // =====================================================

                TxtMontoTotalGastos.Clear();

                LblDeptosPropietario.Text = "0";

                LblExpensaIndividual.Text = "$ 0,00";

                DpFechaVencimiento.SelectedDate =
                    DateTime.Now.AddDays(10);


                // =====================================================
                // 9. ACTUALIZAR GRILLAS
                // =====================================================

                CargarExpensas();

                CargarHistorial();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al guardar la liquidación:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // ============================================================
        // DETALLE DE EXPENSA
        // ============================================================

        private void BtnBuscarDetalle_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                string? periodo = NormalizarPeriodo(TxtPeriodoDetalle.Text);

                if (periodo == null)
                {
                    MessageBox.Show("Ingrese el período con formato MM/aaaa. Ejemplo: 10/2026.",
                        "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }


                Expensa? expensa =
                    _expensaNegocio.ObtenerPorPeriodo(
                        periodo);


                if (expensa == null)
                {

                    DgDetalles.ItemsSource = null;

                    MessageBox.Show(
                        "No existe una liquidación para ese período.",
                        "Información",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    _expensaActual = null;
                    return;
                }
                _expensaActual = expensa;

                List<Detalle_Expensa> detalles =
                    _expensaNegocio.ObtenerDetalles(
                        expensa.Id_Expensa);


                DgDetalles.ItemsSource =
                    detalles;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al buscar los detalles:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        private static string? NormalizarPeriodo(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return null;

            if (DateTime.TryParseExact(
                    texto.Trim(),
                    new[] { "M/yyyy", "MM/yyyy" },
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime fecha))
            {
                return fecha.ToString("MM/yyyy", CultureInfo.InvariantCulture);
            }

            return null;
        }

        // ============================================================
        // EXPORTAR PDF
        // ============================================================

        private List<HistorialExpensa> ObtenerReporteDeExpensaActual()
        {
            return _historialNegocio.ObtenerHistorial()
                .Where(h => h.IdExpensa == _expensaActual!.Id_Expensa)
                .OrderBy(h => h.Piso)
                .ThenBy(h => h.Unidad)
                .ToList();
        }

        private static string LimpiarNombreArchivo(string nombre) =>
            string.Concat(nombre.Split(Path.GetInvalidFileNameChars()));

        private void BtnExportarPdfSeleccionado_Click(object sender, RoutedEventArgs e)
        {
            ExportarPdf(soloSeleccionado: true);
        }

        private void BtnExportarPdfTodos_Click(object sender, RoutedEventArgs e)
        {
            ExportarPdf(soloSeleccionado: false);
        }

        private void ExportarPdf(bool soloSeleccionado)
        {
            try
            {
                if (_expensaActual == null)
                {
                    MessageBox.Show("Primero buscá un período en esta pestaña.",
                        "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                List<HistorialExpensa> reporte = ObtenerReporteDeExpensaActual();

                if (soloSeleccionado)
                {
                    if (DgDetalles.SelectedItem is not Detalle_Expensa seleccionado)
                    {
                        MessageBox.Show("Seleccioná un departamento de la tabla.",
                            "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    reporte = reporte
                        .Where(h => h.IdDetalleExpensa == seleccionado.Id_DetalleExpensa)
                        .ToList();
                }

                if (reporte.Count == 0)
                {
                    MessageBox.Show("No se encontraron datos para generar el reporte.",
                        "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                string periodoArchivo = _expensaActual.Periodo.Replace('/', '-');
                string nombre = soloSeleccionado
                    ? $"Liquidacion_{periodoArchivo}_Piso{reporte[0].Piso}_Unidad{reporte[0].Unidad}.pdf"
                    : $"Liquidacion_{periodoArchivo}_Resumen.pdf";

                var dialogo = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Guardar liquidación en PDF",
                    Filter = "Archivo PDF (*.pdf)|*.pdf",
                    FileName = LimpiarNombreArchivo(nombre)
                };

                if (dialogo.ShowDialog() != true) return;

                var gastos = _gastoEdificioNegocio
                    .ObtenerGastosPorPeriodo(_expensaActual.Periodo)
                    .ToList();

                int cantidadDeptos = _expensaNegocio.ObtenerDetalles(_expensaActual.Id_Expensa).Count;

                // ===== AQUÍ VA EL BLOQUE NUEVO =====
                var servicio = new LiquidacionPdfService();
                string? nombreEdificio = LiquidacionPdfService.ObtenerNombreEdificio();

                if (soloSeleccionado)
                {
                    // Detalle completo de un solo departamento
                    servicio.Generar(
                        dialogo.FileName, _expensaActual, reporte, gastos, cantidadDeptos, nombreEdificio);
                }
                else
                {
                    // Hoja resumen con todos los departamentos
                    servicio.GenerarResumenGeneral(
                        dialogo.FileName, _expensaActual, reporte, gastos, nombreEdificio);
                }
                // ===================================

                var abrir = MessageBox.Show(
                    "El PDF se generó correctamente. ¿Querés abrirlo?",
                    "Reporte generado", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (abrir == MessageBoxResult.Yes)
                {
                    Process.Start(new ProcessStartInfo(dialogo.FileName) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar el PDF:\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Un archivo PDF por cada departamento, guardados en una carpeta
        private void BtnExportarIndividuales_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_expensaActual == null)
                {
                    MessageBox.Show("Primero buscá un período en esta pestaña.",
                        "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                List<HistorialExpensa> reporte = ObtenerReporteDeExpensaActual();

                if (reporte.Count == 0)
                {
                    MessageBox.Show("No se encontraron datos para generar el reporte.",
                        "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Documentos\ConsorApp\Liquidaciones\10-2026\
                string carpeta = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ConsorApp", "Liquidaciones",
                    _expensaActual.Periodo.Replace('/', '-'));

                Directory.CreateDirectory(carpeta);

                var gastos = _gastoEdificioNegocio
                    .ObtenerGastosPorPeriodo(_expensaActual.Periodo)
                    .ToList();

                int cantidadDeptos = _expensaNegocio.ObtenerDetalles(_expensaActual.Id_Expensa).Count;
                string? nombreEdificio = LiquidacionPdfService.ObtenerNombreEdificio();   // <-- NUEVO
                var servicio = new LiquidacionPdfService();


                foreach (var depto in reporte)
                {
                    string nombre = LimpiarNombreArchivo(
                        $"Liquidacion_{_expensaActual.Periodo.Replace('/', '-')}_Piso{depto.Piso}_Unidad{depto.Unidad}.pdf");

                    servicio.Generar(
                        Path.Combine(carpeta, nombre),
                        _expensaActual,
                        new List<HistorialExpensa> { depto },
                        gastos,
                        cantidadDeptos,
                        nombreEdificio);                                  // <-- NUEVO
                }

                MessageBox.Show(
                    $"Se generaron {reporte.Count} PDF en:\n{carpeta}",
                    "Reportes generados", MessageBoxButton.OK, MessageBoxImage.Information);

                Process.Start("explorer.exe", $"\"{carpeta}\"");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar los PDF:\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ============================================================
        // REGISTRAR PAGO (lo hace el encargado)
        // ============================================================

        private void DgDetalles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgDetalles.SelectedItem is not Detalle_Expensa detalle)
            {
                BtnGuardarPago.IsEnabled = false;
                LblPagoSeleccion.Text = "Registrar pago: seleccioná un departamento de la tabla";
                return;
            }

            BtnGuardarPago.IsEnabled = true;

            LblPagoSeleccion.Text =
                $"Registrar pago - Piso {detalle.Departamento?.Piso} Unidad {detalle.Departamento?.Unidad}";

            bool pagado = string.Equals(detalle.EstadoPago, "Pagado", StringComparison.OrdinalIgnoreCase);

            CmbEstadoPago.SelectedIndex = pagado ? 1 : 0;
            DpFechaPago.SelectedDate = detalle.FechaPago ?? DateTime.Today;
            CmbMetodoPago.Text = detalle.MetodoPago ?? string.Empty;
        }

        private void BtnGuardarPago_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_expensaActual == null || DgDetalles.SelectedItem is not Detalle_Expensa detalle)
                {
                    MessageBox.Show("Seleccioná un departamento de la tabla.",
                        "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string estado =
                    (CmbEstadoPago.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Pendiente";

                _expensaNegocio.RegistrarPago(
                    detalle.Id_DetalleExpensa,
                    estado,
                    DpFechaPago.SelectedDate,
                    CmbMetodoPago.Text.Trim());

                MessageBox.Show("El pago fue actualizado correctamente.",
                    "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                // Recargar la tabla y volver a seleccionar el mismo departamento
                int idSeleccionado = detalle.Id_DetalleExpensa;

                List<Detalle_Expensa> detalles =
                    _expensaNegocio.ObtenerDetalles(_expensaActual.Id_Expensa);

                DgDetalles.ItemsSource = detalles;
                DgDetalles.SelectedItem =
                    detalles.FirstOrDefault(d => d.Id_DetalleExpensa == idSeleccionado);

                CargarHistorial();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        // ============================================================
        // HISTORIAL
        // ============================================================

        private void CargarHistorial()
        {
            try
            {
                List<HistorialExpensa> historial =
                    _historialNegocio.ObtenerHistorial();

                DgHistorial.ItemsSource =
                    historial;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar el historial:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
          private int ObtenerIdEdificio()
        {
            EdificioNegocio edificioNegocio = new EdificioNegocio();

            EDIFICIO edificio = edificioNegocio.ObtenerUnicoEdificio();

            if (edificio == null || edificio.id_edificio <= 0)
            {
                throw new Exception("No hay un edificio registrado en la base de datos.");
            }

            return edificio.id_edificio;
        }

        private void BtnFiltrarHistorial_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                string texto =
                    TxtFiltroDepartamento.Text.Trim();


                if (string.IsNullOrWhiteSpace(texto))
                {
                    CargarHistorial();
                    return;
                }


                if (!int.TryParse(
                    texto,
                    out int idDepartamento))
                {
                    MessageBox.Show(
                        "Ingrese un ID de departamento válido.",
                        "Atención",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }


                List<HistorialExpensa> historial =
                    _historialNegocio
                        .ObtenerHistorialPorDepartamento(
                            idDepartamento);


                DgHistorial.ItemsSource =
                    historial;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al filtrar el historial:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        private void BtnMostrarTodoHistorial_Click(
            object sender,
            RoutedEventArgs e)
        {
            TxtFiltroDepartamento.Clear();

            CargarHistorial();
        }

        private void CargarConceptosGasto()
        {
            try
            {
                var conceptos = _conceptoGastoNegocio.ObtenerConceptos().ToList();

                // Combo del formulario
                CmbConceptoGasto.ItemsSource = conceptos;

                // Combo del filtro: se agrega "Todos" al principio
                var conceptosFiltro = new List<conceptoGasto>
        {
            new conceptoGasto { id_conceptoGasto = 0, nombreConcepto = "Todos los conceptos" }
        };
                conceptosFiltro.AddRange(conceptos);

                CmbFiltroConcepto.ItemsSource = conceptosFiltro;
                CmbFiltroConcepto.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudieron cargar los conceptos de gasto.\n\n" + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnGuardarGasto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CmbConceptoGasto.SelectedItem == null)
                {
                    MessageBox.Show(
                        "Seleccione un concepto de gasto.",
                        "Atención",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                if (!decimal.TryParse(TxtMontoGasto.Text.Trim(), out decimal monto)
                    || monto <= 0)
                {
                    MessageBox.Show(
                        "Ingrese un monto válido y mayor a cero.",
                        "Atención",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                string? periodo = NormalizarPeriodo(TxtPeriodoGasto.Text);

                if (periodo == null)
                {
                    MessageBox.Show("Ingrese el período con formato MM/aaaa. Ejemplo: 10/2026.",
                        "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_expensaNegocio.ObtenerPorPeriodo(periodo) != null)
                {
                    MessageBox.Show(
                        $"El período {periodo} ya fue liquidado. No se pueden agregar más gastos a un período cerrado.",
                        "Período liquidado", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                conceptoGasto concepto =
                    (conceptoGasto)CmbConceptoGasto.SelectedItem;

                // Como el sistema trabaja con un solo edificio,
                // tomamos el edificio existente.
                int idEdificio = ObtenerIdEdificio();

                GastoEdificio gasto = new GastoEdificio
                {
                    id_Edificio = idEdificio,
                    Id_conceptoGasto = concepto.id_conceptoGasto,
                    descripcion = TxtDescripcionGasto.Text.Trim(),
                    monto = monto,
                    periodo = periodo,
                    fecha = DpFechaGasto.SelectedDate ?? DateTime.Now,
                    estado = "ACTIVO"
                };

                _gastoEdificioNegocio.GuardarGasto(gasto);

                MessageBox.Show(
                    "El gasto fue registrado correctamente.",
                    "Éxito",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                TxtDescripcionGasto.Clear();
                TxtMontoGasto.Clear();

                ActualizarGrillaGastos();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        private void BtnNuevoConcepto_Click(object sender, RoutedEventArgs e)
        {
            NuevoConceptoGastoView ventana =
                new NuevoConceptoGastoView();

            ventana.Owner = this;

            bool? resultado = ventana.ShowDialog();

            if (resultado == true)
            {
                CargarConceptosGasto();
            }
        }
        private void ActualizarGrillaGastos()
        {
            try
            {
                // Se traen los gastos una vez y se filtra en memoria
                IEnumerable<GastoEdificio> gastos = _gastoEdificioNegocio.ObtenerGastos();

                // Período
                string textoPeriodo = TxtFiltroPeriodoGasto.Text.Trim();
                if (!string.IsNullOrWhiteSpace(textoPeriodo))
                {
                    string? periodo = NormalizarPeriodo(textoPeriodo);
                    if (periodo == null)
                    {
                        MessageBox.Show("El período debe tener el formato MM/aaaa. Ejemplo: 10/2026.",
                            "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    gastos = gastos.Where(g => g.periodo == periodo);
                }

                // Concepto
                if (CmbFiltroConcepto.SelectedItem is conceptoGasto concepto
                    && concepto.id_conceptoGasto != 0)
                {
                    gastos = gastos.Where(g => g.Id_conceptoGasto == concepto.id_conceptoGasto);
                }

                // Rango de fechas
                DateTime? desde = DpFiltroDesde.SelectedDate;
                DateTime? hasta = DpFiltroHasta.SelectedDate;

                if (desde.HasValue && hasta.HasValue && desde > hasta)
                {
                    MessageBox.Show("La fecha 'Desde' no puede ser posterior a 'Hasta'.",
                        "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (desde.HasValue)
                    gastos = gastos.Where(g => g.fecha.Date >= desde.Value.Date);

                if (hasta.HasValue)
                    gastos = gastos.Where(g => g.fecha.Date <= hasta.Value.Date);

                var lista = gastos.ToList();

                DgGastos.ItemsSource = lista;
                LblTotalGastos.Text = lista.Sum(g => g.monto).ToString("C2");
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudieron cargar los gastos.\n\n" + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnFiltrarGastos_Click(object sender, RoutedEventArgs e)
        {
            ActualizarGrillaGastos();
        }

        private void BtnLimpiarFiltrosGastos_Click(object sender, RoutedEventArgs e)
        {
            TxtFiltroPeriodoGasto.Clear();
            CmbFiltroConcepto.SelectedIndex = 0;
            DpFiltroDesde.SelectedDate = null;
            DpFiltroHasta.SelectedDate = null;

            ActualizarGrillaGastos();
        }
    }
}