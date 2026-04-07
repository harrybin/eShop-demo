public sealed class ValidRequestIdFilter : IEndpointFilter
{
    public async ValueTask<object> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var requestId = ResolveRequestId(context);

        if (requestId == Guid.Empty)
        {
            return TypedResults.BadRequest("RequestId is missing");
        }

        return await next(context);
    }

    private static Guid ResolveRequestId(EndpointFilterInvocationContext context)
    {
        foreach (var argument in context.Arguments)
        {
            if (argument is Guid guid)
            {
                return guid;
            }
        }

        if (context.HttpContext.Request.Headers.TryGetValue("x-requestid", out var headerValue)
            && Guid.TryParse(headerValue.ToString(), out var headerGuid))
        {
            return headerGuid;
        }

        if (context.HttpContext.Request.RouteValues.TryGetValue("requestId", out var routeValue)
            && routeValue is not null
            && Guid.TryParse(routeValue.ToString(), out var routeGuid))
        {
            return routeGuid;
        }

        if (context.HttpContext.Request.Query.TryGetValue("requestId", out var queryValue)
            && Guid.TryParse(queryValue.ToString(), out var queryGuid))
        {
            return queryGuid;
        }

        return Guid.Empty;
    }
}
