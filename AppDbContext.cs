using Microsoft.EntityFrameworkCore;
using ProductsAPI.Models;

namespace ProductsAPI.Data
{
    public class AppDbContext : DbContext
    {
        // Construtor vazio (necessário para alguns cenários do EF Core)
        public AppDbContext()
        {
        }

        // Construtor que aceita opções (útil para injeção de dependência no Program.cs)
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Definição das Tabelas
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Apenas configura se ainda não estiver configurado (evita conflito com o Program.cs)
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(@"Server=localhost,1433;Database=ProductsDB;User Id=sa;Password=GrupoE2526!;TrustServerCertificate=true;");
            }
        }
    }
}