using Microsoft.EntityFrameworkCore;
using Examen2DSE.Models;

namespace Examen2DSE.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Permiso> Permisos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Rol>().HasData(
                new Rol { Id = 1, Nombre = "Admin", Descripcion = "Administrador del Negocio" },
                new Rol { Id = 2, Nombre = "User", Descripcion = "Usuario normal" }
            );

            modelBuilder.Entity<Permiso>().HasData(
                new Permiso { Id = 1, Nombre = "Leer", Descripcion = "Permiso para leer datos" },
                new Permiso { Id = 2, Nombre = "Escribir", Descripcion = "Permiso para escribir datos" },
                new Permiso { Id = 3, Nombre = "Eliminar", Descripcion = "Permiso para eliminar datos" }
            );

            modelBuilder.Entity<Usuario>().HasData(
                new Usuario { Id = 1, Nombre = "Kevin", Email = "kevincitoDindo@gmail.com", Contraseña = "12345678", RolId = 1 },
                new Usuario { Id = 2, Nombre = "Alex", Email = "alexBelloso@gmail.com", Contraseña = "87654321", RolId = 2 }
            );
        }
    }
}
