using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace ConsorApp.Negocio
{
    public static class Motivos
    {
        public const string Otro = "Otro";

        public static readonly List<string> Categorias = new()
        {
            "Ruidos molestos",
            "Gotera / Filtración",
            "Basura fuera de horario",
            "Problema con ascensor",
            "Limpieza en espacios comunes",
            "Problemas de seguridad",
            "Problema con agua",
            "Problema eléctrico",
            Otro
        };

        // El orden importa: la primera categoría que coincide gana
        private static readonly (string Categoria, string[] Palabras)[] PalabrasClave =
        {
            ("Gotera / Filtración",         new[] { "gotera", "goteo", "filtra", "humedad", "mancha" }),
            ("Ruidos molestos",             new[] { "ruido", "musica", "fiesta", "grito", "golpe", "bulla", "perro", "ladrido" }),
            ("Basura fuera de horario",     new[] { "basura", "residuo", "contenedor", "bolsa" }),
            ("Problema con ascensor",       new[] { "ascensor", "elevador" }),
            ("Limpieza en espacios comunes",new[] { "limpie", "sucio", "suciedad", "mugre", "pasillo", "escalera" }),
            ("Problemas de seguridad",      new[] { "segur", "robo", "ladron", "porton", "camara", "alarma", "intruso", "cerradura" }),
            ("Problema eléctrico",          new[] { "electric", "luz", "cable", "enchufe", "tablero", "cortocircuito", "corte" }),
            ("Problema con agua",           new[] { "agua", "canilla", "caneria", "presion", "cloaca", "tanque", "inodoro" }),
        };

        public static string Clasificar(string motivo, string descripcion = "")
        {
            string m = Normalizar(motivo);

            // 1) Coincide exacto con una categoría de la lista
            string? exacta = Categorias.FirstOrDefault(c => Normalizar(c) == m);
            if (exacta != null) return exacta;

            // 2) Palabras clave en el motivo escrito
            string porMotivo = BuscarPorPalabras(m);
            if (porMotivo != Otro) return porMotivo;

            // 3) Último intento: palabras clave en la descripción
            return BuscarPorPalabras(Normalizar(descripcion));
        }

        private static string BuscarPorPalabras(string textoNormalizado)
        {
            if (string.IsNullOrWhiteSpace(textoNormalizado)) return Otro;

            foreach (var (categoria, palabras) in PalabrasClave)
                if (palabras.Any(p => textoNormalizado.Contains(p)))
                    return categoria;

            return Otro;
        }

        private static string Normalizar(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return string.Empty;

            string d = texto.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (char c in d)
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}