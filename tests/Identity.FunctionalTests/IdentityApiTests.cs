namespace eShop.Identity.FunctionalTests;

public sealed class IdentityApiTests : IClassFixture<IdentityApiFixture>
{
    private readonly HttpClient _httpClient;

    public IdentityApiTests(IdentityApiFixture fixture)
    {
        _httpClient = fixture.CreateDefaultClient();
    }

    [Fact]
    public async Task AliveEndpointReturnsSuccess()
    {
        var response = await _httpClient.GetAsync("/alive", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HomePageReturnsSuccess()
    {
        var response = await _httpClient.GetAsync("/", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task OpenIdDiscoveryDocumentReturnsIssuerMetadata()
    {
        var response = await _httpClient.GetAsync("/.well-known/openid-configuration", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        using var doc = JsonDocument.Parse(body);
        Assert.True(doc.RootElement.TryGetProperty("issuer", out _));
        Assert.True(doc.RootElement.TryGetProperty("token_endpoint", out _));
    }
}
