using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Webhooks.API.Services;

namespace eShop.Webhooks.FunctionalTests;

public sealed class WebhooksApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly IHost _app;

    public IResourceBuilder<PostgresServerResource> Postgres { get; private set; }

    private string _postgresConnectionString = string.Empty;

    public WebhooksApiFixture()
    {
        var options = new DistributedApplicationOptions
        {
            AssemblyName = typeof(WebhooksApiFixture).Assembly.FullName,
            DisableDashboard = true
        };
        var appBuilder = DistributedApplication.CreateBuilder(options);
        Postgres = appBuilder.AddPostgres("WebhooksDB")
            .WithImage("postgres")
            .WithImageTag("16-alpine");
        _app = appBuilder.Build();
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                // WebhooksContext uses "webhooksdb" connection string key
                { "ConnectionStrings:webhooksdb", _postgresConnectionString },
                // Provide a dummy eventbus connection string to suppress startup errors;
                // the CRUD endpoints under test don't require a live RabbitMQ broker.
                { "ConnectionStrings:eventbus", "amqp://guest:guest@localhost:5672" }
            });
        });
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IStartupFilter>(new AutoAuthorizeStartupFilter());
            // Replace the real grant-URL tester with a stub so POST tests don't make
            // outbound HTTP requests to non-existent hosts.
            services.AddTransient<IGrantUrlTesterService, AlwaysGrantUrlTesterService>();
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
        _postgresConnectionString = await Postgres.Resource.GetConnectionStringAsync();
    }

    private sealed class AlwaysGrantUrlTesterService : IGrantUrlTesterService
    {
        public Task<bool> TestGrantUrl(string urlHook, string url, string token) =>
            Task.FromResult(true);
    }

    private sealed class AutoAuthorizeStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                builder.UseMiddleware<AutoAuthorizeMiddleware>();
                next(builder);
            };
        }
    }
}
