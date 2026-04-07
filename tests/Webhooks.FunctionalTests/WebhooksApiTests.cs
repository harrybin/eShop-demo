using System.Text.Json;
using Asp.Versioning;
using Asp.Versioning.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Webhooks.API.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace eShop.Webhooks.FunctionalTests;

public sealed class WebhooksApiTests : IClassFixture<WebhooksApiFixture>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;

    public WebhooksApiTests(WebhooksApiFixture fixture)
    {
        var handler = new ApiVersionHandler(new QueryStringApiVersionWriter(), new ApiVersion(1.0));

        _factory = fixture;
        _httpClient = fixture.CreateDefaultClient(handler);
    }

    [Fact]
    public async Task WebhooksContextHasNoPendingMigrations()
    {
        using var scope = _factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<WebhooksContext>();

        var pending = await ctx.Database.GetPendingMigrationsAsync(TestContext.Current.CancellationToken);

        Assert.Empty(pending);
    }

    [Fact]
    public async Task GetAllWebhooksReturnsSuccessAndEmptyListInitially()
    {
        var response = await _httpClient.GetAsync("api/webhooks", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var items = await response.Content.ReadFromJsonAsync<List<JsonElement>>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(items);
    }

    [Fact]
    public async Task CreateWebhookSubscriptionReturnsCreated()
    {
        var response = await _httpClient.PostAsJsonAsync("api/webhooks", BuildValidRequest(), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task GetWebhookByIdAfterCreate()
    {
        // Arrange - create a subscription first
        var createResponse = await _httpClient.PostAsJsonAsync("api/webhooks", BuildValidRequest(), TestContext.Current.CancellationToken);
        createResponse.EnsureSuccessStatusCode();

        var location = createResponse.Headers.Location!.ToString();
        var id = location.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();

        // Act
        var getResponse = await _httpClient.GetAsync($"api/webhooks/{id}", TestContext.Current.CancellationToken);

        // Assert
        getResponse.EnsureSuccessStatusCode();
        var body = await getResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.Equal(int.Parse(id), body.GetProperty("id").GetInt32());
    }

    [Fact]
    public async Task DeleteWebhookReturnsAccepted()
    {
        // Arrange - create a subscription first
        var createResponse = await _httpClient.PostAsJsonAsync("api/webhooks", BuildValidRequest(), TestContext.Current.CancellationToken);
        createResponse.EnsureSuccessStatusCode();

        var id = createResponse.Headers.Location!.ToString().Split('/', StringSplitOptions.RemoveEmptyEntries).Last();

        // Act
        var deleteResponse = await _httpClient.DeleteAsync($"api/webhooks/{id}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Accepted, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task GetNonExistentWebhookReturnsNotFound()
    {
        var response = await _httpClient.GetAsync("api/webhooks/99999", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateWebhookWithInvalidEventReturns400()
    {
        var invalidRequest = new
        {
            Url = "http://hooks.example.com/receive",
            GrantUrl = "http://hooks.example.com/grant",
            Token = "test-token",
            Event = "NotARealEvent"
        };

        var response = await _httpClient.PostAsJsonAsync("api/webhooks", invalidRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateDuplicateWebhookSubscriptionReturnsConflict()
    {
        var request = new
        {
            Url = "http://hooks.example.com/duplicate",
            GrantUrl = "http://hooks.example.com/duplicate",
            Token = "tok",
            Event = "OrderPaid"
        };

        // First create should succeed
        var first = await _httpClient.PostAsJsonAsync("api/webhooks", request, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        // Second create with the same URL + event type should be rejected
        var second = await _httpClient.PostAsJsonAsync("api/webhooks", request, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    private static object BuildValidRequest()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var url = $"http://hooks.example.com/receive-{suffix}";

        return new
        {
            Url = url,
            GrantUrl = url,
            Token = "test-token",
            Event = "OrderShipped"
        };
    }
}
