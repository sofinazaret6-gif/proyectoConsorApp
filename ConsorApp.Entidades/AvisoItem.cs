using System;
using System.Collections.Generic;
using System.Text;
using System;

namespace ConsorApp.Entidades
{
    public class AvisoItem
    {
        public Aviso Origen { get; }
        public string Destino { get; }
        // Para la vista del propietario: los individuales son "para él"
        public string EtiquetaPropietario => EsGeneral ? "GENERAL" : "PARA TI";
        public AvisoItem(Aviso origen, string destino)
        {
            Origen = origen;
            Destino = destino;
        }

        public int Id => Origen.id_Aviso;
        public string Titulo => Origen.titulo;
        public string Mensaje => Origen.mensaje;
        public DateTime FechaPublicacion => Origen.fechaPublicacion;

        // 1 = vigente, 0 = archivado
        public bool Vigente => Origen.estado == 1;

        // Aviso general = sin departamento asignado
        public bool EsGeneral => !Origen.id_Departamento.HasValue;

        public string TipoTexto => EsGeneral ? "GENERAL" : "INDIVIDUAL";

        public string DestinoTexto =>
            EsGeneral ? "Para: todo el edificio" : $"Para: {Destino}";

        // Publicado en las últimas 48 horas y todavía vigente
        public bool EsNuevo =>
            Vigente && (DateTime.Now - Origen.fechaPublicacion).TotalHours <= 48;

        public string FechaTexto
        {
            get
            {
                int dias = (DateTime.Today - Origen.fechaPublicacion.Date).Days;

                string relativo =
                    dias <= 0 ? "hoy" :
                    dias == 1 ? "ayer" :
                    $"hace {dias} días";

                return $"Publicado: {Origen.fechaPublicacion:dd/MM/yyyy - HH:mm} hs ({relativo})";
            }
        }
    }
}