using Aspire.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace eShop.PaymentProcessor.FunctionalTests;

public sealed class PaymentProcessorApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly IHost _app;

    public PaymentProcessorApiFixture()
    {
        var options = new DistributedApplicationOptions
        {
            AssemblyName = typeof(PaymentProcessorApiFixture).Assembly.FullName,
            DisableDashboard = true
        };

        _app = DistributedApplication.CreateBuilder(options).Build();
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                // Worker startup only needs a configured value for EventBus.
                { "ConnectionStrings:EventBus", "amqp://guest:guest@localhost:5672" }
            });
        });

        return base.CreateHost(builder);
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _app.StopAsync();
        if (_app is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync().ConfigureAwait(false);
        }
        else
        {
            _app.Dispose();
        }
    }

    public async ValueTask InitializeAsync()
    {
        await _app.StartAsync();
    }
}
