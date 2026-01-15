using Microsoft.EntityFrameworkCore;
using ProductsAPI.Models; 

namespace ProductsAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // As tuas tabelas baseadas nos ficheiros que vi no teu projeto
        public DbSet<Product> Products { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Previne arredondamentos errados no Preço do Produto
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            // Previne arredondamentos errados no Total da Venda
            modelBuilder.Entity<Sale>()
                .Property(s => s.TotalPrice)
                .HasColumnType("decimal(18,2)");
                
            // -------------------------------------------------------------------
        }
    }
}