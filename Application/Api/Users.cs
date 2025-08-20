using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Split.Domain.Primitives;
using Split.Domain.Transaction.Events;
using Split.Domain.User;
using Split.Domain.User.Events;

namespace Split.Application.Api;

public static class Users
{
    public static RouteGroupBuilder MapUsersEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        var group = routeBuilder.MapGroup("/users");

        group.MapGet("/me", GetMe).Produces<User>().WithDescription("Gets info about the requesting user.");

        group
            .MapGet("/me/associates", GetMyAssociates)
            .Produces<IEnumerable<User>>()
            .WithDescription("Gets other users that the requesting user has interacted with previously.");

        group
            .MapGet("/me/balances", GetBalances)
            .Produces<IEnumerable<Balance>>()
            .WithDescription("Gets the balances of the requesting user compared to all associated users.");

        return group;
    }

    private static async Task<IResult> GetMe(
        [FromServices] ISender sender,
        [FromServices] HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        if (httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) is not string id)
        {
            return Results.BadRequest("Subject not found on JWT.");
        }

        var userId = new UserId(id);
        var response = await sender.Send(new UserQuery(userId), cancellationToken);
        return response.User is { } user ? Results.Ok(User.FromDomain(user)) : Results.NotFound();
    }

    private static async Task<IResult> GetMyAssociates(
        [FromServices] ISender sender,
        [FromServices] HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        if (httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) is not string id)
        {
            return Results.BadRequest("Subject not found on JWT.");
        }

        var userId = new UserId(id);
        var stream = sender.CreateStream(new UserRelationshipQuery(userId), cancellationToken);
        var users = await stream.Select(r => User.FromDomain(r.User)).ToArrayAsync(cancellationToken);
        return Results.Ok(users);
    }

    private static async Task<IResult> GetBalances(
        [FromServices] ISender sender,
        [FromServices] HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        if (httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) is not string id)
        {
            return Results.BadRequest("Subject not found on JWT.");
        }
        var userId = new UserId(id);

        var response = await sender.Send(new BalanceQuery(userId), cancellationToken);
        var balances = await response
            .Balances.ToAsyncEnumerable()
            .SelectAwaitWithCancellation(
                async (b, ct) =>
                {
                    var from = await sender.Send(new UserQuery(b.From), ct);
                    var to = await sender.Send(new UserQuery(b.To), ct);
                    return new Balance(
                        User.FromDomain(from.User!),
                        User.FromDomain(to.User!),
                        Money.FromDomain(b.Amount)
                    );
                }
            )
            .ToArrayAsync(cancellationToken);
        return Results.Ok(balances);
    }
}

public record User(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("phoneNumber")] string PhoneNumber
)
{
    public static User FromDomain(UserAggregate user) => new(user.Id.Value, user.Name, user.PhoneNumber.Value);
}

public sealed record Balance(
    [property: JsonPropertyName("from")] User From,
    [property: JsonPropertyName("to")] User To,
    [property: JsonPropertyName("amount")] Money Amount
);

[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(Balance))]
public sealed partial class UserSerializerContext : JsonSerializerContext;
