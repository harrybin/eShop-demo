namespace eShop.PaymentProcessor.FunctionalTests;

public sealed class PaymentProcessorApiTests : IClassFixture<PaymentProcessorApiFixture>
{
    private readonly HttpClient _httpClient;

    public PaymentProcessorApiTests(PaymentProcessorApiFixture fixture)
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
