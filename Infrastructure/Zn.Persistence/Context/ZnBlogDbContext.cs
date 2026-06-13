using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Zn.Domain.Entity;
using Zn.Domain.Entity.Common;

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
        public DbSet<BlogLike> BlogLikes => Set<BlogLike>();
        public DbSet<CommentLike> CommentLikes => Set<CommentLike>();
        public DbSet<SearchLog> SearchLogs => Set<SearchLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Önce Identity'nin kendi eşlemeleri kurulur (AspNetUsers, AspNetRoles...).
            base.OnModelCreating(modelBuilder);

            // Sonra projenin konfigürasyonları uygulanır; böylece gerektiğinde
            // Identity varsayılanları kendi kurallarımızla ezilebilir.
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // Soft delete global query filter: ISoftDeletable uygulayan tüm entity tipleri için
            // otomatik olarak "e => !e.IsDeleted" filtresi kurulur. Böylece silinmiş kayıtlar
            // varsayılan sorgularda (public API dahil) hiçbir zaman dönmez. Admin/Manager
            // sorguları repository seviyesinde IgnoreQueryFilters() ile bu filtreyi bypass eder.
            // Tip-tarama, mevcut IEntityTypeConfiguration assembly-tarama yapısını bozmadan
            // ApplyConfigurationsFromAssembly'den SONRA çalışır.
            ApplySoftDeleteQueryFilters(modelBuilder);

            // Eşleşen (matching) query filter'lar: ISoftDeletable olmayan ama filtreli bir
            // principal'a (Blog/User) ZORUNLU bağımlı olan entity'ler. EF Core, filtreli principal
            // ile filtresiz zorunlu bağımlı arasındaki ilişkide tutarsızlık uyarısı (10622) verir;
            // ayrıca silinmiş bir blogun yorumları/beğenileri (A7) ve silinmiş bir kullanıcının
            // refresh token/alt-yorum/beğenileri public sorgularda sızabilir. Aşağıdaki filtreler
            // bu çocuk kayıtları principal'larıyla birlikte gizleyerek soft-delete'i tutarlı kılar.
            // DİKKAT (A8): Blog için yazar(User) bazlı bir filtre EKLENMEZ — kullanıcı silinince
            // blogları gizlenmez, yalnızca kullanıcı kaydı listelenmez.
            ApplyCascadeSoftDeleteQueryFilters(modelBuilder);
        }

        /// <summary>
        /// Filtreli principal'lara (Blog/User) zorunlu bağımlı olan, kendisi <see cref="ISoftDeletable"/>
        /// OLMAYAN entity'lere principal'larının soft-delete durumunu yansıtan eşleşen global query
        /// filter'ları ekler. Böylece silinmiş bir blogun/kullanıcının çocuk kayıtları da varsayılan
        /// sorgularda dönmez (A7 ile tutarlı). Admin sorguları gerektiğinde IgnoreQueryFilters() ile
        /// bu filtreleri de bypass edebilir.
        /// </summary>
        private static void ApplyCascadeSoftDeleteQueryFilters(ModelBuilder modelBuilder)
        {
            // Blog soft-delete edilince yorumları ve blog beğenileri gizlenir.
            modelBuilder.Entity<Comment>().HasQueryFilter(c => !c.Blog.IsDeleted);
            modelBuilder.Entity<BlogLike>().HasQueryFilter(bl => !bl.Blog.IsDeleted);

            // Kullanıcı soft-delete edilince refresh token'ları gizlenir (zaten login olamaz).
            modelBuilder.Entity<RefreshToken>().HasQueryFilter(rt => !rt.User.IsDeleted);

            // Alt yorum ve yorum beğenisi hem yazar(User) hem de ait olduğu blogun durumuyla
            // tutarlı olmalı: kullanıcı silinmişse veya alt yorumun/yorumun bağlı olduğu blog
            // silinmişse gizlenir. (Comment filtresiyle de zincirleme uyumludur.)
            modelBuilder.Entity<SubComment>().HasQueryFilter(sc => !sc.User.IsDeleted && !sc.Comment.Blog.IsDeleted);
            modelBuilder.Entity<CommentLike>().HasQueryFilter(cl => !cl.User.IsDeleted && !cl.Comment.Blog.IsDeleted);
        }

        /// <summary>
        /// Modeldeki <see cref="ISoftDeletable"/> uygulayan her entity tipi için global query
        /// filter (<c>e => !e.IsDeleted</c>) ekler. Generic <see cref="HasQueryFilter{TEntity}"/>
        /// metodu reflection ile her tip için çağrılır.
        /// </summary>
        private static void ApplySoftDeleteQueryFilters(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (entityType.BaseType is not null)
                {
                    // Türetilmiş (TPH) tipleri atla; filtre kök tipe uygulanır.
                    continue;
                }

                if (!typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
                {
                    continue;
                }

                MethodInfo method = typeof(ZnBlogDbContext)
                    .GetMethod(nameof(HasSoftDeleteQueryFilter), BindingFlags.NonPublic | BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);

                method.Invoke(null, new object[] { modelBuilder });
            }
        }

        /// <summary>
        /// Verilen <typeparamref name="TEntity"/> için <c>e => !e.IsDeleted</c> global query
        /// filter'ını tip güvenli biçimde kurar.
        /// </summary>
        private static void HasSoftDeleteQueryFilter<TEntity>(ModelBuilder modelBuilder)
            where TEntity : class, ISoftDeletable
        {
            Expression<Func<TEntity, bool>> filter = e => !e.IsDeleted;
            modelBuilder.Entity<TEntity>().HasQueryFilter(filter);
        }
    }
}
