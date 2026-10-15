using Contracts;

namespace API.Middleware;

public class UserContextMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;
    private const string CorrelationIdKey = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context, IUserContext userContext)
    {
        var claims = context.User.Claims;
        if (!context.User.HasClaim(x => x.Type.Equals("userId")))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Insufficient claims in user Claims. Expecting required userId.");
            return;
        }

        if (context.Items.TryGetValue(CorrelationIdKey, out var correlationId) && correlationId != null)
            userContext.CorrelationId = Guid.Parse(correlationId.ToString()!);
        else
        {
            var newCorrelationId = Guid.NewGuid();
            context.Items["X-Correlation-Id"] = newCorrelationId;
            userContext.CorrelationId = newCorrelationId;
        }
        userContext.UserId = Guid.Parse(claims.SingleOrDefault(x => x.Type == "userId")!.Value);

        await _next(context);
    }
}