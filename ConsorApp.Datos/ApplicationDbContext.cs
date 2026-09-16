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
            if (!optionsBuilder.IsConfigured)
            {
                // El punto (.) indica "mi servidor local de SQL Express actual"
                optionsBuilder.UseSqlServer(@"Server=.\SQLEXPRESS;Database=consorAppDb;Integrated Security=True;TrustServerCertificate=True;");
            }
        }
    }
}