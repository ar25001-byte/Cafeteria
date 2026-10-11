using Cafeteria.Domain.Entities;
using Cafeteria.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Cafeteria.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<DetallePedido> DetallesPedido => Set<DetallePedido>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- Categoria Configuration ---
        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Descripcion).HasMaxLength(250);
            entity.Property(c => c.Estado).HasConversion<int>().IsRequired();
            entity.Property(c => c.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasMany(c => c.Productos)
                  .WithOne(p => p.Categoria)
                  .HasForeignKey(p => p.CategoriaId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // --- Producto Configuration ---
        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Nombre).IsRequired().HasMaxLength(150);
            entity.Property(p => p.Descripcion).HasMaxLength(500);
            entity.Property(p => p.Precio).HasPrecision(18, 2).IsRequired();
            entity.Property(p => p.Stock).IsRequired();
            entity.Property(p => p.StockMinimo).IsRequired();
            entity.Property(p => p.ImagenUrl).HasMaxLength(500);
            entity.Property(p => p.Estado).HasConversion<int>().IsRequired();
            entity.Property(p => p.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(p => p.Nombre);
            entity.HasIndex(p => p.CategoriaId);
        });

        // --- Cliente Configuration ---
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Telefono).IsRequired().HasMaxLength(20);
            entity.Property(c => c.Email).IsRequired().HasMaxLength(120);
            entity.Property(c => c.FechaRegistro).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(c => c.Email).IsUnique();
            entity.HasIndex(c => c.Nombre);

            entity.HasMany(c => c.Pedidos)
                  .WithOne(p => p.Cliente)
                  .HasForeignKey(p => p.ClienteId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // --- Pedido Configuration ---
        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Fecha).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(p => p.Estado).HasConversion<int>().IsRequired();
            entity.Property(p => p.Subtotal).HasPrecision(18, 2).IsRequired();
            entity.Property(p => p.Impuesto).HasPrecision(18, 2).IsRequired();
            entity.Property(p => p.Total).HasPrecision(18, 2).IsRequired();

            entity.HasMany(p => p.Detalles)
                  .WithOne(d => d.Pedido)
                  .HasForeignKey(d => d.PedidoId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // --- DetallePedido Configuration ---
        modelBuilder.Entity<DetallePedido>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.PrecioUnitario).HasPrecision(18, 2).IsRequired();
            entity.Property(d => d.Subtotal).HasPrecision(18, 2).IsRequired();
            entity.Property(d => d.Cantidad).IsRequired();

            entity.HasOne(d => d.Producto)
                  .WithMany(p => p.Detalles)
                  .HasForeignKey(d => d.ProductoId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // --- Usuario Configuration ---
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.NombreCompleto).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(120);
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Rol).HasConversion<int>().IsRequired();
            entity.Property(u => u.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();
        });

        // --- Seed Data ---
        SeedInitialData(modelBuilder);
    }

    private static void SeedInitialData(ModelBuilder modelBuilder)
    {
        // Seed Usuarios iniciales

        modelBuilder.Entity<Usuario>().HasData(
            new Usuario
            {
                Id = 1,
                NombreCompleto = "Administrador Principal",
                Username = "admin",
                Email = "admin@cafeteria.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!*"),
                Rol = RolUsuario.Admin,
                Activo = true,
                FechaCreacion = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Usuario
            {
                Id = 2,
                NombreCompleto = "Cajero Turno Matutino",
                Username = "cajero1",
                Email = "cajero@cafeteria.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Cajero123!*"),
                Rol = RolUsuario.Cajero,
                Activo = true,
                FechaCreacion = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        modelBuilder.Entity<Categoria>().HasData(
            new Categoria
            {
                Id = 1,
                Nombre = "Cafés Calientes",
                Descripcion = "Especialidades clásicas de café espresso e infusiones calientes.",
                Estado = EstadoCategoria.Activo,
                FechaCreacion = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Categoria
            {
                Id = 2,
                Nombre = "Bebidas Frías & Frappés",
                Descripcion = "Cafés helados, frappés artesanales, smoothies y sodas italianas.",
                Estado = EstadoCategoria.Activo,
                FechaCreacion = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Categoria
            {
                Id = 3,
                Nombre = "Repostería & Panadería",
                Descripcion = "Pasteles horneados a diario, croissants y bocadillos dulces.",
                Estado = EstadoCategoria.Activo,
                FechaCreacion = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Categoria
            {
                Id = 4,
                Nombre = "Desayunos & Snacks",
                Descripcion = "Sandwiches gourmet, bagels, tostadas y quiches.",
                Estado = EstadoCategoria.Activo,
                FechaCreacion = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        modelBuilder.Entity<Producto>().HasData(
            new Producto
            {
                Id = 1,
                CategoriaId = 1,
                Nombre = "Espresso Doble",
                Descripcion = "Extracción doble de granos arábica de especialidad de altura.",
                Precio = 45.00m,
                Stock = 80,
                StockMinimo = 15,
                ImagenUrl = "https://images.unsplash.com/photo-1510591509098-f4fdc6d0ff04?w=500",
                Estado = EstadoProducto.Disponible,
                FechaCreacion = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Producto
            {
                Id = 2,
                CategoriaId = 1,
                Nombre = "Cappuccino Italiano",
                Descripcion = "Espresso balanceado con leche vaporizada y microespuma aterciopelada.",
                Precio = 65.00m,
                Stock = 50,
                StockMinimo = 10,
                ImagenUrl = "https://images.unsplash.com/photo-1534778101976-62847782c213?w=500",
                Estado = EstadoProducto.Disponible,
                FechaCreacion = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Producto
            {
                Id = 3,
                CategoriaId = 2,
                Nombre = "Frappé Moka Avellana",
                Descripcion = "Café espresso batido con chocolate belga, jarabe de avellana y crema chantilly.",
                Precio = 85.00m,
                Stock = 35,
                StockMinimo = 8,
                ImagenUrl = "https://images.unsplash.com/photo-1572490122747-3968b75cc699?w=500",
                Estado = EstadoProducto.Disponible,
                FechaCreacion = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Producto
            {
                Id = 4,
                CategoriaId = 3,
                Nombre = "Croissant de Mantequilla",
                Descripcion = "Hojaldrado tradicional francés con mantequilla pura importada.",
                Precio = 48.00m,
                Stock = 4, // Stock bajo a propósito para probar dashboard
                StockMinimo = 10,
                ImagenUrl = "https://images.unsplash.com/photo-1555507036-ab1f4038808a?w=500",
                Estado = EstadoProducto.Disponible,
                FechaCreacion = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Producto
            {
                Id = 5,
                CategoriaId = 4,
                Nombre = "Panini Pavo & Queso Gouda",
                Descripcion = "Pan ciabatta artesanal con pechuga de pavo ahumada, pesto y queso derretido.",
                Precio = 95.00m,
                Stock = 20,
                StockMinimo = 5,
                ImagenUrl = "https://images.unsplash.com/photo-1528735602780-2552fd46c7af?w=500",
                Estado = EstadoProducto.Disponible,
                FechaCreacion = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        modelBuilder.Entity<Cliente>().HasData(
            new Cliente
            {
                Id = 1,
                Nombre = "Carlos Mendoza",
                Telefono = "+525512345678",
                Email = "carlos.mendoza@gmail.com",
                FechaRegistro = new DateTime(2026, 1, 5, 10, 0, 0, DateTimeKind.Utc)
            },
            new Cliente
            {
                Id = 2,
                Nombre = "Ana Sofía Ruiz",
                Telefono = "+525598765432",
                Email = "ana.ruiz@gmail.com",
                FechaRegistro = new DateTime(2026, 1, 8, 12, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
