using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zn.Domain.Entity;

namespace Zn.Persistence.Configurations
{
    /// <summary>
    /// BlogLike (blog beğenisi) saf ilişki tablosunun veritabanı eşlemesi.
    /// <para>
    /// Birincil anahtar composite'tir: (BlogId, UserId). Bu sayede aynı kullanıcının aynı blogu
    /// birden fazla kez beğenmesi veritabanı seviyesinde engellenir (idempotent beğeni). Surrogate
    /// Id yoktur. Blog silinince beğeniler de silinir (Cascade); kullanıcı tarafında ise çoklu
    /// cascade path hatasını önlemek için Restrict kullanılır (User zaten soft-delete edilir).
    /// </para>
    /// </summary>
    public class BlogLikeConfiguration : IEntityTypeConfiguration<BlogLike>
    {
        public void Configure(EntityTypeBuilder<BlogLike> builder)
        {
            builder.ToTable("BlogLikes");

            // Composite birincil anahtar — aynı (blog, kullanıcı) çifti yalnızca bir kez var olabilir.
            builder.HasKey(bl => new { bl.BlogId, bl.UserId });

            builder.Property(bl => bl.CreatedAt)
                   .IsRequired();

            // İlişki: Blog silinince ona ait beğeniler de silinir (Cascade).
            builder.HasOne(bl => bl.Blog)
                   .WithMany(b => b.Likes)
                   .HasForeignKey(bl => bl.BlogId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade);

            // İlişki: Kullanıcı tarafında Restrict — SQL Server çoklu cascade path hatasını
            // önler ve User soft-delete edildiği için fiziksel silme zaten oluşmaz.
            builder.HasOne(bl => bl.User)
                   .WithMany()
                   .HasForeignKey(bl => bl.UserId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Restrict);

            // "Bu kullanıcının beğenileri" sorgusunu hızlandırır (PK BlogId ile başladığı için
            // UserId tek başına aramada ayrı indeks gerektirir).
            builder.HasIndex(bl => bl.UserId);
        }
    }
}
