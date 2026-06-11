using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Zn.Domain.Entity;

namespace Zn.Persistence.Context
{
    /// <summary>
    /// Uygulamanın EF Core veritabanı bağlamı.
    /// IdentityDbContext'ten türediği için Identity tabloları (AspNetUsers,
    /// AspNetRoles, AspNetUserRoles vb.) string anahtar tipiyle otomatik kurulur.
    /// Tüm IEntityTypeConfiguration sınıfları assembly taramasıyla uygulanır.
    /// </summary>
    public class ZnBlogDbContext : IdentityDbContext<User, Role, string>
    {
        public ZnBlogDbContext(DbContextOptions<ZnBlogDbContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Blog> Blogs => Set<Blog>();
        public DbSet<Contact> Contacts => Set<Contact>();
        public DbSet<Message> Messages => Set<Message>();
        public DbSet<SocialMedia> SocialMedias => Set<SocialMedia>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<SubComment> SubComments => Set<SubComment>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Önce Identity'nin kendi eşlemeleri kurulur (AspNetUsers, AspNetRoles...).
            base.OnModelCreating(modelBuilder);

            // Sonra projenin konfigürasyonları uygulanır; böylece gerektiğinde
            // Identity varsayılanları kendi kurallarımızla ezilebilir.
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
