using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace RetailStore.Tests.Integration.Api;

/// <summary>
/// HTTP-level tests for UsersController (api/v1/users).
/// Register and Login are [AllowAnonymous]; remaining endpoints require auth.
/// InvalidCredentials → DomainErrorType.Unauthorized → 401.
/// </summary>
public class UsersApiTests : IntegrationTestBase, IClassFixture<RetailStoreWebAppFactory>
{
    private const string Base = "/api/v1/users";

    // Unique suffix for each test to avoid unique-index collisions inside the shared DB
    private static string UniqueEmail() => $"u-{Guid.NewGuid().ToString("N")[..10]}@test.com";
    private static string UniqueUsername() => $"user{Guid.NewGuid().ToString("N")[..8]}";

    public UsersApiTests(RetailStoreWebAppFactory factory) : base(factory) { }

    // ── POST /api/v1/users/register ───────────────────────────────────────────

    [Fact]
    public async Task Register_ValidUser_Returns201()
    {
        var response = await CreateAnonymousClient().PostAsJsonAsync(
            $"{Base}/register",
            new { username = UniqueUsername(), email = UniqueEmail(), password = "Password123!" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        var email = UniqueEmail();
        var client = CreateAnonymousClient();
        (await client.PostAsJsonAsync($"{Base}/register",
            new { username = UniqueUsername(), email, password = "Password123!" }))
            .EnsureSuccessStatusCode();

        var response = await client.PostAsJsonAsync($"{Base}/register",
            new { username = UniqueUsername(), email, password = "Password123!" });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_PasswordTooShort_Returns400()
    {
        var response = await CreateAnonymousClient().PostAsJsonAsync(
            $"{Base}/register",
            new { username = UniqueUsername(), email = UniqueEmail(), password = "abc" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_UsernameTooShort_Returns400()
    {
        var response = await CreateAnonymousClient().PostAsJsonAsync(
            $"{Base}/register",
            new { username = "ab", email = UniqueEmail(), password = "Password123!" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── POST /api/v1/users/login ──────────────────────────────────────────────

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithJwt()
    {
        var (email, password) = await RegisterUserAsync();

        var response = await CreateAnonymousClient().PostAsJsonAsync(
            $"{Base}/login", new { email, password });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.RefreshToken.Should().NotBeNullOrWhiteSpace();
        body.UserId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        // InvalidCredentials → DomainErrorType.Unauthorized → HTTP 401
        var (email, _) = await RegisterUserAsync();

        var response = await CreateAnonymousClient().PostAsJsonAsync(
            $"{Base}/login", new { email, password = "WrongPassword!" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_UnknownEmail_Returns401()
    {
        var response = await CreateAnonymousClient().PostAsJsonAsync(
            $"{Base}/login", new { email = "nobody@example.com", password = "Password123!" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_EmptyEmail_Returns400()
    {
        var response = await CreateAnonymousClient().PostAsJsonAsync(
            $"{Base}/login", new { email = "", password = "Password123!" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── GET /api/v1/users ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetUsers_AdminClient_Returns200WithList()
    {
        var response = await CreateAdminClient().GetAsync(Base);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();
        users.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUsers_AnonymousClient_Returns401()
    {
        var response = await CreateAnonymousClient().GetAsync(Base);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── GET /api/v1/users/roles ───────────────────────────────────────────────

    [Fact]
    public async Task GetRoles_AdminClient_Returns200WithSeededRoles()
    {
        // DatabaseSeeder seeds Admin and Staff roles on startup
        var response = await CreateAdminClient().GetAsync($"{Base}/roles");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var roles = await response.Content.ReadFromJsonAsync<List<RoleDto>>();
        roles!.Should().Contain(r => r.Name == "Admin");
        roles.Should().Contain(r => r.Name == "Staff");
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    private async Task<(string email, string password)> RegisterUserAsync()
    {
        var email = UniqueEmail();
        const string password = "Password123!";

        (await CreateAnonymousClient().PostAsJsonAsync(
            $"{Base}/register",
            new { username = UniqueUsername(), email, password }))
            .EnsureSuccessStatusCode();

        return (email, password);
    }

    // ── Local DTOs ────────────────────────────────────────────────────────────

    private record LoginResponse(string AccessToken, string RefreshToken,
        DateTime ExpiresAt, Guid UserId, string Username);

    private record UserDto(Guid Id, string Username, string Email, bool IsActive);

    private record RoleDto(Guid Id, string Name, string Description);
}
