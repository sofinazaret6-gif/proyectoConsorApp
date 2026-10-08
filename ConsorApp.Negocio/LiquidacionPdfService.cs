using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ConsorApp.Entidades;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ConsorApp.Negocio
{
    public class LiquidacionPdfService
    {
        private static readonly CultureInfo Cultura = new CultureInfo("es-AR");

        public LiquidacionPdfService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public void Generar(
            string rutaArchivo,
            Expensa expensa,
            List<HistorialExpensa> departamentos,
            IEnumerable<GastoEdificio> gastosPeriodo, int cantidadDepartamentos, string? nombreEdificio = null)
        {
            var gastos = gastosPeriodo
                .OrderBy(g => g.ConceptoGasto?.nombreConcepto)
                .ThenBy(g => g.fecha)
                .ToList();

            decimal totalListado = gastos.Sum(g => g.monto);

            Document.Create(documento =>
            {
                // Una página (o más, si hay muchos gastos) por departamento
                foreach (var depto in departamentos)
                {
                    documento.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        page.Header().Element(c => Encabezado(c, expensa, nombreEdificio));

                        page.Content().Element(c =>
                            Contenido(c, expensa, depto, gastos, totalListado, cantidadDepartamentos));

                        page.Footer().AlignCenter().Text(
                            $"ConsorApp - Generado el {DateTime.Now:dd/MM/yyyy HH:mm}")
                            .FontSize(8).FontColor(Colors.Grey.Darken1);
                    });
                }
            })
            .GeneratePdf(rutaArchivo);
        }

        // ---------------------------------------------------------
        // ENCABEZADO
        // ---------------------------------------------------------

        private static void Encabezado(IContainer contenedor, Expensa expensa, string? nombreEdificio)
        {
            contenedor.Column(col =>
            {
                col.Item().Row(fila =>
                {
                    fila.RelativeItem().Column(izq =>
                    {
                        if (!string.IsNullOrWhiteSpace(nombreEdificio))
                        {
                            izq.Item().Text("Edificio ")
                                .FontSize(11).SemiBold().FontColor(Colors.Grey.Darken2);
                            izq.Item().Text(nombreEdificio)
                                .FontSize(11).SemiBold().FontColor(Colors.Grey.Darken2);
                        }

                        izq.Item().Text("Liquidación de Expensas")
                            .FontSize(20).Bold().FontColor(Colors.Blue.Darken3);

                        izq.Item().Text($"Período {expensa.Periodo}")
                            .FontSize(12).SemiBold();
                    });

                    fila.ConstantItem(160).AlignRight().Text(
                        $"Fecha de emisión: {expensa.FechaEmision:dd/MM/yyyy}");
                });

                col.Item().PaddingTop(8)
                    .LineHorizontal(1).LineColor(Colors.Grey.Medium);
            });
        }
        public static string? ObtenerNombreEdificio()
        {
            try
            {
                EDIFICIO? edificio = new EdificioNegocio().ObtenerUnicoEdificio();

                // >>> Reemplazá AQUI_EL_NOMBRE por la propiedad del nombre en tu clase EDIFICIO <<<
                return edificio?.Descripcion;
            }
            catch
            {
                // Si falla la búsqueda, el PDF se genera igual, solo que sin el nombre
                return null;
            }
        }

        // ---------------------------------------------------------
        // CONTENIDO
        // ---------------------------------------------------------
        private static void Contenido(
            IContainer contenedor,
            Expensa expensa,
            HistorialExpensa depto,
            List<GastoEdificio> gastos,
            decimal totalListado,
            int cantidadDepartamentos)
        {
            contenedor.PaddingVertical(10).Column(col =>
            {
                col.Spacing(12);

                // ----- Datos de la unidad -----
                col.Item().Background(Colors.Grey.Lighten4).Padding(10).Column(info =>
                {
                    info.Spacing(3);

                    info.Item().Text("Datos de la unidad").Bold().FontSize(12);

                    info.Item().Text(t =>
                    {
                        t.Span("Departamento: ").SemiBold();
                        t.Span($"Piso {depto.Piso} - Unidad {depto.Unidad}");
                    });

                    info.Item().Text(t =>
                    {
                        t.Span("Propietario: ").SemiBold();
                        t.Span(depto.NombrePropietario);
                    });
                });

                // ----- Gastos del período -----
                col.Item().Text("Detalle de gastos del período").Bold().FontSize(12);

                if (gastos.Count == 0)
                {
                    col.Item().Text("No hay gastos registrados para este período.")
                        .Italic().FontColor(Colors.Grey.Darken1);
                }
                else
                {
                    col.Item().Table(tabla =>
                    {
                        tabla.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(62);   // Fecha
                            c.RelativeColumn(2);    // Concepto
                            c.RelativeColumn(3);    // Descripción
                            c.ConstantColumn(85);   // Monto
                        });

                        tabla.Header(h =>
                        {
                            h.Cell().Element(CeldaEncabezado).Text("Fecha");
                            h.Cell().Element(CeldaEncabezado).Text("Concepto");
                            h.Cell().Element(CeldaEncabezado).Text("Descripción");
                            h.Cell().Element(CeldaEncabezado).AlignRight().Text("Monto");
                        });

                        foreach (var g in gastos)
                        {
                            tabla.Cell().Element(Celda).Text(g.fecha.ToString("dd/MM/yyyy"));
                            tabla.Cell().Element(Celda).Text(g.ConceptoGasto?.nombreConcepto ?? "");
                            tabla.Cell().Element(Celda).Text(g.descripcion);
                            tabla.Cell().Element(Celda).AlignRight().Text(g.monto.ToString("C2", Cultura));
                        }
                    });

                    col.Item().AlignRight().Text(
                        $"Total de gastos: {totalListado.ToString("C2", Cultura)}")
                        .Bold();
                }

                // Aviso si los gastos actuales ya no coinciden con lo que se liquidó
                if (Math.Abs(totalListado - expensa.MontoTotalGastos) > 0.01m)
                {
                    col.Item().Text(
                        "Nota: los gastos actuales del período no coinciden con el total liquidado " +
                        $"({expensa.MontoTotalGastos.ToString("C2", Cultura)}). " +
                        "Puede haberse anulado o modificado un gasto después de la liquidación.")
                        .FontSize(8).Italic().FontColor(Colors.Red.Darken2);
                }

                // ----- Resumen de la liquidación -----
                col.Item().Border(1).BorderColor(Colors.Grey.Medium).Padding(10).Column(r =>
                {
                    r.Spacing(4);

                    r.Item().Text("Resumen de la liquidación").Bold().FontSize(12);

                    Fila(r, "Total de gastos liquidado", expensa.MontoTotalGastos.ToString("C2", Cultura));
                    Fila(r, "Cantidad de departamentos", cantidadDepartamentos.ToString());
                    Fila(r, "Expensa individual", expensa.ValorExpensaIndividual.ToString("C2", Cultura));
                    Fila(r, "Fecha de vencimiento", depto.FechaVencimiento.ToString("dd/MM/yyyy"));
                    bool pagado = string.Equals(depto.EstadoPago, "Pagado", StringComparison.OrdinalIgnoreCase);
                    bool atrasado = !pagado && depto.FechaVencimiento.Date < DateTime.Today;

                    string textoEstado = pagado ? "PAGADO" : atrasado ? "ATRASADO" : "PENDIENTE";
                    var colorEstado = pagado ? Colors.Green.Darken3
                                    : atrasado ? Colors.Red.Darken2
                                    : Colors.Orange.Darken2;

                    r.Item().Row(fila =>
                    {
                        fila.RelativeItem().Text("Estado de pago");
                        fila.ConstantItem(150).AlignRight().Text(textoEstado).Bold().FontColor(colorEstado);
                    });

                    if (depto.FechaPago.HasValue)
                        Fila(r, "Fecha de pago", depto.FechaPago.Value.ToString("dd/MM/yyyy"));

                    if (!string.IsNullOrWhiteSpace(depto.MetodoPago))
                        Fila(r, "Método de pago", depto.MetodoPago);

                    r.Item().PaddingVertical(4)
                        .LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);

                    r.Item().Row(fila =>
                    {
                        fila.RelativeItem().Text(pagado ? "MONTO PAGADO" : "MONTO A PAGAR")
                            .Bold().FontSize(13);

                        fila.ConstantItem(150).AlignRight()
                            .Text(depto.MontoAPagar.ToString("C2", Cultura))
                            .Bold().FontSize(14).FontColor(Colors.Green.Darken3);
                    });
                });
            });
        }

        // ---------------------------------------------------------
        // AYUDAS DE FORMATO
        // ---------------------------------------------------------
        private static void Fila(ColumnDescriptor col, string etiqueta, string valor)
        {
            col.Item().Row(fila =>
            {
                fila.RelativeItem().Text(etiqueta);
                fila.ConstantItem(150).AlignRight().Text(valor).SemiBold();
            });
        }

        private static IContainer CeldaEncabezado(IContainer c) =>
            c.Background(Colors.Blue.Darken3)
             .Padding(5)
             .DefaultTextStyle(x => x.FontColor(Colors.White).SemiBold());

        private static IContainer Celda(IContainer c) =>
            c.BorderBottom(0.5f)
             .BorderColor(Colors.Grey.Lighten1)
             .Padding(5);

        // ---------------------------------------------------------
        // RESUMEN GENERAL: UNA SOLA HOJA CON TODOS LOS DEPARTAMENTOS
        // ---------------------------------------------------------
        public void GenerarResumenGeneral(
            string rutaArchivo,
            Expensa expensa,
            List<HistorialExpensa> departamentos,
            IEnumerable<GastoEdificio> gastosPeriodo,
            string? nombreEdificio = null)
        {
            var gastos = gastosPeriodo
                .OrderBy(g => g.ConceptoGasto?.nombreConcepto)
                .ThenBy(g => g.fecha)
                .ToList();

            decimal totalListado = gastos.Sum(g => g.monto);

            Document.Create(documento =>
            {
                documento.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Element(c => Encabezado(c, expensa, nombreEdificio));

                    page.Content().Element(c =>
                        ContenidoResumen(c, expensa, departamentos, gastos, totalListado));

                    page.Footer().AlignCenter().Text(
                        $"ConsorApp - Generado el {DateTime.Now:dd/MM/yyyy HH:mm}")
                        .FontSize(8).FontColor(Colors.Grey.Darken1);
                });
            })
            .GeneratePdf(rutaArchivo);
        }

        private static bool EstaPagado(HistorialExpensa d) =>
            string.Equals(d.EstadoPago, "Pagado", StringComparison.OrdinalIgnoreCase);

        private static void ContenidoResumen(
            IContainer contenedor,
            Expensa expensa,
            List<HistorialExpensa> departamentos,
            List<GastoEdificio> gastos,
            decimal totalListado)
        {
            var ordenados = departamentos
                .OrderBy(d => d.Piso)
                .ThenBy(d => d.Unidad)
                .ToList();

            decimal totalACobrar = ordenados.Sum(d => d.MontoAPagar);
            decimal cobrado = ordenados.Where(EstaPagado).Sum(d => d.MontoAPagar);
            decimal pendiente = totalACobrar - cobrado;

            contenedor.PaddingVertical(8).Column(col =>
            {
                col.Spacing(10);

                // ===== TABLA 1: DEPARTAMENTOS Y PROPIETARIOS =====
                col.Item().Text($"Liquidación por departamento ({ordenados.Count})")
                    .Bold().FontSize(12);

                col.Item().Table(tabla =>
                {
                    tabla.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(70);   // Departamento
                        c.RelativeColumn();     // Propietario
                        c.ConstantColumn(80);   // Monto
                        c.ConstantColumn(62);   // Vencimiento
                        c.ConstantColumn(65);   // Estado
                        c.ConstantColumn(62);   // Fecha de pago
                    });

                    tabla.Header(h =>
                    {
                        h.Cell().Element(CeldaEncabezado).Text("Depto.");
                        h.Cell().Element(CeldaEncabezado).Text("Propietario");
                        h.Cell().Element(CeldaEncabezado).AlignRight().Text("A pagar");
                        h.Cell().Element(CeldaEncabezado).Text("Vence");
                        h.Cell().Element(CeldaEncabezado).Text("Estado");
                        h.Cell().Element(CeldaEncabezado).Text("Fecha pago");
                    });

                    foreach (var d in ordenados)
                    {
                        bool pagado = EstaPagado(d);
                        bool atrasado = !pagado && d.FechaVencimiento.Date < DateTime.Today;

                        string textoEstado = pagado ? "PAGADO" : atrasado ? "ATRASADO" : "PENDIENTE";
                        var colorEstado = pagado ? Colors.Green.Darken3
                                        : atrasado ? Colors.Red.Darken2
                                        : Colors.Orange.Darken2;

                        tabla.Cell().Element(Celda).Text($"Piso {d.Piso} - {d.Unidad}");
                        tabla.Cell().Element(Celda).Text(d.NombrePropietario);
                        tabla.Cell().Element(Celda).AlignRight().Text(d.MontoAPagar.ToString("C2", Cultura));
                        tabla.Cell().Element(Celda).Text(d.FechaVencimiento.ToString("dd/MM/yyyy"));
                        tabla.Cell().Element(Celda).Text(textoEstado).Bold().FontColor(colorEstado);
                        tabla.Cell().Element(Celda).Text(
                            d.FechaPago.HasValue ? d.FechaPago.Value.ToString("dd/MM/yyyy") : "-");
                    }
                });

                // ===== RESUMEN DE TOTALES =====
                col.Item().AlignRight().Width(260).Column(r =>
                {
                    r.Spacing(2);

                    Fila(r, "Total de gastos liquidado", expensa.MontoTotalGastos.ToString("C2", Cultura));
                    Fila(r, "Expensa individual", expensa.ValorExpensaIndividual.ToString("C2", Cultura));
                    Fila(r, "Cobrado", cobrado.ToString("C2", Cultura));
                    Fila(r, "Pendiente de cobro", pendiente.ToString("C2", Cultura));
                });

                // ===== TABLA 2: GASTOS DEL PERÍODO =====
                col.Item().PaddingTop(6).Text("Gastos del período").Bold().FontSize(12);

                if (gastos.Count == 0)
                {
                    col.Item().Text("No hay gastos registrados para este período.")
                        .Italic().FontColor(Colors.Grey.Darken1);
                }
                else
                {
                    col.Item().Table(tabla =>
                    {
                        tabla.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(62);   // Fecha
                            c.RelativeColumn(2);    // Concepto
                            c.RelativeColumn(3);    // Descripción
                            c.ConstantColumn(85);   // Monto
                        });

                        tabla.Header(h =>
                        {
                            h.Cell().Element(CeldaEncabezado).Text("Fecha");
                            h.Cell().Element(CeldaEncabezado).Text("Concepto");
                            h.Cell().Element(CeldaEncabezado).Text("Descripción");
                            h.Cell().Element(CeldaEncabezado).AlignRight().Text("Monto");
                        });

                        foreach (var g in gastos)
                        {
                            tabla.Cell().Element(Celda).Text(g.fecha.ToString("dd/MM/yyyy"));
                            tabla.Cell().Element(Celda).Text(g.ConceptoGasto?.nombreConcepto ?? "");
                            tabla.Cell().Element(Celda).Text(g.descripcion);
                            tabla.Cell().Element(Celda).AlignRight().Text(g.monto.ToString("C2", Cultura));
                        }
                    });

                    col.Item().AlignRight().Text(
                        $"Total de gastos: {totalListado.ToString("C2", Cultura)}").Bold();
                }

                // Aviso si los gastos actuales ya no coinciden con lo liquidado
                if (Math.Abs(totalListado - expensa.MontoTotalGastos) > 0.01m)
                {
                    col.Item().Text(
                        "Nota: los gastos actuales del período no coinciden con el total liquidado " +
                        $"({expensa.MontoTotalGastos.ToString("C2", Cultura)}). " +
                        "Puede haberse anulado o modificado un gasto después de la liquidación.")
                        .FontSize(8).Italic().FontColor(Colors.Red.Darken2);
                }
            });
        }
    }


}