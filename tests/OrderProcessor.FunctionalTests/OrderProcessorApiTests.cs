namespace eShop.OrderProcessor.FunctionalTests;

public sealed class OrderProcessorApiTests : IClassFixture<OrderProcessorApiFixture>
{
    private readonly HttpClient _httpClient;

    public OrderProcessorApiTests(OrderProcessorApiFixture fixture)
    {
        _httpClient = fixture.CreateDefaultClient();
    }

    [Fact]
    public async Task AliveEndpointReturnsSuccess()
    {
        var response = await _httpClient.GetAsync("/alive", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
