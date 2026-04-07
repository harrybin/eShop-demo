using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Webhooks.API.Extensions;

namespace Webhooks.API;

public static class WebHooksApi
{
    public static RouteGroupBuilder MapWebHooksApiV1(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/webhooks").HasApiVersion(1.0);

        api.MapGet("/", async (WebhooksContext context, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var data = await context.Subscriptions.Where(s => s.UserId == userId).ToListAsync(ct);
            return TypedResults.Ok(data);
        });

        api.MapGet("/{id:int}", async Task<Results<Ok<WebhookSubscription>, NotFound<string>>> (
            WebhooksContext context,
            ClaimsPrincipal user,
            int id,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var subscription = await context.Subscriptions
                .SingleOrDefaultAsync(s => s.Id == id && s.UserId == userId, ct);
            if (subscription != null)
            {
                return TypedResults.Ok(subscription);
            }
            return TypedResults.NotFound($"Subscriptions {id} not found");
        });

        api.MapPost("/", async Task<Results<Created, BadRequest<string>, Conflict<string>>> (
            WebhookSubscriptionRequest request,
            IGrantUrlTesterService grantUrlTester,
            WebhooksContext context,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var webhookType = Enum.Parse<WebhookType>(request.Event, ignoreCase: true);

            // Idempotency: prevent duplicate subscriptions for the same URL + event type per user.
            var alreadyExists = await context.Subscriptions
                .AnyAsync(s => s.UserId == userId && s.DestUrl == request.Url && s.Type == webhookType, ct);

            if (alreadyExists)
            {
                return TypedResults.Conflict($"A subscription for event '{request.Event}' to '{request.Url}' already exists.");
            }

            var grantOk = await grantUrlTester.TestGrantUrl(request.Url, request.GrantUrl, request.Token ?? string.Empty);

            if (grantOk)
            {
                var subscription = new WebhookSubscription()
                {
                    Date = DateTime.UtcNow,
                    DestUrl = request.Url,
                    Token = request.Token,
                    Type = webhookType,
                    UserId = userId
                };

                context.Add(subscription);
                await context.SaveChangesAsync(ct);

                return TypedResults.Created($"/api/webhooks/{subscription.Id}");
            }
            else
            {
                return TypedResults.BadRequest($"Invalid grant URL: {request.GrantUrl}");
            }
        })
        .ValidateWebhookSubscriptionRequest();

        api.MapDelete("/{id:int}", async Task<Results<Accepted, NotFound<string>>> (
            WebhooksContext context,
            ClaimsPrincipal user,
            int id,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var subscription = await context.Subscriptions.SingleOrDefaultAsync(s => s.Id == id && s.UserId == userId, ct);

            if (subscription != null)
            {
                context.Remove(subscription);
                await context.SaveChangesAsync(ct);
                return TypedResults.Accepted($"/api/webhooks/{subscription.Id}");
            }

            return TypedResults.NotFound($"Subscriptions {id} not found");
        });

        return api;
    }
}
