using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zn.Application.Interfaces.Persistence;
using Zn.Domain.Entity;
using Zn.Persistence.Context;
using Zn.Persistence.Repositories;

namespace Zn.Persistence.Extensions
{
    /// <summary>
    /// Persistence katmanının DI kayıtlarını tek noktada toplar.
    /// WebApi tarafında Program.cs içinde
    /// builder.Services.AddPersistenceServices(builder.Configuration)
    /// çağrısı yeterlidir; katmanın iç detayları dışarı sızmaz.
    /// </summary>
    public static class PersistenceRegistration
    {
        public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            // DbContext kaydı: bağlantı cümlesi appsettings.json -> ConnectionStrings:DefaultConnection
            services.AddDbContext<ZnBlogDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Identity entegrasyonu: UserManager, RoleManager, SignInManager
            // servislerini DI konteynerine ekler ve store olarak EF Core'u kullanır.
            services.AddIdentity<User, Role>(options =>
            {
                // Parola politikası: minimum güvenlik çizgisi.
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;

                // Aynı e-posta ile birden çok hesap açılmasını engeller.
                options.User.RequireUniqueEmail = true;

                // Brute-force koruması: 5 hatalı denemede 5 dakika kilit.
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = System.TimeSpan.FromMinutes(5);
            })
            .AddEntityFrameworkStores<ZnBlogDbContext>()
            .AddDefaultTokenProviders();

            // Repository kayıtları: DbContext Scoped olduğu için repository de Scoped.
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IBlogRepository, BlogRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<ISubCommentRepository, SubCommentRepository>();
            services.AddScoped<IBlogLikeRepository, BlogLikeRepository>();
            services.AddScoped<ICommentLikeRepository, CommentLikeRepository>();
            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<IContactRepository, ContactRepository>();
            services.AddScoped<ISocialMediaRepository, SocialMediaRepository>();
            services.AddScoped<ISearchLogRepository, SearchLogRepository>();

            return services;
        }
    }
}
