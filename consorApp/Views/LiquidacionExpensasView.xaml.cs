using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace consorApp.Views
{
    public partial class LiquidacionExpensasView : Window
    {
        // Modelo simulado
        public class GastoEdificioDemo
        {
            public string Concepto { get; set; } = string.Empty;
            public string Descripcion { get; set; } = string.Empty;
            public decimal Monto { get; set; }
            public string Periodo { get; set; } = string.Empty;
            public DateTime Fecha { get; set; }
        }

        // Datos estáticos
        private static List<GastoEdificioDemo> _listaGastos = new List<GastoEdificioDemo>
        {
            new GastoEdificioDemo { Concepto = "Mantenimiento Limpieza", Descripcion = "Insumos y personal de limpieza general", Monto = 120000.00m, Periodo = "09/2026", Fecha = new DateTime(2026, 9, 5) },
            new GastoEdificioDemo { Concepto = "Servicio Eléctrico (Luz)", Descripcion = "Luz de pasillos y espacios comunes", Monto = 85000.50m, Periodo = "09/2026", Fecha = new DateTime(2026, 9, 10) },
            new GastoEdificioDemo { Concepto = "Reparación Ascensor", Descripcion = "Mantenimiento preventivo mensual ascensor", Monto = 95000.00m, Periodo = "09/2026", Fecha = new DateTime(2026, 9, 15) }
        };

        private const int DEPARTAMENTOS_CON_PROPIETARIO = 12;

        public LiquidacionExpensasView()
        {
            InitializeComponent();
            DpFecha.SelectedDate = DateTime.Now;
            ActualizarGrillaGastos();
            CalcularExpensasSimulado();
        }

        private void ActualizarGrillaGastos()
        {
            string filtro = TxtFiltroPeriodo.Text.Trim();
            var gastosFiltrados = _listaGastos.Where(g => g.Periodo.Equals(filtro, StringComparison.OrdinalIgnoreCase)).ToList();
            DgGastos.ItemsSource = null;
            DgGastos.ItemsSource = gastosFiltrados;
        }

        private void BtnGuardarGasto_Click(object sender, RoutedEventArgs e)
        {
            if (CmbConcepto.SelectedItem == null)
            {
                MessageBox.Show("Seleccione un concepto de gasto.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(TxtMonto.Text, out decimal monto) || monto <= 0)
            {
                MessageBox.Show("Ingrese un monto válido y mayor a cero.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string conceptoTexto = (CmbConcepto.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Varios";

            _listaGastos.Add(new GastoEdificioDemo
            {
                Concepto = conceptoTexto,
                Descripcion = TxtDescripcion.Text.Trim(),
                Monto = monto,
                Periodo = TxtPeriodo.Text.Trim(),
                Fecha = DpFecha.SelectedDate ?? DateTime.Now
            });

            MessageBox.Show("¡Gasto registrado con éxito!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

            TxtDescripcion.Clear();
            TxtMonto.Clear();

            ActualizarGrillaGastos();
            CalcularExpensasSimulado();
        }

        private void BtnFiltrar_Click(object sender, RoutedEventArgs e)
        {
            ActualizarGrillaGastos();
        }

        private void BtnCalcular_Click(object sender, RoutedEventArgs e)
        {
            CalcularExpensasSimulado();
        }

        private void CalcularExpensasSimulado()
        {
            string periodoLiquidar = TxtPeriodoLiquidar.Text.Trim();

            decimal totalGastosPeriodo = _listaGastos
                .Where(g => g.Periodo.Equals(periodoLiquidar, StringComparison.OrdinalIgnoreCase))
                .Sum(g => g.Monto);

            int cantidadDeptos = DEPARTAMENTOS_CON_PROPIETARIO;
            decimal expensaPorDepto = cantidadDeptos > 0 ? (totalGastosPeriodo / cantidadDeptos) : 0;

            LblTotalGastos.Text = $"$ {totalGastosPeriodo:N2}";
            LblDeptosPropietario.Text = cantidadDeptos.ToString();
            LblExpensaIndividual.Text = $"$ {expensaPorDepto:N2}";
        }
    }
}