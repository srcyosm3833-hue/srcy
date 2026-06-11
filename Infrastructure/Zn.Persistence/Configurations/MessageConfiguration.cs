using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zn.Domain.Entity;

namespace Zn.Persistence.Configurations
{
    /// <summary>
    /// Message entity'sinin veritabanı eşlemesi.
    /// ApplyConfigurationsFromAssembly tarafından otomatik keşfedilir.
    /// </summary>
    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.ToTable("Messages");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(m => m.Email)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(m => m.Subject)
                   .IsRequired()
                   .HasMaxLength(200);

            // Mesaj gövdesi serbest uzunlukta; veritabanında nvarchar(max) olarak kalır.
            builder.Property(m => m.MessageBody)
                   .IsRequired();

            // Yeni mesaj veritabanı seviyesinde de okunmamış olarak başlar.
            builder.Property(m => m.IsRead)
                   .IsRequired()
                   .HasDefaultValue(false);

            builder.Property(m => m.CreatedAt)
                   .IsRequired();

            builder.Property(m => m.UpdatedAt)
                   .IsRequired(false);

            // Yönetim panelindeki "okunmamış mesajlar" sorgusunu hızlandırır.
            builder.HasIndex(m => m.IsRead);
        }
    }
}
