using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;


using ConsorApp.Entidades;
using ConsorApp.Negocio;

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
                string periodo =
                    TxtPeriodoLiquidar.Text.Trim();

                if (string.IsNullOrWhiteSpace(periodo))
                {
                    MessageBox.Show(
                        "Ingrese el período.",
                        "Atención",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

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
                string periodo =
                    TxtPeriodoLiquidar.Text.Trim();

                if (string.IsNullOrWhiteSpace(periodo))
                {
                    MessageBox.Show(
                        "Ingrese el período.",
                        "Atención",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

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
                string periodo =
                    TxtPeriodoDetalle.Text.Trim();


                if (string.IsNullOrWhiteSpace(periodo))
                {
                    MessageBox.Show(
                        "Ingrese un período.",
                        "Atención",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

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

                    return;
                }


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
                CmbConceptoGasto.ItemsSource =
                    _conceptoGastoNegocio.ObtenerConceptos();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudieron cargar los conceptos de gasto.\n\n" + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
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

                string periodo = TxtPeriodoGasto.Text.Trim();

                if (string.IsNullOrWhiteSpace(periodo))
                {
                    MessageBox.Show(
                        "Ingrese el período del gasto. Ejemplo: 10/2026.",
                        "Atención",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

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
                string periodo = TxtFiltroPeriodoGasto.Text.Trim();

                if (string.IsNullOrWhiteSpace(periodo))
                {
                    DgGastos.ItemsSource =
                        _gastoEdificioNegocio.ObtenerGastos();

                    return;
                }

                DgGastos.ItemsSource =
                    _gastoEdificioNegocio.ObtenerGastosPorPeriodo(periodo);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudieron cargar los gastos.\n\n" + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        private void BtnFiltrarGastos_Click(object sender, RoutedEventArgs e)
        {
            ActualizarGrillaGastos();
        }
    }
}