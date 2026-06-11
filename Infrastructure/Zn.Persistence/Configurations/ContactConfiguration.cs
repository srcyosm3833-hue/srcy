using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zn.Domain.Entity;

namespace Zn.Persistence.Configurations
{
    /// <summary>
    /// Contact entity'sinin veritabanı eşlemesi.
    /// ApplyConfigurationsFromAssembly tarafından otomatik keşfedilir;
    /// DbContext'e elle kayıt gerekmez.
    /// </summary>
    public class ContactConfiguration : IEntityTypeConfiguration<Contact>
    {
        public void Configure(EntityTypeBuilder<Contact> builder)
        {
            builder.ToTable("Contacts");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Address)
                   .IsRequired()
                   .HasMaxLength(300);

            builder.Property(c => c.Email)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(c => c.Phone)
                   .IsRequired()
                   .HasMaxLength(20);

            // Embed harita linkleri uzun olabildiği için geniş sınır bırakıldı.
            builder.Property(c => c.MapUrl)
                   .IsRequired()
                   .HasMaxLength(1000);

            builder.Property(c => c.CreatedAt)
                   .IsRequired();

            builder.Property(c => c.UpdatedAt)
                   .IsRequired(false);
        }
    }
}
