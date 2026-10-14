using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;
using ConsorApp.Datos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace consorApp.Views
{
    public partial class ReportesEstadisticasView : Window
    {
        private readonly ReportesDatos _reportesDatos = new ReportesDatos();

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

                // 1. CARGAR MOROSIDAD Y DEUDAS
                DataTable dtMorosidad = _reportesDatos.ObtenerReporteMorosidad();
                DgMorosidad.ItemsSource = dtMorosidad.DefaultView;

                // KPI: Unidades morosas y Deuda Total
                int unidadesMorosas = dtMorosidad.Rows.Count;
                KpiUnidadesMorosas.Text = unidadesMorosas.ToString();

                decimal deudaTotal = 0;
                foreach (DataRow row in dtMorosidad.Rows)
                {
                    if (row["Deuda"] != DBNull.Value)
                        deudaTotal += Convert.ToDecimal(row["Deuda"]);
                }
                KpiDeudaTotal.Text = deudaTotal.ToString("$#,0.00");

                // 2. CARGAR RECLAMOS FILTRADOS POR FECHA
                DataTable dtReclamos = _reportesDatos.ObtenerEstadisticasReclamos(desde, hasta);
                DgReclamosEstadisticas.ItemsSource = dtReclamos.DefaultView;

                // KPI: Reclamos pendientes en el período
                int reclamosPendientes = 0;
                foreach (DataRow row in dtReclamos.Rows)
                {
                    if (row["Pendientes"] != DBNull.Value)
                        reclamosPendientes += Convert.ToInt32(row["Pendientes"]);
                }
                KpiReclamosPendientes.Text = reclamosPendientes.ToString();

                // 3. CARGAR ESPACIOS COMUNES FILTRADOS POR FECHA
                DataTable dtEspacios = _reportesDatos.ObtenerUsoEspaciosComunes(desde, hasta);
                DgUsoEspacios.ItemsSource = dtEspacios.DefaultView;

                // KPI: Reservas del período seleccionado
                int totalReservas = 0;
                foreach (DataRow row in dtEspacios.Rows)
                {
                    if (row["CantidadReservas"] != DBNull.Value)
                        totalReservas += Convert.ToInt32(row["CantidadReservas"]);
                }
                KpiReservasMes.Text = totalReservas.ToString();

                // 4. CARGAR GRÁFICOS DE TORTA
                CargarGraficos(dtMorosidad, dtReclamos, dtEspacios);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los reportes: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarGraficos(DataTable dtMorosidad, DataTable dtReclamos, DataTable dtEspacios)
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
                string motivo = row["Motivo"].ToString();
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
        }

        private void BtnImprimirReporte_Click(object sender, RoutedEventArgs e)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Archivo PDF (*.pdf)|*.pdf",
                FileName = $"Informe_Ejecutivo_Consorcio_{DateTime.Now:yyyyMMdd}.pdf"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    string periodoDesde = DpDesde.SelectedDate?.ToString("dd/MM/yyyy") ?? "-";
                    string periodoHasta = DpHasta.SelectedDate?.ToString("dd/MM/yyyy") ?? "-";
                    string totalMorosas = KpiUnidadesMorosas.Text;
                    string deudaTotal = KpiDeudaTotal.Text;
                    string reclamosPendientes = KpiReclamosPendientes.Text;
                    string reservasPeriodo = KpiReservasMes.Text;

                    // Obtener datos del edificio desde la base de datos
                    string nombreEdificio = "Consorcio General";
                    string ubicacionEdificio = "Sin especificar";
                    string cantDeptos = "-";
                    string cantPisos = "-";

                    DataTable dtEdificio = _reportesDatos.ObtenerDatosEdificio();
                    if (dtEdificio.Rows.Count > 0)
                    {
                        DataRow rowEdificio = dtEdificio.Rows[0];
                        nombreEdificio = rowEdificio["descripcion"]?.ToString() ?? "Consorcio General";
                        ubicacionEdificio = rowEdificio["ubicacion"]?.ToString() ?? "Sin especificar";
                        cantDeptos = rowEdificio["CantDepto"]?.ToString() ?? "-";
                        cantPisos = rowEdificio["cantPisos"]?.ToString() ?? "-";
                    }

                    string espacioMasOcupado = "Sin registros";
                    if (DgUsoEspacios.ItemsSource is DataView dvEspaciosTop && dvEspaciosTop.Table.Rows.Count > 0)
                    {
                        DataRow topEspacio = dvEspaciosTop.Table.Rows[0];
                        espacioMasOcupado = $"{topEspacio["Espacio"]} ({topEspacio["CantidadReservas"]} reservas)";
                    }

                    int sumaTotalReclamos = 0;
                    if (DgReclamosEstadisticas.ItemsSource is DataView dvRecTotal)
                    {
                        foreach (DataRowView rw in dvRecTotal)
                        {
                            if (rw["TotalReclamos"] != DBNull.Value)
                                sumaTotalReclamos += Convert.ToInt32(rw["TotalReclamos"]);
                        }
                    }

                    int sumaTotalReservas = 0;
                    if (DgUsoEspacios.ItemsSource is DataView dvResTotal)
                    {
                        foreach (DataRowView rw in dvResTotal)
                        {
                            if (rw["CantidadReservas"] != DBNull.Value)
                                sumaTotalReservas += Convert.ToInt32(rw["CantidadReservas"]);
                        }
                    }

                    Document.Create(container =>
                    {
                        container.Page(page =>
                        {
                            page.Size(PageSizes.A4);
                            page.Margin(1.5f, Unit.Centimetre);
                            page.PageColor(Colors.White);
                            page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken3));

                            // ENCABEZADO
                            page.Header().ShowOnce().Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("ConsorApp - Panel Directivo").Bold().FontSize(18).FontColor("#A09285");
                                    col.Item().Text("Informe Estadístico Ejecutivo e Indicadores Clave").FontSize(12).SemiBold();
                                    col.Item().Text($"Período analizado: Desde {periodoDesde} hasta {periodoHasta}").FontSize(9).FontColor(Colors.Grey.Medium);
                                });

                                row.ConstantItem(100).AlignRight().Text($"{DateTime.Now:dd/MM/yyyy}").FontSize(9);
                            });

                            // CONTENIDO PRINCIPAL
                            page.Content().PaddingVertical(10).Column(col =>
                            {
                                col.Spacing(12);

                                // DATOS DEL EDIFICIO
                                col.Item().Background("#F4F1EE").Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(cEdificio =>
                                {
                                    cEdificio.Spacing(4);
                                    cEdificio.Item().Text($"Edificio: {nombreEdificio}").Bold().FontSize(11).FontColor("#333333");
                                    cEdificio.Item().Text(t =>
                                    {
                                        t.Span("Ubicación: ").Bold().FontSize(9);
                                        t.Span($"{ubicacionEdificio}  |  ").FontSize(9);
                                        t.Span("Departamentos: ").Bold().FontSize(9);
                                        t.Span($"{cantDeptos}  |  ").FontSize(9);
                                        t.Span("Pisos: ").Bold().FontSize(9);
                                        t.Span($"{cantPisos}").FontSize(9);
                                    });
                                });

                                // 1. RESUMEN EJECUTIVO / KPIS
                                col.Item().Text("1. Resumen Ejecutivo de Indicadores").Bold().FontSize(12).FontColor("#333333");

                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                    });

                                    static IContainer HeaderStyle(IContainer container) => container.Background("#F4F1EE").Padding(6).AlignCenter();
                                    static IContainer CellStyle(IContainer container) => container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignCenter();

                                    table.Cell().Element(HeaderStyle).Text("Unidades Morosas").Bold().FontSize(9);
                                    table.Cell().Element(HeaderStyle).Text("Deuda Total").Bold().FontSize(9);
                                    table.Cell().Element(HeaderStyle).Text("Reclamos Pendientes").Bold().FontSize(9);
                                    table.Cell().Element(HeaderStyle).Text("Reservas Período").Bold().FontSize(9);

                                    table.Cell().Element(CellStyle).Text(totalMorosas).Bold().FontSize(12).FontColor("#B3413B");
                                    table.Cell().Element(CellStyle).Text(deudaTotal).Bold().FontSize(12);
                                    table.Cell().Element(CellStyle).Text(reclamosPendientes).Bold().FontSize(12).FontColor("#C8871E");
                                    table.Cell().Element(CellStyle).Text(reservasPeriodo).Bold().FontSize(12).FontColor("#3B7D4F");
                                });

                                col.Item().Background("#F9F9F9").Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Row(row =>
                                {
                                    row.RelativeItem().Text(t =>
                                    {
                                        t.Span("Espacio Común más solicitado en el período: ").Bold().FontSize(10);
                                        t.Span(espacioMasOcupado).FontSize(10).FontColor("#3B7D4F").Bold();
                                    });
                                });

                                // 2. DETALLE DE MOROSIDAD
                                col.Item().Text("2. Detalle de Unidades Morosas y Deudas").Bold().FontSize(12).FontColor("#333333");

                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(50);
                                        columns.RelativeColumn();
                                        columns.ConstantColumn(80);
                                        columns.ConstantColumn(60);
                                    });

                                    table.Cell().Background("#333333").Padding(5).Text("Unidad").FontColor(Colors.White).Bold().FontSize(9);
                                    table.Cell().Background("#333333").Padding(5).Text("Propietario").FontColor(Colors.White).Bold().FontSize(9);
                                    table.Cell().Background("#333333").Padding(5).Text("Deuda").FontColor(Colors.White).Bold().FontSize(9);
                                    table.Cell().Background("#333333").Padding(5).Text("Días").FontColor(Colors.White).Bold().FontSize(9);

                                    if (DgMorosidad.ItemsSource is DataView dvMorosidad)
                                    {
                                        foreach (DataRowView rowView in dvMorosidad)
                                        {
                                            DataRow row = rowView.Row;
                                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(row["Unidad"]?.ToString() ?? "").FontSize(9);
                                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(row["Propietario"]?.ToString() ?? "").FontSize(9);

                                            decimal deudaVal = row["Deuda"] != DBNull.Value ? Convert.ToDecimal(row["Deuda"]) : 0;
                                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(deudaVal.ToString("C")).FontSize(9);

                                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(row["DiasAtraso"]?.ToString() ?? "").FontSize(9);
                                        }
                                    }
                                });

                                // 3. RECLAMOS Y PORCENTAJES
                                col.Item().Text("3. Análisis de Reclamos y Quejas Frecuentes").Bold().FontSize(12).FontColor("#333333");

                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn();
                                        columns.ConstantColumn(50);
                                        columns.ConstantColumn(65);
                                        columns.ConstantColumn(65);
                                        columns.ConstantColumn(60);
                                    });

                                    table.Cell().Background("#333333").Padding(5).Text("Motivo del Reclamo").FontColor(Colors.White).Bold().FontSize(9);
                                    table.Cell().Background("#333333").Padding(5).Text("Total").FontColor(Colors.White).Bold().FontSize(9);
                                    table.Cell().Background("#333333").Padding(5).Text("Resueltos").FontColor(Colors.White).Bold().FontSize(9);
                                    table.Cell().Background("#333333").Padding(5).Text("Pendientes").FontColor(Colors.White).Bold().FontSize(9);
                                    table.Cell().Background("#333333").Padding(5).Text("Incidencia").FontColor(Colors.White).Bold().FontSize(9);

                                    if (DgReclamosEstadisticas.ItemsSource is DataView dvReclamos)
                                    {
                                        foreach (DataRowView rowView in dvReclamos)
                                        {
                                            DataRow row = rowView.Row;
                                            string motivo = row["Motivo"]?.ToString() ?? "";
                                            int total = row["TotalReclamos"] != DBNull.Value ? Convert.ToInt32(row["TotalReclamos"]) : 0;
                                            int resueltos = row["Resueltos"] != DBNull.Value ? Convert.ToInt32(row["Resueltos"]) : 0;
                                            int pendientes = row["Pendientes"] != DBNull.Value ? Convert.ToInt32(row["Pendientes"]) : 0;

                                            double porcentaje = sumaTotalReclamos > 0 ? ((double)total / sumaTotalReclamos) * 100 : 0;

                                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(motivo).FontSize(9);
                                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(total.ToString()).FontSize(9);
                                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(resueltos.ToString()).FontSize(9);
                                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(pendientes.ToString()).FontSize(9);
                                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text($"{porcentaje:F1}%").FontSize(9);
                                        }
                                    }
                                });

                                // 4. USO DE ESPACIOS COMUNES Y CANCELACIONES
                                col.Item().Text("4. Uso de Espacios Comunes y Cancelaciones").Bold().FontSize(12).FontColor("#333333");

                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn();
                                        columns.ConstantColumn(65);
                                        columns.ConstantColumn(65);
                                        columns.ConstantColumn(75);
                                    });

                                    table.Cell().Background("#333333").Padding(5).Text("Espacio Común").FontColor(Colors.White).Bold().FontSize(9);
                                    table.Cell().Background("#333333").Padding(5).Text("Reservas").FontColor(Colors.White).Bold().FontSize(9);
                                    table.Cell().Background("#333333").Padding(5).Text("Canceladas").FontColor(Colors.White).Bold().FontSize(9);
                                    table.Cell().Background("#333333").Padding(5).Text("% del Total").FontColor(Colors.White).Bold().FontSize(9);

                                    if (DgUsoEspacios.ItemsSource is DataView dvEspacios)
                                    {
                                        foreach (DataRowView rowView in dvEspacios)
                                        {
                                            DataRow row = rowView.Row;
                                            string espacio = row["Espacio"]?.ToString() ?? "";
                                            int reservas = row["CantidadReservas"] != DBNull.Value ? Convert.ToInt32(row["CantidadReservas"]) : 0;
                                            int canceladas = row["ReservasCanceladas"] != DBNull.Value ? Convert.ToInt32(row["ReservasCanceladas"]) : 0;

                                            double porcentajeEspacio = sumaTotalReservas > 0 ? ((double)reservas / sumaTotalReservas) * 100 : 0;

                                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(espacio).FontSize(9);
                                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(reservas.ToString()).FontSize(9);
                                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(canceladas.ToString()).FontSize(9);
                                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text($"{porcentajeEspacio:F1}%").FontSize(9);
                                        }
                                    }
                                });

                                col.Item().PaddingTop(10).Text(FormattableString.Invariant($"Informe generado automáticamente por ConsorApp para el período {periodoDesde} - {periodoHasta}.")).Italic().FontSize(8).FontColor(Colors.Grey.Medium);
                            });

                            // PIE DE PÁGINA
                            page.Footer().AlignCenter().Text(x =>
                            {
                                x.Span("Página ");
                                x.CurrentPageNumber();
                                x.Span(" de ");
                                x.TotalPages();
                            });
                        });
                    }).GeneratePdf(saveDialog.FileName);

                    MessageBox.Show("Informe PDF ejecutivo generado con éxito.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al generar el PDF: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}