using Microsoft.EntityFrameworkCore;
using QuickBite.API.Models;
using System.Text.Json;

namespace QuickBite.API.Data
{
    /// <summary>
    /// Entity Framework Core database context for QuickBite API
    /// Manages database connections and entity mappings
    /// </summary>
    public class QuickBiteDbContext : DbContext
    {
        public QuickBiteDbContext(DbContextOptions<QuickBiteDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Menu items table
        /// </summary>
        public DbSet<MenuItem> MenuItems { get; set; }

        /// <summary>
        /// Categories reference table
        /// </summary>
        public DbSet<Category> Categories { get; set; }

        /// <summary>
        /// Dietary tags reference table
        /// </summary>
        public DbSet<DietaryTag> DietaryTags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure MenuItem entity
            modelBuilder.Entity<MenuItem>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.Price)
                    .IsRequired()
                    .HasColumnType("decimal(10,2)");

                entity.Property(e => e.Category)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.DietaryTags)
                    .HasMaxLength(1000);

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("datetime('now')");

                entity.Property(e => e.UpdatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("datetime('now')");

                // Create unique constraint on Name and Category combination
                entity.HasIndex(e => new { e.Name, e.Category })
                    .IsUnique()
                    .HasDatabaseName("IX_MenuItem_Name_Category");

                // Create indexes for performance
                entity.HasIndex(e => e.Category)
                    .HasDatabaseName("IX_MenuItem_Category");

                entity.HasIndex(e => e.Price)
                    .HasDatabaseName("IX_MenuItem_Price");

                entity.HasIndex(e => e.DeletedAt)
                    .HasDatabaseName("IX_MenuItem_DeletedAt");

                entity.HasIndex(e => e.CreatedAt)
                    .HasDatabaseName("IX_MenuItem_CreatedAt");

                // Configure check constraint for positive price
                entity.HasCheckConstraint("CK_MenuItem_Price_Positive", "Price > 0");
            });

            // Configure Category entity
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.DisplayOrder)
                    .HasDefaultValue(0);

                entity.Property(e => e.Active)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("datetime('now')");

                entity.HasIndex(e => e.Name)
                    .IsUnique()
                    .HasDatabaseName("IX_Category_Name");
            });

            // Configure DietaryTag entity
            modelBuilder.Entity<DietaryTag>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Description)
                    .HasMaxLength(200);

                entity.Property(e => e.Active)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("datetime('now')");

                entity.HasIndex(e => e.Name)
                    .IsUnique()
                    .HasDatabaseName("IX_DietaryTag_Name");
            });

            // Seed initial data
            SeedData(modelBuilder);
        }

        /// <summary>
        /// Seed initial reference data as specified in BRD
        /// </summary>
        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Appetizers", DisplayOrder = 1, Active = true, CreatedAt = DateTime.UtcNow },
                new Category { Id = 2, Name = "Salads", DisplayOrder = 2, Active = true, CreatedAt = DateTime.UtcNow },
                new Category { Id = 3, Name = "Soups", DisplayOrder = 3, Active = true, CreatedAt = DateTime.UtcNow },
                new Category { Id = 4, Name = "Main Course", DisplayOrder = 4, Active = true, CreatedAt = DateTime.UtcNow },
                new Category { Id = 5, Name = "Desserts", DisplayOrder = 5, Active = true, CreatedAt = DateTime.UtcNow },
                new Category { Id = 6, Name = "Beverages", DisplayOrder = 6, Active = true, CreatedAt = DateTime.UtcNow }
            );

            // Seed Dietary Tags
            modelBuilder.Entity<DietaryTag>().HasData(
                new DietaryTag { Id = 1, Name = "Vegetarian", Description = "Contains no meat or fish", Active = true, CreatedAt = DateTime.UtcNow },
                new DietaryTag { Id = 2, Name = "Vegan", Description = "Contains no animal products", Active = true, CreatedAt = DateTime.UtcNow },
                new DietaryTag { Id = 3, Name = "Gluten-Free", Description = "Does not contain gluten", Active = true, CreatedAt = DateTime.UtcNow },
                new DietaryTag { Id = 4, Name = "Dairy-Free", Description = "Contains no dairy products", Active = true, CreatedAt = DateTime.UtcNow },
                new DietaryTag { Id = 5, Name = "Nut-Free", Description = "Does not contain nuts", Active = true, CreatedAt = DateTime.UtcNow },
                new DietaryTag { Id = 6, Name = "Spicy", Description = "Contains spicy ingredients", Active = true, CreatedAt = DateTime.UtcNow },
                new DietaryTag { Id = 7, Name = "Low-Carb", Description = "Low in carbohydrates", Active = true, CreatedAt = DateTime.UtcNow }
            );
        }

        /// <summary>
        /// Override SaveChanges to automatically update UpdatedAt timestamp
        /// </summary>
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        /// <summary>
        /// Override SaveChangesAsync to automatically update UpdatedAt timestamp
        /// </summary>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Update timestamps for modified entities
        /// </summary>
        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is MenuItem && (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                var entity = (MenuItem)entityEntry.Entity;

                if (entityEntry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow;
                }

                entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}