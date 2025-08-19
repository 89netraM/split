using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Scalar.AspNetCore;

namespace Split.Application.Api;

public static class OpenApi
{
    private const string OpenApiRoutePattern = "/api/openapi/{documentName}.json";

    public static IEndpointRouteBuilder MapOpenApi(this IEndpointRouteBuilder app)
    {
        OpenApiEndpointRouteBuilderExtensions.MapOpenApi(app, OpenApiRoutePattern);
        app.MapScalarApiReference(
            "/api/openapi",
            (options, context) =>
            {
                options.OpenApiRoutePattern = OpenApiRoutePattern;
                if (
                    context.Request.Headers.TryGetValue("X-Forwarded-Proto", out var protocols)
                    && context.Request.Headers.TryGetValue("X-Forwarded-Host", out var hosts)
                )
                {
                    options.Servers =
                    [
                        .. protocols.SelectMany(p => hosts.Select(h => new ScalarServer(p + "://" + h))),
                    ];
                }
                else
                {
                    context
                        .RequestServices.GetRequiredService<ILoggerFactory>()
                        .CreateLogger(nameof(OpenApi))
                        .LogWarning(
                            "Could not get X-Forwarded values with request, leaving server base addresses intact for Scalar UI."
                        );
                }
            }
        );
        return app;
    }
}
