using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ConsorApp.Negocio
{
    /// <summary>
    /// Dibuja un gráfico de torta tipo "dona" como PNG usando WPF (sin librerías extra).
    /// Debe llamarse desde el hilo de la interfaz (el click de un botón sirve).
    /// </summary>
    public static class TortaRenderer
    {
        public static byte[] Generar(
            IReadOnlyList<(double Valor, string ColorHex)> porciones,
            string textoCentro = "",
            string subtextoCentro = "",
            int lado = 320)
        {
            double total = porciones.Sum(p => p.Valor);
            var visual = new DrawingVisual();

            using (DrawingContext dc = visual.RenderOpen())
            {
                dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, lado, lado));

                double centro = lado / 2.0;
                double radio = centro - 8;
                double radioHueco = radio * 0.58;
                var puntoCentro = new Point(centro, centro);

                var separador = new Pen(Brushes.White, 2);
                separador.Freeze();

                double inicio = -90; // arranca arriba
                foreach (var p in porciones)
                {
                    if (p.Valor <= 0 || total <= 0) continue;

                    double barrido = p.Valor / total * 360.0;
                    Brush pincel = Pincel(p.ColorHex);

                    if (barrido >= 359.9)
                    {
                        // Una sola porción: círculo completo
                        dc.DrawEllipse(pincel, null, puntoCentro, radio, radio);
                    }
                    else
                    {
                        Point a = PuntoEn(puntoCentro, radio, inicio);
                        Point b = PuntoEn(puntoCentro, radio, inicio + barrido);

                        var figura = new PathFigure { StartPoint = puntoCentro, IsClosed = true };
                        figura.Segments.Add(new LineSegment(a, true));
                        figura.Segments.Add(new ArcSegment(b, new Size(radio, radio), 0,
                            barrido > 180, SweepDirection.Clockwise, true));

                        var geometria = new PathGeometry(new[] { figura });
                        geometria.Freeze();
                        dc.DrawGeometry(pincel, separador, geometria);
                    }

                    inicio += barrido;
                }

                // Hueco de la dona
                dc.DrawEllipse(Brushes.White, null, puntoCentro, radioHueco, radioHueco);

                // Texto central
                bool haySub = !string.IsNullOrWhiteSpace(subtextoCentro);
                Texto(dc, textoCentro, 18, true, "#333333", centro, haySub ? centro - 24 : centro - 14);
                if (haySub)
                    Texto(dc, subtextoCentro, 11, false, "#777777", centro, centro + 4);
            }

            // Se renderiza al doble de resolución para que se vea nítido en el PDF
            const double dpi = 192;
            int px = (int)(lado * dpi / 96.0);
            var bitmap = new RenderTargetBitmap(px, px, dpi, dpi, PixelFormats.Pbgra32);
            bitmap.Render(visual);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using var ms = new MemoryStream();
            encoder.Save(ms);
            return ms.ToArray();
        }

        private static Point PuntoEn(Point centro, double radio, double gradosAngulo)
        {
            double rad = gradosAngulo * Math.PI / 180.0;
            return new Point(centro.X + radio * Math.Cos(rad), centro.Y + radio * Math.Sin(rad));
        }

        private static void Texto(DrawingContext dc, string texto, double tamano, bool negrita,
                                  string colorHex, double centroX, double y)
        {
            if (string.IsNullOrWhiteSpace(texto)) return;

            var ft = new FormattedText(
                texto,
                CultureInfo.GetCultureInfo("es-AR"),
                FlowDirection.LeftToRight,
                new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal,
                             negrita ? FontWeights.Bold : FontWeights.Normal, FontStretches.Normal),
                tamano,
                Pincel(colorHex),
                1.0);

            dc.DrawText(ft, new Point(centroX - ft.Width / 2, y));
        }

        private static Brush Pincel(string hex)
        {
            var pincel = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
            pincel.Freeze();
            return pincel;
        }
    }
}