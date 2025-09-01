
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using System.Threading.Tasks;
using WeatherSrv.Models;

namespace WeatherSrv.Middleware
{
    public class JsonFormatMiddleware
    {
        private const int _limit = 10; // max requests per 10 seconds fixed window
        private static readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
        private readonly RequestDelegate _next;
        private readonly ILogger<JsonFormatMiddleware> _logger;

        private static readonly HashSet<string> _publicPaths = new()
            {
                "/api/v1/auth/login",
                "/api/v1/weather"
            };

        public JsonFormatMiddleware(RequestDelegate next, ILogger<JsonFormatMiddleware> logger)
        {
            this._next = next;
            this._logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (await HandleAuthorization(context) == false)
                return;

            if (await HandleRateLimiting(context) == false)
                return;

            HandleUserHeader(context);

            if (!await HandleJsonNormalization(context))
                return;

            await HandleRequestWithException(context);
        }
        private async Task<bool> HandleAuthorization(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();

            if (_publicPaths.Contains(path))
                return true;

            if (!(context.User.Identity?.IsAuthenticated ?? false))
            
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("User is not authenticated");
                return false;
            }

            // role check, not add role claim currently
            //if (!context.User.IsInRole("Admin"))
            //{
            //    context.Response.StatusCode = StatusCodes.Status403Forbidden;
            //    await context.Response.WriteAsync("Access denied");
            //    return false;
            //}

            return true;
        }

        private async Task<bool> HandleRateLimiting(HttpContext context)
        {//Ratelimit here
            var key = context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
            if (!_cache.TryGetValue(key, out int count))
            {
                count = 0;
            }

            _logger.LogInformation($"--> Request count: {count}");
            if (count >= _limit)
            {
                context.Response.StatusCode = 429; // Too Many Requests
                await context.Response.WriteAsync("Rate limit exceeded.");
                return false;
            }

            _cache.Set(key, count + 1, TimeSpan.FromSeconds(10));
            return true;
        }

        private void HandleUserHeader(HttpContext context)
        {
            //Authorization, can add more control based role and policy, 
            //currently just append the user name, just allowed retrieve logined user's data
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var username = context.User.Identity.Name ?? "";
                context.Request.Headers["x-userId"] = username;
            }
        }

        private async Task<bool> HandleJsonNormalization(HttpContext context)
        {
            //Json format
            if (context.Request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true &&
                (HttpMethods.IsPost(context.Request.Method) ||
                 HttpMethods.IsPut(context.Request.Method) ||
                 HttpMethods.IsPatch(context.Request.Method)))
            {
                context.Request.EnableBuffering();
                using var reader = new StreamReader(context.Request.Body, leaveOpen: true);//auto disposed
                var body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
                if (!string.IsNullOrEmpty(body))
                {
                    //try parse
                    try
                    {
                        var jsondoc = JsonDocument.Parse(body);
                        var root = jsondoc.RootElement;

                        //camelCase
                        var normalizedJson = JsonSerializer.Serialize(root, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        });
                        var bytes = System.Text.Encoding.UTF8.GetBytes(normalizedJson);
                        context.Request.Body = new MemoryStream(bytes);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError($"Bad request: {ex.Message}");
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsync($"Invalid Json format:{ex.Message}");
                        return false;
                    }
                }
            }
            return true;
        }

        private async Task HandleRequestWithException(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred");

                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";

                var result = System.Text.Json.JsonSerializer.Serialize(new
                {
                    error = "An unexpected error occurred.",
                    details = ex.Message // Optional
                });

                await context.Response.WriteAsync(result);
            }
        }
    }
}