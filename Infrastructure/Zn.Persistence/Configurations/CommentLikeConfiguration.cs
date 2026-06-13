using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zn.Domain.Entity;

namespace Zn.Persistence.Configurations
{
    /// <summary>
    /// CommentLike (yorum beğenisi) saf ilişki tablosunun veritabanı eşlemesi.
    /// <para>
    /// Birincil anahtar composite'tir: (CommentId, UserId). Bu sayede aynı kullanıcının aynı yorumu
    /// birden fazla kez beğenmesi veritabanı seviyesinde engellenir (idempotent beğeni). Surrogate
    /// Id yoktur. Yorum silinince beğeniler de silinir (Cascade); kullanıcı tarafında ise çoklu
    /// cascade path hatasını önlemek için Restrict kullanılır (User zaten soft-delete edilir).
    /// </para>
    /// </summary>
    public class CommentLikeConfiguration : IEntityTypeConfiguration<CommentLike>
    {
        public void Configure(EntityTypeBuilder<CommentLike> builder)
        {
            builder.ToTable("CommentLikes");

            // Composite birincil anahtar — aynı (yorum, kullanıcı) çifti yalnızca bir kez var olabilir.
            builder.HasKey(cl => new { cl.CommentId, cl.UserId });

            builder.Property(cl => cl.CreatedAt)
                   .IsRequired();

            // İlişki: Yorum silinince ona ait beğeniler de silinir (Cascade).
            builder.HasOne(cl => cl.Comment)
                   .WithMany(c => c.Likes)
                   .HasForeignKey(cl => cl.CommentId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade);

            // İlişki: Kullanıcı tarafında Restrict — SQL Server çoklu cascade path hatasını
            // önler ve User soft-delete edildiği için fiziksel silme zaten oluşmaz.
            builder.HasOne(cl => cl.User)
                   .WithMany()
                   .HasForeignKey(cl => cl.UserId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Restrict);

            // "Bu kullanıcının beğenileri" sorgusunu hızlandırır.
            builder.HasIndex(cl => cl.UserId);
        }
    }
}
