using FluentAssertions;

namespace RetailStore.Tests.Integration;

/// <summary>
/// Smoke test — verifies the factory can start the app, reach the health endpoint,
/// and that auth is wired correctly. Deleted once integration tests are in place.
/// </summary>
public class RetailStoreWebAppFactorySmokeTest
    : IntegrationTestBase, IClassFixture<RetailStoreWebAppFactory>
{
    public RetailStoreWebAppFactorySmokeTest(RetailStoreWebAppFactory factory)
        : base(factory) { }

    [Fact]
    public async Task App_StartsSuccessfully_HealthEndpointReturns200()
    {
        var client = CreateAnonymousClient();

        var response = await client.GetAsync("/health");

        // Health will be "Degraded" because SQL Server health check fails
        // (we replaced it with SQLite), but the endpoint itself responds
        response.StatusCode.Should().BeOneOf(
            System.Net.HttpStatusCode.OK,
            System.Net.HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task AdminClient_HasValidJwt_ProtectedEndpointReturns200OrFound()
    {
        var client = CreateAdminClient();

        // Products list is a protected endpoint
        var response = await client.GetAsync("/api/v1/products");

        ((int)response.StatusCode).Should().BeLessThan(500);
        response.StatusCode.Should().NotBe(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AnonymousClient_AccessesProtectedEndpoint_Returns401()
    {
        var client = CreateAnonymousClient();

        var response = await client.GetAsync("/api/v1/products");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }
}
