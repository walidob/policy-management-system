using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace PolicyManagementApp.Api.Extensions
{
    public static class ApiServiceExtensions
    {
        public static IServiceCollection AddApiRateLimiting(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? httpContext.Request.Headers.Host.ToString(),
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 100,
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

        public static IServiceCollection AddApiOutputCache(this IServiceCollection services)
        {
            services.AddOutputCache(options =>
            {
                options.DefaultExpirationTimeSpan = TimeSpan.FromMinutes(5);
                options.AddBasePolicy(builder => builder.Tag("api-all"));

                options.AddPolicy("Policies", builder =>
                    builder.Tag("policies")
                          .Expire(TimeSpan.FromMinutes(10)));

                options.AddPolicy("Tenants", builder =>
                    builder.Tag("tenants")
                          .Expire(TimeSpan.FromMinutes(15)));

                options.AddPolicy("Clients", builder =>
                    builder.Tag("clients")
                          .Expire(TimeSpan.FromMinutes(10)));
            });

            return services;
        }


    }
}
