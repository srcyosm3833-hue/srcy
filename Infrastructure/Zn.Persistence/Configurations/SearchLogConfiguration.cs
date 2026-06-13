using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zn.Domain.Entity;

namespace Zn.Persistence.Configurations
{
    /// <summary>
    /// SearchLog entity'sinin veritabanı eşlemesi. ApplyConfigurationsFromAssembly tarafından
    /// otomatik keşfedilir. Yalın bir audit kaydıdır (BaseEntity değil, UpdatedAt yok).
    /// <para>
    /// <see cref="SearchLog.UserId"/> nullable FK → AspNetUsers'tır; navigation property bilinçli
    /// olarak yoktur (log "kim aradı" sorusunu ayrıca <see cref="SearchLog.UserFullName"/> snapshot'ı
    /// ile de yanıtlar). Kullanıcı kaydı kalkarsa FK <c>SetNull</c> ile temizlenir; log satırı
    /// (snapshot'lı adıyla) korunur. SearchLog ISoftDeletable DEĞİLDİR; global query filter almaz —
    /// admin denetimi için silinmiş kullanıcının aramaları da görünür kalmalıdır.
    /// </para>
    /// </summary>
    public class SearchLogConfiguration : IEntityTypeConfiguration<SearchLog>
    {
        public void Configure(EntityTypeBuilder<SearchLog> builder)
        {
            builder.ToTable("SearchLogs");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Term)
                   .IsRequired()
                   .HasMaxLength(SearchLog.TermMaxLength);

            // Nullable: anonim aramada UserId boştur.
            builder.Property(s => s.UserId)
                   .IsRequired(false);

            // Log anındaki ad-soyad snapshot'ı; anonimde null.
            builder.Property(s => s.UserFullName)
                   .IsRequired(false)
                   .HasMaxLength(SearchLog.UserFullNameMaxLength);

            // Tuzlu SHA-256 IP hash'i (base64 ~44 karakter); çözülemediyse null.
            builder.Property(s => s.IpHash)
                   .IsRequired(false)
                   .HasMaxLength(SearchLog.IpHashMaxLength);

            builder.Property(s => s.SearchedAt)
                   .IsRequired();

            // Nullable FK → AspNetUsers; navigation YOK (yalın entity). Kullanıcı kaydı kalkarsa
            // FK temizlenir, log korunur. Bu ilişki AspNetUsers'a giden ek bir cascade yolu açmaz
            // (SetNull), SQL Server çoklu cascade path kısıtını ihlal etmez.
            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(s => s.UserId)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.SetNull);

            // En yeni aramalar önce listelendiği için SearchedAt üzerinde (azalan) indeks.
            builder.HasIndex(s => s.SearchedAt);
        }
    }
}
