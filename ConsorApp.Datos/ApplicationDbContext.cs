using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ConsorApp.Entidades;
using System;
using System.Collections.Generic;


namespace ConsorApp.Datos
{
    public class ApplicationDbContext : DbContext
    {
        // 1. Tablas de Usuarios y Seguridad
        public DbSet<Perfil> Perfiles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Propietario> Propietarios { get; set; }

        // 2. Tablas del Edificio y Departamentos
        public DbSet<EDIFICIO> Edificios { get; set; }
        public DbSet<Departamento> Departamentos { get; set; }
        public DbSet<EspacioComun> EspaciosComunes { get; set; }

        // 3. Tablas de Operaciones (Reservas, Reclamos, Avisos)
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<Reclamo> Reclamos { get; set; }
        public DbSet<Aviso> Avisos { get; set; }

        // 4. Tablas de Contabilidad y Gastos
        public DbSet<conceptoGasto> ConceptosGasto { get; set; }
        public DbSet<GastoEdificio> GastosEdificio { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Conexión local a la base de datos SQL Express
                optionsBuilder.UseSqlServer(@"Server=.\SQLEXPRESS;Database=consorAppDb;Integrated Security=True;TrustServerCertificate=True;");
            }
        }
    }
}