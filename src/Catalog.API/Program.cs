using Asp.Versioning.Builder;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddApplicationServices();
builder.Services.AddProblemDetails();

// Add output caching for catalog endpoints
builder.Services.AddOutputCache(options =>
{
    // Cache catalog items list for 1 hour, invalidate on mutations
    options.AddPolicy("catalog-items-1h", builder => builder
        .Expire(TimeSpan.FromHours(1))
        .Tag("catalog-items"));

    // Cache individual item by ID for 1 hour
    options.AddPolicy("catalog-item-by-id-1h", builder => builder
        .Expire(TimeSpan.FromHours(1))
        .Tag("catalog-item"));

    // Cache catalog types/brands for 24 hours (rarely change)
    options.AddPolicy("catalog-types-brands-24h", builder => builder
        .Expire(TimeSpan.FromHours(24))
        .Tag("catalog-metadata"));
});

var withApiVersioning = builder.Services.AddApiVersioning();

builder.AddDefaultOpenApi(withApiVersioning);

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseStatusCodePages();

// Use output caching
app.UseOutputCache();

app.MapCatalogApi();

app.UseDefaultOpenApi();
app.Run();
