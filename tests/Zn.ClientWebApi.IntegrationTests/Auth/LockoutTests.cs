using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Zn.ClientWebApi.IntegrationTests.Infrastructure;

namespace Zn.ClientWebApi.IntegrationTests.Auth
{
    /// <summary>
    /// POST /api/auth/login — account lockout after 5 consecutive failed attempts.
    /// Scenario: F1-T1 #8
    ///
    /// Isolation note: this test registers its own dedicated user so that
    /// failed login attempts from other test classes cannot interfere.
    /// The lockout window is 5 minutes; because we never use the correct
    /// password within the 5 failing attempts, the lock is triggered on the
    /// 6th attempt (with the correct password) — no sleep is needed.
    /// </summary>
    [Collection(AuthApiCollection.CollectionName)]
    public sealed class LockoutTests
    {
        private readonly HttpClient _client;

        public LockoutTests(AuthApiFixture fixture)
        {
            _client = fixture.CreateClient();
        }

        [Fact]
        public async Task Login_AfterFiveConsecutiveFailures_Returns423OnSixthAttempt()
        {
            // Arrange — isolated user for this test only
            string email = AuthApiFixture.UniqueEmail("lockout-user");
            const string correctPassword = "Correct@Lockout1";
            const string wrongPassword = "Wrong@Lockout9";

            // Identity config: MaxFailedAccessAttempts = 5.
            // The account is locked AFTER the 5th wrong attempt.
            // Attempts 1–4: 401 (not yet locked)
            // Attempt 5:    may return 401 or 423 depending on exact Identity implementation
            // Attempt 6 (correct password): must return 423 (locked)
            const int attemptsBeforeLockConfirmed = 4;

            // Register the user
            HttpResponseMessage reg = await _client.RegisterAsync(
                new RegisterRequest(Email: email, Password: correctPassword));
            reg.StatusCode.Should().Be(HttpStatusCode.Created, "user must be created before lockout test");

            // Act — make 4 clearly non-lockout wrong-password attempts (all must be 401)
            for (int i = 0; i < attemptsBeforeLockConfirmed; i++)
            {
                HttpResponseMessage failedAttempt = await _client.LoginAsync(email, wrongPassword);
                failedAttempt.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
                    because: $"attempt #{i + 1} with wrong password should return 401 before lockout triggers");
            }

            // Make one more wrong attempt to reach / exceed the lockout threshold
            await _client.LoginAsync(email, wrongPassword);

            // The next attempt — even with the CORRECT password — must be locked out (423)
            HttpResponseMessage lockedResponse = await _client.LoginAsync(email, correctPassword);

            // Assert
            ((int)lockedResponse.StatusCode).Should().Be(423,
                because: "after reaching MaxFailedAccessAttempts the account is locked; " +
                         "even a correct password must return 423 until the lockout window expires");
        }
    }
}
