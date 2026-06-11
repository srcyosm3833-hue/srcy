using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Zn.ClientWebApi.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Collection fixture: tek bir TestContainer ayağa kalkar, tüm test sınıfları paylaşır.
    /// Her test kendi kullanıcısını unique e-posta ile register eder → paralel çalışmaya dayanıklı.
    /// </summary>
    public sealed class AuthApiFixture : IAsyncLifetime
    {
        public AuthWebApplicationFactory Factory { get; } = new();

        public HttpClient CreateClient() =>
            Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

        public static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public async Task InitializeAsync() => await Factory.InitializeAsync();
        public async Task DisposeAsync() => await Factory.DisposeAsync();

        /// <summary>Her testin benzersiz test kullanıcı e-postası üretmesi için yardımcı.</summary>
        public static string UniqueEmail(string prefix = "testuser") =>
            $"{prefix}-{Guid.NewGuid():N}@integration.test";
    }

    /// <summary>xUnit collection tanımı: tüm auth integration testleri tek container paylaşır.</summary>
    [CollectionDefinition(CollectionName)]
    public sealed class AuthApiCollection : ICollectionFixture<AuthApiFixture>
    {
        public const string CollectionName = "AuthApi";
    }
}
