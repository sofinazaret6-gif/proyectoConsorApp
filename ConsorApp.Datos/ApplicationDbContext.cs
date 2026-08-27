using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ConsorApp.Entidades;

namespace ConsorApp.Datos
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Perfil> Perfiles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Configuración para usar LocalDB de SQL Server en tu PC
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ConsorAppDB;Trusted_Connection=True;MultipleActiveResultSets=true");
        }
    }
}