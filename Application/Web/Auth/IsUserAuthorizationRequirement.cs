using System.Security.Claims;
using System.Threading.Tasks;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Split.Domain.Primitives;
using Split.Domain.User;
using Split.Domain.User.Events;

namespace Split.Application.Web.Auth;

public class IsUserAuthorizationRequirement : IAuthorizationRequirement;

public class IsUserAuthorizationHandler(ILogger<IsUserAuthorizationHandler> logger, IMediator mediator)
    : AuthorizationHandler<IsUserAuthorizationRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        IsUserAuthorizationRequirement requirement
    )
    {
        if (context.User.Identity is not { IsAuthenticated: true })
        {
            logger.LogDebug("User is not authenticated");
            return;
        }

        var userClaimId = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaimId is null)
        {
            logger.LogDebug("User ID claim not found");
            return;
        }
        var userId = new UserId($"{userClaimId.Issuer}:{userClaimId.Value}");

        var userQueryResult = await mediator.Send(new UserQuery(userId));
        if (userQueryResult is not { User: UserAggregate })
        {
            logger.LogDebug("User not found");
            return;
        }

        context.Succeed(requirement);
    }
}

public static class IsUserAuthorizationServiceCollectionExtensions
{
    public static IServiceCollection AddIsUserAuthorization(this IServiceCollection services)
    {
        services
            .AddScoped<IAuthorizationHandler, IsUserAuthorizationHandler>()
            .AddAuthorizationBuilder()
            .AddDefaultPolicy(
                "IsUserPolicy",
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new IsUserAuthorizationRequirement());
                }
            );
        return services;
    }
}
