using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zn.Domain.Entity;

namespace Zn.Persistence.Configurations
{
    /// <summary>
    /// SubComment entity'sinin veritabanı eşlemesi; Comment ve User ile olan
    /// bire-çok ilişkilerin Fluent API tanımı.
    /// </summary>
    public class SubCommentConfiguration : IEntityTypeConfiguration<SubComment>
    {
        public void Configure(EntityTypeBuilder<SubComment> builder)
        {
            builder.ToTable("SubComments");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.SubCommentText)
                   .IsRequired()
                   .HasMaxLength(1000);

            builder.Property(s => s.CreatedAt)
                   .IsRequired();

            builder.Property(s => s.UpdatedAt)
                   .IsRequired(false);

            // İlişki: Ana yorum silinince yanıtları da silinir (Cascade).
            builder.HasOne(s => s.Comment)
                   .WithMany(c => c.SubComments)
                   .HasForeignKey(s => s.CommentId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade);

            // İlişki: Comment tarafıyla aynı gerekçeyle Restrict
            // (yorum geçmişini koru + çoklu cascade path'i engelle).
            builder.HasOne(s => s.User)
                   .WithMany(u => u.SubComments)
                   .HasForeignKey(s => s.UserId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Restrict);

            // Yorumun yanıtlarını listeleme sorgusunu hızlandırır.
            builder.HasIndex(s => s.CommentId);
        }
    }
}
