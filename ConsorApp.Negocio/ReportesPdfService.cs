using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using ConsorApp.Negocio.Modelos;
using QuestPDF.Fluent;
using ConsorApp.Datos;
using ConsorApp.Entidades;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ConsorApp.Negocio
{
    /// <summary>
    /// Genera un PDF independiente por cada reporte:
    /// Morosidad, Reclamos, Uso de espacios comunes y Gastos por concepto.
    /// Todos llevan el nombre del edificio, importes en pesos argentinos y gráfico de torta.
    /// </summary>
    public class ReportesPdfService
    {
        private static readonly CultureInfo Ar = new CultureInfo("es-AR");

        private const string ColorMarca = "#A09285";
        private const string ColorOtros = "#C9C9C9";
        private static readonly string[] Paleta =
            { "#A09285", "#3B7D4F", "#C8871E", "#B3413B", "#4A7FB5", "#7A5C8E" };

        static ReportesPdfService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        // Pesos argentinos: $ 1.234.567,89  (nunca euros ni símbolo genérico)
        public static string Peso(decimal valor) => valor.ToString("C2", Ar);
        private static string Peso0(decimal valor) => valor.ToString("C0", Ar);
        private static string Pct(double valor) => valor.ToString("F1", Ar) + " %";
        private static string Fecha(DateTime f) => f.ToString("dd/MM/yyyy", Ar);

        // =====================================================================
        //  1) MOROSIDAD Y DEUDAS
        // =====================================================================
        public void GenerarMorosidad(string ruta, DatosEdificio edificio,
                                     DateTime desde, DateTime hasta, DataTable dt)
        {
            var filas = dt.Rows.Cast<DataRow>()
                .Select(r => new
                {
                    Unidad = Str(r, "Unidad"),
                    Propietario = Str(r, "Propietario"),
                    Deuda = Dec(r, "Deuda"),
                    Dias = Int(r, "DiasAtraso")
                })
                .OrderByDescending(x => x.Deuda)
                .ToList();

            int morosas = filas.Count;
            decimal total = filas.Sum(x => x.Deuda);
            decimal promedio = morosas > 0 ? total / morosas : 0;
            int mayorAtraso = morosas > 0 ? filas.Max(x => x.Dias) : 0;

            Armar(ruta, edificio,
                "Reporte de Morosidad y Deudas",
                $"Vencimientos entre {Fecha(desde)} y {Fecha(hasta)}  ·  atraso calculado al {Fecha(hasta.Date > DateTime.Today ? DateTime.Today : hasta.Date)}",
                col =>
                {
                    Tarjetas(col,
                        ("Unidades morosas", morosas.ToString(Ar), "#B3413B"),
                        ("Deuda total", Peso(total), "#333333"),
                        ("Deuda promedio por unidad", Peso(promedio), "#333333"),
                        ("Mayor atraso", $"{mayorAtraso} días", "#C8871E"));

                    if (morosas == 0)
                    {
                        col.Item().Text("No hay unidades con expensas impagas y vencidas en el rango seleccionado.")
                            .Italic().FontColor(Colors.Grey.Darken1);
                        return;
                    }

                    var porciones = Agrupar(filas.Select(x =>
                        ($"Unidad {x.Unidad} · {x.Propietario}", (double)x.Deuda)));

                    Torta(col, "Distribución de la deuda por unidad", porciones,
                        Peso0(total), "deuda total", v => Peso((decimal)v));

                    col.Item().Text("Detalle por unidad").Bold().FontSize(12).FontColor("#333333");

                    Tabla(col,
                        new[] { "Unidad", "Propietario", "Deuda", "% del total", "Días de atraso" },
                        new[] { 1f, 3f, 2f, 1.5f, 1.5f },
                        new[] { 2, 3, 4 },
                        filas.Select(x => new[]
                        {
                            x.Unidad,
                            x.Propietario,
                            Peso(x.Deuda),
                            Pct(total > 0 ? (double)(x.Deuda / total * 100) : 0),
                            x.Dias.ToString(Ar)
                        }).ToList(),
                        new[] { "TOTAL", $"{morosas} unidades", Peso(total), Pct(100), "" });
                });
        }

        // =====================================================================
        //  2) RECLAMOS POR CATEGORÍA
        // =====================================================================
        public void GenerarReclamos(string ruta, DatosEdificio edificio,
                                    DateTime desde, DateTime hasta, DataTable dt)
        {
            var filas = dt.Rows.Cast<DataRow>()
                .Select(r => new
                {
                    Categoria = Str(r, "Categoria"),
                    Total = Int(r, "TotalReclamos"),
                    Resueltos = Int(r, "Resueltos"),
                    Pendientes = Int(r, "Pendientes")
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            int total = filas.Sum(x => x.Total);
            int resueltos = filas.Sum(x => x.Resueltos);
            int pendientes = filas.Sum(x => x.Pendientes);
            double pctResueltos = total > 0 ? resueltos * 100.0 / total : 0;

            Armar(ruta, edificio,
                "Reporte de Reclamos por Categoría",
                $"Período: {Fecha(desde)} al {Fecha(hasta)}",
                col =>
                {
                    Tarjetas(col,
                        ("Total de reclamos", total.ToString(Ar), "#333333"),
                        ("Resueltos", resueltos.ToString(Ar), "#3B7D4F"),
                        ("Pendientes", pendientes.ToString(Ar), "#C8871E"),
                        ("% resueltos", Pct(pctResueltos), "#3B7D4F"));

                    if (total == 0)
                    {
                        col.Item().Text("No se registraron reclamos en el período seleccionado.")
                            .Italic().FontColor(Colors.Grey.Darken1);
                        return;
                    }

                    var porciones = Agrupar(filas.Select(x => (x.Categoria, (double)x.Total)));

                    Torta(col, "Reclamos por categoría", porciones,
                        total.ToString(Ar), "reclamos", v => v.ToString("N0", Ar));

                    col.Item().Text("Detalle por categoría").Bold().FontSize(12).FontColor("#333333");

                    Tabla(col,
                        new[] { "Categoría", "Total", "Resueltos", "Pendientes", "% del total" },
                        new[] { 4f, 1.2f, 1.5f, 1.5f, 1.6f },
                        new[] { 1, 2, 3, 4 },
                        filas.Select(x => new[]
                        {
                            x.Categoria,
                            x.Total.ToString(Ar),
                            x.Resueltos.ToString(Ar),
                            x.Pendientes.ToString(Ar),
                            Pct(total > 0 ? x.Total * 100.0 / total : 0)
                        }).ToList(),
                        new[] { "TOTAL", total.ToString(Ar), resueltos.ToString(Ar),
                                pendientes.ToString(Ar), Pct(100) });
                });
        }

        // =====================================================================
        //  3) USO DE ESPACIOS COMUNES
        // =====================================================================
        public void GenerarEspacios(string ruta, DatosEdificio edificio,
                                    DateTime desde, DateTime hasta, DataTable dt,
                                    DataTable? dtFranjas = null, DataTable? dtEspacioTurno = null)
        {
            var filas = dt.Rows.Cast<DataRow>()
                .Select(r => new
                {
                    Espacio = Str(r, "Espacio"),
                    Reservas = Int(r, "CantidadReservas"),
                    Canceladas = Int(r, "ReservasCanceladas")
                })
                .OrderByDescending(x => x.Reservas)
                .ToList();

            int total = filas.Sum(x => x.Reservas);
            int canceladas = filas.Sum(x => x.Canceladas);
            double pctCancel = total + canceladas > 0 ? canceladas * 100.0 / (total + canceladas) : 0;
            string masUsado = filas.Count > 0 && filas[0].Reservas > 0 ? filas[0].Espacio : "Sin registros";

            // Turnos (Mañana / Tarde / Noche)
            var turnos = (dtFranjas == null ? new List<DataRow>() : dtFranjas.Rows.Cast<DataRow>().ToList())
                .Select(r => new
                {
                    Nombre = Str(r, "Franja"),
                    Horario = Str(r, "Horario"),
                    Cantidad = Int(r, "CantidadReservas")
                })
                .ToList();
            int totalTurnos = turnos.Sum(x => x.Cantidad);
            var turnoTop = turnos.OrderByDescending(x => x.Cantidad).FirstOrDefault();
            string turnoMasPedido = turnoTop != null && turnoTop.Cantidad > 0 ? turnoTop.Nombre : "Sin registros";

            Armar(ruta, edificio,
                "Reporte de Uso de Espacios Comunes",
                $"Período: {Fecha(desde)} al {Fecha(hasta)}",
                col =>
                {
                    Tarjetas(col,
                        ("Reservas del período", total.ToString(Ar), "#3B7D4F"),
                        ("Espacio más reservado", masUsado, "#333333"),
                        ("Turno más pedido", turnoMasPedido, "#4A7FB5"),
                        ("Canceladas", $"{canceladas} ({Pct(pctCancel)})", "#B3413B"));

                    if (total == 0)
                    {
                        col.Item().Text("No se registraron reservas en el período seleccionado.")
                            .Italic().FontColor(Colors.Grey.Darken1);
                        return;
                    }

                    var porciones = Agrupar(filas.Select(x => (x.Espacio, (double)x.Reservas)));

                    Torta(col, "Reservas por espacio común", porciones,
                        total.ToString(Ar), "reservas", v => v.ToString("N0", Ar));

                    col.Item().Text("Detalle por espacio").Bold().FontSize(12).FontColor("#333333");

                    Tabla(col,
                        new[] { "Espacio común", "Reservas", "Canceladas", "% del total" },
                        new[] { 4f, 1.5f, 1.6f, 1.6f },
                        new[] { 1, 2, 3 },
                        filas.Select(x => new[]
                        {
                            x.Espacio,
                            x.Reservas.ToString(Ar),
                            x.Canceladas.ToString(Ar),
                            Pct(total > 0 ? x.Reservas * 100.0 / total : 0)
                        }).ToList(),
                        new[] { "TOTAL", total.ToString(Ar), canceladas.ToString(Ar), Pct(100) });

                    // ---- ¿En qué turnos? ----
                    if (turnos.Count > 0)
                    {
                        col.Item().Text("Reservas por turno").Bold().FontSize(12).FontColor("#333333");

                        Tabla(col,
                            new[] { "Turno", "Horario", "Reservas", "% del total" },
                            new[] { 2f, 2f, 1.5f, 1.5f },
                            new[] { 2, 3 },
                            turnos.Select(x => new[]
                            {
                                x.Nombre,
                                x.Horario,
                                x.Cantidad.ToString(Ar),
                                Pct(totalTurnos > 0 ? x.Cantidad * 100.0 / totalTurnos : 0)
                            }).ToList(),
                            new[] { "TOTAL", "", totalTurnos.ToString(Ar), Pct(100) });
                    }

                    // ---- Cruce espacio x turno ----
                    if (dtEspacioTurno != null && dtEspacioTurno.Rows.Count > 0 && turnos.Count > 0)
                    {
                        var cruce = dtEspacioTurno.Rows.Cast<DataRow>()
                            .GroupBy(r => (Str(r, "Espacio"), Str(r, "Turno")))
                            .ToDictionary(g => g.Key, g => g.Sum(r => Int(r, "CantidadReservas")));

                        var nombresEspacios = dtEspacioTurno.Rows.Cast<DataRow>()
                            .Select(r => Str(r, "Espacio")).Distinct().ToList();

                        var encabezados = new List<string> { "Espacio" };
                        encabezados.AddRange(turnos.Select(tn => tn.Nombre));
                        encabezados.Add("Total");

                        var anchos = new List<float> { 3f };
                        anchos.AddRange(turnos.Select(tn => 1.3f));
                        anchos.Add(1.2f);

                        var filasCruce = new List<string[]>();
                        foreach (string esp in nombresEspacios)
                        {
                            var fila = new List<string> { esp };
                            int sumaEspacio = 0;
                            foreach (var tn in turnos)
                            {
                                cruce.TryGetValue((esp, tn.Nombre), out int cant);
                                fila.Add(cant.ToString(Ar));
                                sumaEspacio += cant;
                            }
                            fila.Add(sumaEspacio.ToString(Ar));
                            filasCruce.Add(fila.ToArray());
                        }

                        var totalesCruce = new List<string> { "TOTAL" };
                        foreach (var tn in turnos)
                            totalesCruce.Add(cruce.Where(kv => kv.Key.Item2 == tn.Nombre).Sum(kv => kv.Value).ToString(Ar));
                        totalesCruce.Add(cruce.Values.Sum().ToString(Ar));

                        col.Item().Text("Reservas por espacio y turno").Bold().FontSize(12).FontColor("#333333");

                        Tabla(col,
                            encabezados.ToArray(),
                            anchos.ToArray(),
                            Enumerable.Range(1, turnos.Count + 1).ToArray(),
                            filasCruce,
                            totalesCruce.ToArray());
                    }
                });
        }

        // =====================================================================
        //  4) GASTOS POR CONCEPTO (en qué se va la plata)
        // =====================================================================
        public void GenerarGastos(string ruta, DatosEdificio edificio,
                                  DateTime desde, DateTime hasta, DataTable dt)
        {
            var filas = dt.Rows.Cast<DataRow>()
                .Select(r => new
                {
                    Concepto = Str(r, "Concepto"),
                    Cantidad = Int(r, "CantidadGastos"),
                    Total = Dec(r, "TotalGastado")
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            decimal total = filas.Sum(x => x.Total);
            int cantidad = filas.Sum(x => x.Cantidad);
            var mayor = filas.FirstOrDefault();
            double pctMayor = mayor != null && total > 0 ? (double)(mayor.Total / total * 100) : 0;

            Armar(ruta, edificio,
                "Reporte de Gastos por Concepto",
                $"Período: {Fecha(desde)} al {Fecha(hasta)}",
                col =>
                {
                    Tarjetas(col,
                        ("Total gastado", Peso(total), "#333333"),
                        ("Cantidad de gastos", cantidad.ToString(Ar), "#4A7FB5"),
                        ("Concepto más caro", mayor != null && total > 0 ? mayor.Concepto : "Sin registros", "#B3413B"),
                        ("% del total", Pct(pctMayor), "#C8871E"));

                    if (total <= 0)
                    {
                        col.Item().Text("No se registraron gastos en el período seleccionado.")
                            .Italic().FontColor(Colors.Grey.Darken1);
                        return;
                    }

                    var porciones = Agrupar(filas.Select(x => (x.Concepto, (double)x.Total)));

                    Torta(col, "¿En qué se va la plata?", porciones,
                        Peso0(total), "gastado", v => Peso((decimal)v));

                    col.Item().Text("Ranking de conceptos más caros").Bold().FontSize(12).FontColor("#333333");

                    Tabla(col,
                        new[] { "#", "Concepto", "Cant.", "Total", "% del total", "Promedio por gasto" },
                        new[] { 0.7f, 3.5f, 1f, 2f, 1.4f, 2f },
                        new[] { 2, 3, 4, 5 },
                        filas.Select((x, i) => new[]
                        {
                            (i + 1).ToString(Ar),
                            x.Concepto,
                            x.Cantidad.ToString(Ar),
                            Peso(x.Total),
                            Pct(total > 0 ? (double)(x.Total / total * 100) : 0),
                            Peso(x.Cantidad > 0 ? x.Total / x.Cantidad : 0)
                        }).ToList(),
                        new[] { "", "TOTAL", cantidad.ToString(Ar), Peso(total), Pct(100), "" });
                });
        }

        // =====================================================================
        //  PLANTILLA COMÚN (encabezado con el edificio, pie, página A4)
        // =====================================================================
        private static void Armar(string ruta, DatosEdificio edificio, string titulo,
                                  string subtitulo, Action<ColumnDescriptor> contenido)
        {
            Document.Create(doc => doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken3));

                page.Header().Element(c => Encabezado(c, edificio, titulo, subtitulo));

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Spacing(12);
                    contenido(col);
                });

                page.Footer().Element(c => Pie(c, edificio));
            })).GeneratePdf(ruta);
        }

        private static void Encabezado(IContainer c, DatosEdificio ed, string titulo, string subtitulo)
        {
            c.PaddingBottom(8).BorderBottom(1.5f).BorderColor(ColorMarca).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(ed.Nombre).Bold().FontSize(18).FontColor(ColorMarca);
                    col.Item().Text($"{ed.Ubicacion}  ·  {ed.CantDeptos} departamentos  ·  {ed.CantPisos} pisos")
                        .FontSize(8).FontColor(Colors.Grey.Medium);
                    col.Item().PaddingTop(4).Text(titulo).SemiBold().FontSize(13).FontColor("#333333");
                    col.Item().Text(subtitulo).FontSize(9).FontColor(Colors.Grey.Darken1);
                });

                row.ConstantItem(120).AlignRight().Text(DateTime.Now.ToString("dd/MM/yyyy HH:mm", Ar))
                    .FontSize(8).FontColor(Colors.Grey.Medium);
            });
        }

        private static void Pie(IContainer c, DatosEdificio ed)
        {
            c.Row(row =>
            {
                row.RelativeItem().Text($"ConsorApp  ·  {ed.Nombre}")
                    .FontSize(8).FontColor(Colors.Grey.Medium);

                row.RelativeItem().AlignRight().DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Medium))
                    .Text(t =>
                    {
                        t.Span("Página ");
                        t.CurrentPageNumber();
                        t.Span(" de ");
                        t.TotalPages();
                    });
            });
        }

        // =====================================================================
        //  COMPONENTES
        // =====================================================================
        private static void Tarjetas(ColumnDescriptor col, params (string Titulo, string Valor, string Color)[] items)
        {
            col.Item().Row(row =>
            {
                foreach (var it in items)
                {
                    row.RelativeItem().Padding(3).Background("#F4F1EE").Padding(8).Column(c =>
                    {
                        c.Item().Text(it.Titulo).FontSize(8).FontColor(Colors.Grey.Darken1);
                        c.Item().Text(it.Valor).Bold().FontSize(13).FontColor(it.Color);
                    });
                }
            });
        }

        private static void Torta(ColumnDescriptor col, string titulo,
                                  List<(string Etiqueta, double Valor)> porciones,
                                  string centro, string subCentro, Func<double, string> formatoValor)
        {
            col.Item().Text(titulo).Bold().FontSize(12).FontColor("#333333");

            double total = porciones.Sum(p => p.Valor);
            if (total <= 0)
            {
                col.Item().Text("Sin datos para graficar.").Italic().FontColor(Colors.Grey.Medium);
                return;
            }

            var conColor = porciones
                .Select((p, i) => (p.Etiqueta, p.Valor,
                                   Hex: p.Etiqueta == "Otros" ? ColorOtros : Paleta[i % Paleta.Length]))
                .ToList();

            byte[] png = TortaRenderer.Generar(
                conColor.Select(p => (p.Valor, p.Hex)).ToList(), centro, subCentro);

            col.Item().ShowEntire().Row(row =>
            {
                row.ConstantItem(190).Height(190).Image(png);

                row.RelativeItem().PaddingLeft(16).AlignMiddle().Column(leyenda =>
                {
                    leyenda.Spacing(5);
                    foreach (var p in conColor)
                    {
                        leyenda.Item().Row(r =>
                        {
                            r.ConstantItem(10).PaddingTop(2).Height(10).Background(p.Hex);
                            r.RelativeItem().PaddingLeft(6).Text(t =>
                            {
                                t.Span(p.Etiqueta).FontSize(9);
                                t.Span($"   {formatoValor(p.Valor)}  ({Pct(p.Valor / total * 100)})")
                                    .FontSize(9).Bold();
                            });
                        });
                    }
                });
            });
        }

        private static void Tabla(ColumnDescriptor col, string[] encabezados, float[] anchos,
                                  int[] columnasDerecha, List<string[]> filas, string[]? totales)
        {
            col.Item().Table(tabla =>
            {
                tabla.ColumnsDefinition(cd =>
                {
                    foreach (var a in anchos) cd.RelativeColumn(a);
                });

                tabla.Header(h =>
                {
                    for (int i = 0; i < encabezados.Length; i++)
                    {
                        var celda = h.Cell().Background("#333333").Padding(5);
                        if (columnasDerecha.Contains(i)) celda = celda.AlignRight();
                        celda.Text(encabezados[i]).FontColor(Colors.White).Bold().FontSize(9);
                    }
                });

                for (int f = 0; f < filas.Count; f++)
                {
                    string fondo = f % 2 == 0 ? "#FFFFFF" : "#F9F7F5";
                    for (int i = 0; i < encabezados.Length; i++)
                    {
                        var celda = tabla.Cell().Background(fondo)
                            .BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5);
                        if (columnasDerecha.Contains(i)) celda = celda.AlignRight();
                        celda.Text(filas[f][i]).FontSize(9);
                    }
                }

                if (totales != null)
                {
                    for (int i = 0; i < encabezados.Length; i++)
                    {
                        var celda = tabla.Cell().Background("#F4F1EE")
                            .BorderTop(1).BorderColor(ColorMarca).Padding(5);
                        if (columnasDerecha.Contains(i)) celda = celda.AlignRight();
                        celda.Text(totales[i]).Bold().FontSize(9);
                    }
                }
            });
        }

        /// <summary>Deja las 6 porciones más grandes y suma el resto en "Otros".</summary>
        private static List<(string Etiqueta, double Valor)> Agrupar(
            IEnumerable<(string Etiqueta, double Valor)> datos, int maximo = 6)
        {
            var ordenadas = datos.Where(d => d.Valor > 0).OrderByDescending(d => d.Valor).ToList();
            if (ordenadas.Count <= maximo) return ordenadas;

            var resultado = ordenadas.Take(maximo).ToList();
            resultado.Add(("Otros", ordenadas.Skip(maximo).Sum(d => d.Valor)));
            return resultado;
        }

        // ---- Lectura segura de DataRow (aguanta NULL y columnas que no existan) ----
        private static string Str(DataRow r, string c) =>
            r.Table.Columns.Contains(c) && r[c] != DBNull.Value ? r[c].ToString() ?? "" : "";

        private static decimal Dec(DataRow r, string c) =>
            r.Table.Columns.Contains(c) && r[c] != DBNull.Value ? Convert.ToDecimal(r[c]) : 0m;

        private static int Int(DataRow r, string c) =>
            r.Table.Columns.Contains(c) && r[c] != DBNull.Value ? Convert.ToInt32(r[c]) : 0;
    }
}