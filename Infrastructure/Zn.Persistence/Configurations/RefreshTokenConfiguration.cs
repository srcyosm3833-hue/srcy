using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zn.Domain.Entity;

namespace Zn.Persistence.Configurations
{
    /// <summary>
    /// RefreshToken entity'sinin veritabanı eşlemesi ve User ile olan
    /// bire-çok (1-N) ilişkisinin Fluent API tanımı.
    /// </summary>
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");

            builder.HasKey(rt => rt.Id);

            // Token SHA-256 hash olarak saklanır; base64 hali 44 karakterdir,
            // pay bırakmak için 256. Benzersiz olmalı (aynı hash iki kez bulunamaz).
            builder.Property(rt => rt.Token)
                   .IsRequired()
                   .HasMaxLength(256);

            builder.Property(rt => rt.UserId)
                   .IsRequired();

            builder.Property(rt => rt.ExpiresAt)
                   .IsRequired();

            builder.Property(rt => rt.RevokedAt)
                   .IsRequired(false);

            builder.Property(rt => rt.ReplacedByToken)
                   .IsRequired(false)
                   .HasMaxLength(256);

            builder.Property(rt => rt.CreatedAt)
                   .IsRequired();

            builder.Property(rt => rt.UpdatedAt)
                   .IsRequired(false);

            // Hesaplanan özellikler (IsActive/IsExpired) DB'ye eşlenmez.
            builder.Ignore(rt => rt.IsActive);
            builder.Ignore(rt => rt.IsExpired);

            // İlişki: Bir RefreshToken zorunlu olarak bir User'a aittir (N-1).
            // Kullanıcı silinince token'larının yaşaması anlamsızdır; Cascade uygundur.
            // User'a giden tek cascade yolu bu olduğundan SQL Server çoklu cascade
            // path kısıtı ihlal edilmez (Blog/Comment/SubComment ilişkileri Restrict).
            builder.HasOne(rt => rt.User)
                   .WithMany(u => u.RefreshTokens)
                   .HasForeignKey(rt => rt.UserId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade);

            // Refresh akışında token hash'iyle hızlı ve benzersiz arama için unique index.
            builder.HasIndex(rt => rt.Token)
                   .IsUnique();

            // "Bu kullanıcının tüm aktif token'larını iptal et" sorgusunu hızlandırır.
            builder.HasIndex(rt => rt.UserId);
        }
    }
}
