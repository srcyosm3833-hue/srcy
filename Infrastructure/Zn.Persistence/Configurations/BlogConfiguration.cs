using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zn.Domain.Entity;

namespace Zn.Persistence.Configurations
{
    /// <summary>
    /// Blog entity'sinin veritabanı eşlemesi ve Category ile olan
    /// bire-çok (1-N) ilişkisinin Fluent API tanımı.
    /// </summary>
    public class BlogConfiguration : IEntityTypeConfiguration<Blog>
    {
        public void Configure(EntityTypeBuilder<Blog> builder)
        {
            builder.ToTable("Blogs");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.Title)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(b => b.CoverImage)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(b => b.BlogImage)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(b => b.Description)
                   .IsRequired();

            builder.Property(b => b.CreatedAt)
                   .IsRequired();

            builder.Property(b => b.UpdatedAt)
                   .IsRequired(false);

            // İlişki: Bir Blog zorunlu olarak bir Category'ye aittir,
            // bir Category'nin birden çok Blog'u olabilir (1-N).
            builder.HasOne(b => b.Category)
                   .WithMany(c => c.Blogs)
                   .HasForeignKey(b => b.CategoryId)
                   .IsRequired()
                   // Bloglari olan bir kategorinin yanlışlıkla silinip
                   // blogların da cascade ile yok olmasını engeller.
                   .OnDelete(DeleteBehavior.Restrict);

            // İlişki: Bir Blog zorunlu olarak bir yazara (User) aittir (N-1).
            // Yazar silinse bile blogları yok olmamalı (iş kuralı) + Comment/SubComment
            // gibi User ilişkileriyle tutarlı olarak SQL Server çoklu cascade path'ini
            // önlemek için Restrict.
            builder.HasOne(b => b.User)
                   .WithMany(u => u.Blogs)
                   .HasForeignKey(b => b.UserId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Restrict);

            // Kategoriye göre listeleme en sık sorgu olacağı için FK üzerinde indeks.
            builder.HasIndex(b => b.CategoryId);

            // Yazara göre listeleme ("bu kullanıcının blogları") sorgusunu hızlandırır.
            builder.HasIndex(b => b.UserId);
        }
    }
}
