using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zn.Domain.Entity;

namespace Zn.Persistence.Configurations
{
    /// <summary>
    /// SocialMedia entity'sinin veritabanı eşlemesi.
    /// ApplyConfigurationsFromAssembly tarafından otomatik keşfedilir.
    /// </summary>
    public class SocialMediaConfiguration : IEntityTypeConfiguration<SocialMedia>
    {
        public void Configure(EntityTypeBuilder<SocialMedia> builder)
        {
            builder.ToTable("SocialMedias");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Title)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(s => s.Url)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(s => s.Icon)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(s => s.CreatedAt)
                   .IsRequired();

            builder.Property(s => s.UpdatedAt)
                   .IsRequired(false);

            // Aynı platformun (örn. iki kez "Instagram") mükerrer eklenmesini
            // veritabanı seviyesinde engeller.
            builder.HasIndex(s => s.Title)
                   .IsUnique();
        }
    }
}
