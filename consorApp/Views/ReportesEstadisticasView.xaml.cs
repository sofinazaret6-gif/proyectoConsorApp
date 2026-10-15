using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;
using ConsorApp.Datos;
using System.IO;
using System.Linq;
using System.Diagnostics;
using ConsorApp.Negocio;
using ConsorApp.Negocio.Modelos;

namespace consorApp.Views
{
    public partial class ReportesEstadisticasView : Window
    {
        private readonly ReportesNegocio _negocio = new ReportesNegocio();
        private readonly ReportesPdfService _pdf = new ReportesPdfService();
        private ResumenReportes? _resumen;


        public ReportesEstadisticasView()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Fechas por defecto (del mes actual)
            DpDesde.SelectedDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            DpHasta.SelectedDate = DateTime.Today;

            CargarDatos();
        }

        private void BtnActualizar_Click(object sender, RoutedEventArgs e)
        {
            CargarDatos();
        }

        private void CargarDatos()
        {
            try
            {
                DateTime desde = DpDesde.SelectedDate ?? DateTime.Today.AddMonths(-1);
                DateTime hasta = DpHasta.SelectedDate ?? DateTime.Today;

                _resumen = _negocio.ObtenerResumen(desde, hasta);

                DgMorosidad.ItemsSource = _resumen.Morosidad.DefaultView;
                DgReclamosEstadisticas.ItemsSource = _resumen.Reclamos.DefaultView;
                DgUsoEspacios.ItemsSource = _resumen.Espacios.DefaultView;
                DgGastos.ItemsSource = _resumen.Gastos.DefaultView;
                DgFranjas.ItemsSource = _resumen.Franjas.DefaultView;

                TxtVacioMorosidad.Visibility = _resumen.UnidadesMorosas == 0 ? Visibility.Visible : Visibility.Collapsed;
                TxtVacioReclamos.Visibility = _resumen.Reclamos.Rows.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                TxtVacioEspacios.Visibility = _resumen.ReservasPeriodo == 0 ? Visibility.Visible : Visibility.Collapsed;
                TxtVacioGastos.Visibility = _resumen.Gastos.Rows.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

                KpiUnidadesMorosas.Text = _resumen.UnidadesMorosas.ToString();
                KpiDeudaTotal.Text = ReportesPdfService.Peso(_resumen.DeudaTotal);
                KpiReclamosPendientes.Text = _resumen.ReclamosPendientes.ToString();
                KpiReservasMes.Text = _resumen.ReservasPeriodo.ToString();
                KpiGastos.Text = ReportesPdfService.Peso(_resumen.TotalGastos);

                CargarGraficos(_resumen.Morosidad, _resumen.Reclamos, _resumen.Espacios, _resumen.Gastos);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los reportes: " + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExportarPdf_Click(object sender, RoutedEventArgs e)
        {
            if (_resumen == null)
            {
                MessageBox.Show("Primero cargá los datos con «Actualizar».", "Atención",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int pestana = TabReportes.SelectedIndex;
            string clave = pestana switch
            {
                0 => "Morosidad",
                1 => "Reclamos",
                2 => "Uso_Espacios",
                _ => "Gastos_por_Concepto"
            };
            string edificio = string.Concat(_resumen.Edificio.Nombre
                .Where(ch => !Path.GetInvalidFileNameChars().Contains(ch))).Trim().Replace(' ', '_');

            var dialogo = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Guardar reporte en PDF",
                Filter = "Archivo PDF (*.pdf)|*.pdf",
                FileName = $"Reporte_{clave}_{edificio}_{DateTime.Now:yyyyMMdd}.pdf"
            };
            if (dialogo.ShowDialog() != true) return;

            try
            {
                switch (pestana)
                {
                    case 0:
                        _pdf.GenerarMorosidad(dialogo.FileName, _resumen.Edificio,
                            _resumen.Desde, _resumen.Hasta, _resumen.Morosidad);
                        break;
                    case 1:
                        _pdf.GenerarReclamos(dialogo.FileName, _resumen.Edificio,
                            _resumen.Desde, _resumen.Hasta, _resumen.Reclamos);
                        break;
                    case 2:
                        _pdf.GenerarEspacios(dialogo.FileName, _resumen.Edificio,
                            _resumen.Desde, _resumen.Hasta, _resumen.Espacios,
                            _resumen.Franjas, _resumen.EspacioTurno);
                        break;
                    default:
                        _pdf.GenerarGastos(dialogo.FileName, _resumen.Edificio,
                            _resumen.Desde, _resumen.Hasta, _resumen.Gastos);
                        break;
                }

                if (MessageBox.Show("El PDF se generó correctamente. ¿Querés abrirlo?", "Reporte listo",
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    Process.Start(new ProcessStartInfo(dialogo.FileName) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al generar el PDF: " + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarGraficos(DataTable dtMorosidad, DataTable dtReclamos, DataTable dtEspacios, DataTable dtGastos)
        {
            // Gráfico de Deuda por Unidad (Torta)
            var seriesDeuda = new SeriesCollection();
            foreach (DataRow row in dtMorosidad.Rows)
            {
                string unidad = "Unidad " + row["Unidad"].ToString();
                double deuda = row["Deuda"] != DBNull.Value ? Convert.ToDouble(row["Deuda"]) : 0;

                seriesDeuda.Add(new PieSeries
                {
                    Title = unidad,
                    Values = new ChartValues<double> { deuda },
                    DataLabels = true
                });
            }
            GraficoDeudaPorUnidad.Series = seriesDeuda;

            // Gráfico de Reclamos Frecuentes (Torta)
            var seriesReclamos = new SeriesCollection();
            foreach (DataRow row in dtReclamos.Rows)
            {
                string motivo = row["Categoria"].ToString();   // antes: row["Motivo"]
                double total = row["TotalReclamos"] != DBNull.Value ? Convert.ToDouble(row["TotalReclamos"]) : 0;

                seriesReclamos.Add(new PieSeries
                {
                    Title = motivo,
                    Values = new ChartValues<double> { total },
                    DataLabels = true
                });
            }
            GraficoReclamos.Series = seriesReclamos;

            // Gráfico de Uso de Espacios Comunes (Torta)
            var seriesEspacios = new SeriesCollection();
            foreach (DataRow row in dtEspacios.Rows)
            {
                string espacio = row["Espacio"].ToString();
                double reservas = row["CantidadReservas"] != DBNull.Value ? Convert.ToDouble(row["CantidadReservas"]) : 0;

                seriesEspacios.Add(new PieSeries
                {
                    Title = espacio,
                    Values = new ChartValues<double> { reservas },
                    DataLabels = true
                });
            }
            GraficoUsoEspacios.Series = seriesEspacios;

            var seriesGastos = new SeriesCollection();
            foreach (DataRow row in dtGastos.Rows)
            {
                double monto = row["TotalGastado"] != DBNull.Value ? Convert.ToDouble(row["TotalGastado"]) : 0;
                seriesGastos.Add(new PieSeries
                {
                    Title = row["Concepto"].ToString(),
                    Values = new ChartValues<double> { monto },
                    DataLabels = true
                });
            }
            GraficoGastos.Series = seriesGastos;
        }


        
        }
    }
