using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zn.Domain.Entity;

namespace Zn.Persistence.Configurations
{
    /// <summary>
    /// Comment entity'sinin veritabanı eşlemesi; Blog ve User ile olan
    /// bire-çok ilişkilerin Fluent API tanımı.
    /// </summary>
    public class CommentConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            builder.ToTable("Comments");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.CommentText)
                   .IsRequired()
                   .HasMaxLength(1000);

            builder.Property(c => c.CreatedAt)
                   .IsRequired();

            builder.Property(c => c.UpdatedAt)
                   .IsRequired(false);

            // İlişki: Blog silinince ona ait yorumlar da silinir (Cascade).
            builder.HasOne(c => c.Blog)
                   .WithMany(b => b.Comments)
                   .HasForeignKey(c => c.BlogId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade);

            // İlişki: Kullanıcı silinse bile yorum geçmişi korunmalı; ayrıca
            // SQL Server'da çoklu cascade path hatasını önlemek için Restrict.
            builder.HasOne(c => c.User)
                   .WithMany(u => u.Comments)
                   .HasForeignKey(c => c.UserId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Restrict);

            // Blog detay sayfasındaki "bu blogun yorumları" sorgusunu hızlandırır.
            builder.HasIndex(c => c.BlogId);
        }
    }
}
