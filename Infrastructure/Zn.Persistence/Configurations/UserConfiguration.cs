using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zn.Domain.Entity;

namespace Zn.Persistence.Configurations
{
    /// <summary>
    /// User entity'sinin veritabanı eşlemesi. Id, UserName, Email gibi
    /// Identity alanları IdentityDbContext (base.OnModelCreating) tarafından
    /// konfigüre edilir; burada yalnızca projeye eklenen özel alanlar tanımlanır.
    /// </summary>
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(u => u.FirstName)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(u => u.LastName)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(u => u.ImageUrl)
                   .IsRequired()
                   .HasMaxLength(500);
        }
    }
}
