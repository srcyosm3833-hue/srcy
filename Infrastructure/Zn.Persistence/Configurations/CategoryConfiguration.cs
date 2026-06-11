using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zn.Domain.Entity;

namespace Zn.Persistence.Configurations
{
    /// <summary>
    /// Category entity'sinin veritabanı eşlemesi.
    /// IEntityTypeConfiguration kullanımı, tüm map'leme kurallarını
    /// DbContext'i şişirmeden entity başına tek sınıfta toplar.
    /// </summary>
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.CategoryName)
                   .IsRequired()
                   .HasMaxLength(100);

            // Aynı isimde iki kategori oluşturulmasını veritabanı seviyesinde engeller.
            builder.HasIndex(c => c.CategoryName)
                   .IsUnique();

            builder.Property(c => c.CreatedAt)
                   .IsRequired();

            builder.Property(c => c.UpdatedAt)
                   .IsRequired(false);
        }
    }
}
