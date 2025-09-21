using Microsoft.EntityFrameworkCore;
using SimpleInventory.DataAccess.DAOs;

namespace SimpleInventory.DataAccess
{
    public class SimpleInventoryContext : DbContext
    {
        public SimpleInventoryContext(DbContextOptions<SimpleInventoryContext> options)
            : base(options) { }
        public DbSet<CategoryDAO> Categories { get; set; }
        public DbSet<ProductDAO> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CategoryDAO>()
                .HasIndex(c => c.Name)
                .IsUnique();

            modelBuilder.Entity<ProductDAO>(entity =>
            {
                entity.HasOne(p => p.Category)
                      .WithMany()
                      .HasForeignKey(p => p.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(p => p.Sku)
                      .IsUnique();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}