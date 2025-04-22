using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace PolicyManagementApp.Api.Extensions
{
    public static class ApiServiceExtensions
    {
        public static IServiceCollection AddApiRateLimiting(this IServiceCollection services)
        {
            //EXAMPLE OF THIS IN OUR API:Failed to load policies: Http failure response for https://127.0.0.1:52619/api/policies?pageNumber=1&pageSize=10&sortColumn=id&sortDirection=asc: 429 Too Many Requests
            services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? httpContext.Request.Headers.Host.ToString(),
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 50,
                            QueueLimit = 0,
                            Window = TimeSpan.FromMinutes(1)
                        }));

                options.AddFixedWindowLimiter("api_policy", options =>
                {
                    options.PermitLimit = 30;
                    options.Window = TimeSpan.FromMinutes(1);
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = 0;
                });

                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.Headers["Retry-After"] = "60";
                    await context.HttpContext.Response.WriteAsJsonAsync(new
                    {
                        title = "Too Many Requests",
                        status = 429,
                        detail = "You've exceeded the rate limit. Please try again later."
                    }, cancellationToken);
                };
            });

            return services;
        }
    }
}
