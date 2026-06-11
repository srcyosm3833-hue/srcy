using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Wolverine;
using Wolverine.FluentValidation;
using Zn.Application.Extensions;
using Zn.ClientWebApi.Extensions;
using Zn.ClientWebApi.Middleware;
using Zn.ClientWebApi.Seeding;
using Zn.Domain.Entity;
using Zn.Infrastructure.Extensions;
using Zn.Infrastructure.Storage;
using Zn.Persistence.Context;
using Zn.Persistence.Extensions;

var builder = WebApplication.CreateBuilder(args);

// --- Wolverine (mediator) host entegrasyonu ---
// Handler keşfi için Application assembly'si taramaya dahil edilir.
builder.Host.UseWolverine(opts =>
{
    opts.Discovery.IncludeAssembly(ApplicationRegistration.ApplicationAssembly);

    // FluentValidation middleware: handler invoke edilmeden önce ilgili
    // validator'ları çalıştırır; hata varsa ValidationException fırlatılır ve
    // GlobalExceptionHandler bunu 400 ProblemDetails'e çevirir.
    opts.UseFluentValidation();

    // Wolverine 6 varsayılan olarak handler'larda service location'a izin vermez
    // (codegen ile bağımlılıkları doğrudan resolve eder). UserManager/SignInManager
    // "opaque lambda factory" ile kayıtlı olduğundan codegen edilemezler; bunlar için
    // service location'a izin veriyoruz. Diğer bağımlılıklar optimize codegen ile gelir.
    opts.CodeGeneration.AlwaysUseServiceLocationFor<UserManager<User>>();
    opts.CodeGeneration.AlwaysUseServiceLocationFor<SignInManager<User>>();

    // EF Core'un DbContextOptions'ı da opaque lambda factory ile kayıtlıdır ve
    // repository → DbContext zinciri üzerinden handler bağımlılıklarına sızar.
    // DbContext'i service location köküne alarak tüm zinciri tek noktada çözeriz.
    opts.CodeGeneration.AlwaysUseServiceLocationFor<ZnBlogDbContext>();
});

// --- Katman DI kayıtları ---
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);

// --- İlk admin kullanıcı seed ayarları (rol seed'i sabit; admin user opsiyoneldir) ---
builder.Services.Configure<AdminUserOptions>(
    builder.Configuration.GetSection(AdminUserOptions.SectionName));

// --- Dosya depolama kök yolu: WebRootPath (wwwroot) altında "uploads" ---
// Infrastructure ASP.NET hosting tiplerini bilmediğinden, fiziksel kök yolu burada,
// host ortamına göre belirleyip FileStorageOptions'a post-configure ediyoruz.
// WebRootPath yoksa (wwwroot klasörü mevcut değilse) ContentRoot altında üretilir.
string webRootPath = builder.Environment.WebRootPath
    ?? Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
string uploadsRootPath = Path.Combine(webRootPath, "uploads");
Directory.CreateDirectory(uploadsRootPath);

builder.Services.PostConfigure<FileStorageOptions>(options =>
{
    if (string.IsNullOrWhiteSpace(options.RootPath))
    {
        options.RootPath = uploadsRootPath;
    }
});

// --- API servisleri ---
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// --- AuthN/AuthZ, CORS ---
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddCorsPolicies();

// --- Global hata yönetimi (ProblemDetails + IExceptionHandler) ---
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

// --- Açılışta idempotent seed: roller + (yapılandırılmışsa) ilk admin kullanıcı ---
// Migration testlerde WebApplicationFactory tarafından ayrıca uygulanır; burada yalnızca
// rol/kullanıcı seed'i yapılır. Idempotent olduğundan her başlatmada güvenle çalışır.
await IdentityDataSeeder.SeedAsync(app.Services);

// --- HTTP pipeline ---
// Exception handler en başta olmalı ki alt katmanların hataları yakalanabilsin.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Yüklenen görselleri (wwwroot/uploads → /uploads) statik dosya olarak servis eder.
// Kimlik doğrulamadan önce gelir: yüklenen görseller herkese açık okunabilir.
app.UseStaticFiles();

app.UseCors(CorsRegistration.DevelopmentPolicy);

// Sıralama kritik: önce kimlik doğrulama, sonra yetkilendirme.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// WebApplicationFactory<Program> erişimi için: test projesi bu sınıfa referans verir.
// Minimal dokunuş — üretim davranışını değiştirmez.
public partial class Program { }
