using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zn.Domain.Authorization;
using Zn.Domain.Entity;
using Zn.Persistence.Context;

namespace Zn.ClientWebApi.Seeding
{
    /// <summary>
    /// Uygulama açılışında çalışan, idempotent Identity seed mekanizması.
    /// Eksik rolleri (<see cref="RoleNames.All"/>) ve yapılandırılmışsa ilk admin
    /// kullanıcısını oluşturur. Tekrar çalıştırıldığında var olanları atlar; bu yüzden
    /// her başlatmada güvenle çağrılabilir (testler dahil).
    /// </summary>
    public static class IdentityDataSeeder
    {
        /// <summary>
        /// Yeni bir DI scope açıp rolleri ve admin kullanıcısını seed eder.
        /// Program.cs'te <c>app.Run()</c> öncesinde çağrılır.
        /// </summary>
        public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
        {
            using IServiceScope scope = services.CreateScope();
            IServiceProvider provider = scope.ServiceProvider;

            ILogger logger = provider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(typeof(IdentityDataSeeder).FullName!);

            var dbContext = provider.GetRequiredService<ZnBlogDbContext>();
            var roleManager = provider.GetRequiredService<RoleManager<Role>>();
            var userManager = provider.GetRequiredService<UserManager<User>>();
            AdminUserOptions adminOptions = provider
                .GetRequiredService<IOptions<AdminUserOptions>>().Value;

            // Seed öncesi şemanın hazır olduğunu garanti et. Bekleyen migration yoksa
            // (üretimde elle uygulanmışsa) MigrateAsync no-op'tur; idempotenttir. Bu sayede
            // seed, henüz migrate edilmemiş bir veritabanına erişmeye çalışıp patlamaz.
            await dbContext.Database.MigrateAsync(cancellationToken);

            await SeedRolesAsync(roleManager, logger);
            await SeedAdminUserAsync(userManager, adminOptions, logger);
        }

        /// <summary>Tanımlı tüm rolleri idempotent olarak oluşturur.</summary>
        private static async Task SeedRolesAsync(RoleManager<Role> roleManager, ILogger logger)
        {
            foreach (string roleName in RoleNames.All)
            {
                if (await roleManager.RoleExistsAsync(roleName))
                {
                    continue;
                }

                IdentityResult result = await roleManager.CreateAsync(new Role { Name = roleName });
                if (result.Succeeded)
                {
                    logger.LogInformation("Seeded role '{Role}'.", roleName);
                }
                else
                {
                    logger.LogWarning(
                        "Failed to seed role '{Role}': {Errors}",
                        roleName,
                        string.Join("; ", result.Errors.Select(e => e.Description)));
                }
            }
        }

        /// <summary>
        /// Yapılandırılmışsa ilk admin kullanıcısını oluşturur ve Admin rolüne atar.
        /// E-posta/şifre verilmemişse (örn. test ortamı) sessizce atlanır.
        /// Kullanıcı zaten varsa yalnızca Admin rolünde olduğundan emin olunur.
        /// </summary>
        private static async Task SeedAdminUserAsync(
            UserManager<User> userManager, AdminUserOptions options, ILogger logger)
        {
            if (!options.IsConfigured)
            {
                logger.LogInformation(
                    "AdminUser is not configured; skipping admin user seeding.");
                return;
            }

            User? existing = await userManager.FindByEmailAsync(options.Email!);
            if (existing is not null)
            {
                // İdempotentlik: kullanıcı var ama role atanmamış olabilir → garanti altına al.
                await EnsureInAdminRoleAsync(userManager, existing, logger);
                return;
            }

            var admin = new User
            {
                UserName = options.Email,
                Email = options.Email,
                EmailConfirmed = true,
                FirstName = options.FirstName,
                LastName = options.LastName,
                ImageUrl = options.ImageUrl
            };

            IdentityResult createResult = await userManager.CreateAsync(admin, options.Password!);
            if (!createResult.Succeeded)
            {
                logger.LogError(
                    "Failed to seed admin user '{Email}': {Errors}",
                    options.Email,
                    string.Join("; ", createResult.Errors.Select(e => e.Description)));
                return;
            }

            await EnsureInAdminRoleAsync(userManager, admin, logger);
            logger.LogInformation("Seeded admin user '{Email}'.", options.Email);
        }

        /// <summary>Kullanıcının Admin rolünde olduğundan emin olur (idempotent).</summary>
        private static async Task EnsureInAdminRoleAsync(
            UserManager<User> userManager, User user, ILogger logger)
        {
            if (await userManager.IsInRoleAsync(user, RoleNames.Admin))
            {
                return;
            }

            IdentityResult roleResult = await userManager.AddToRoleAsync(user, RoleNames.Admin);
            if (!roleResult.Succeeded)
            {
                logger.LogError(
                    "Failed to add user '{Email}' to '{Role}': {Errors}",
                    user.Email,
                    RoleNames.Admin,
                    string.Join("; ", roleResult.Errors.Select(e => e.Description)));
            }
        }
    }
}
