using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Split.Domain.Primitives;
using Split.Domain.Transaction;
using Split.Domain.Transaction.Events;
using Split.Domain.User.Events;

namespace Split.Application.Api;

public static class Transactions
{
    public static RouteGroupBuilder MapTransactionsEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        var group = routeBuilder.MapGroup("/transactions");

        group
            .MapGet("/", GetTransactions)
            .Produces<IEnumerable<Transaction>>()
            .WithDescription("Gets transactions involving the requesting user.");

        group
            .MapPost("/", CreateTransaction)
            .Produces<Transaction>()
            .WithDescription("Creates a transaction from the requesting user.");

        group
            .MapDelete("/{transactionId}", DeleteTransaction)
            .Produces<Transaction>()
            .WithDescription("Deletes a transaction.");

        return group;
    }

    private static IResult GetTransactions(
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
        var stream = sender.CreateStream(new UserTransactionsQuery(userId), cancellationToken);
        var users = stream.Select(r => Transaction.FromDomain(r.Transaction));
        return Results.Ok(users);
    }

    private static async Task<IResult> CreateTransaction(
        [FromServices] ISender sender,
        [FromServices] HttpContext httpContext,
        [FromBody] CreateTransactionRequest request,
        CancellationToken cancellationToken
    )
    {
        if (httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) is not string id)
        {
            return Results.BadRequest("Subject not found on JWT.");
        }
        var userId = new UserId(id);

        var amount = request.Amount.TryToDomain();
        if (amount is null)
        {
            return Results.BadRequest("Amount is malformed.");
        }

        if (request.Recipients is [])
        {
            return Results.BadRequest("RecipientIds is empty.");
        }

        if (
            !request.Recipients.All(r =>
                r is RecipientWithId
                || (r is RecipientWithPhoneNumber { PhoneNumber: var phoneNumber } && PhoneNumber.IsValid(phoneNumber))
            )
        )
        {
            return Results.BadRequest(
                "Not all recipients are well-formatted, each requires an ID or a well-formatted phone number."
            );
        }

        var recipientIds = await request
            .Recipients.ToAsyncEnumerable()
            .SelectAwaitWithCancellation(
                async (r, ct) =>
                    r switch
                    {
                        RecipientWithId { Id: var id } => new UserId(id),
                        RecipientWithPhoneNumber { PhoneNumber: var phoneNumber } => (
                            await sender.Send(new PhoneNumberQuery(new(phoneNumber)), ct)
                        )
                            .User
                            ?.Id,
                        _ => null,
                    }
            )
            .ToArrayAsync(cancellationToken);

        if (recipientIds.Any(r => r is null))
        {
            return Results.BadRequest("Not all recipients could be found.");
        }

        var response = await sender.Send(
            new Domain.Transaction.Events.CreateTransactionRequest(amount, request.Description, userId, recipientIds!),
            cancellationToken
        );

        return Results.Ok(Transaction.FromDomain(response.Transaction));
    }

    private static async Task<IResult> DeleteTransaction(
        [FromServices] ISender sender,
        [FromRoute] Guid transactionId,
        CancellationToken cancellationToken
    )
    {
        _ = await sender.Send(new RemoveTransactionRequest(new TransactionId(transactionId)), cancellationToken);

        return Results.NoContent();
    }
}

public record Transaction(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("amount")] Money Amount,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("senderId")] string SenderId,
    [property: JsonPropertyName("recipientIds")] string[] RecipientIds,
    [property: JsonPropertyName("createdAt")] DateTime CreatedAt
)
{
    public static Transaction FromDomain(TransactionAggregate transaction) =>
        new(
            transaction.Id.Value,
            Money.FromDomain(transaction.Amount),
            transaction.Description,
            transaction.SenderId.Value,
            [.. transaction.RecipientIds.Select(id => id.Value)],
            transaction.CreatedAt.UtcDateTime
        );
}

public record Money(
    [property:
        JsonPropertyName("amount"),
        JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)
    ]
        decimal Amount,
    [property: JsonPropertyName("currency")] string Currency
)
{
    public static Money FromDomain(Domain.Primitives.Money money) => new(money.Amount, money.Currency.Value);

    public Domain.Primitives.Money? TryToDomain() =>
        !Domain.Primitives.Currency.IsValid(Currency) || Amount < 0 ? null : new(Amount, new(Currency));
}

public record CreateTransactionRequest(
    [property: JsonPropertyName("amount")] Money Amount,
    [property: JsonPropertyName("description"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        string? Description,
    [property: JsonPropertyName("recipients")] Recipient[] Recipients
);

[JsonConverter(typeof(RecipientConverter))]
[JsonDerivedType(typeof(RecipientWithId), typeDiscriminator: null!)]
[JsonDerivedType(typeof(RecipientWithPhoneNumber), typeDiscriminator: null!)]
public abstract record Recipient();

public record RecipientWithId([property: JsonPropertyName("id")] string Id) : Recipient();

public record RecipientWithPhoneNumber([property: JsonPropertyName("phoneNumber")] string PhoneNumber) : Recipient();

public sealed class RecipientConverter : JsonConverter<Recipient>
{
    public override Recipient? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var document = JsonDocument.ParseValue(ref reader).RootElement;
        if (document.TryGetProperty("id", out var id))
        {
            return new RecipientWithId(id.GetString() ?? throw new JsonException(""));
        }
        if (document.TryGetProperty("phoneNumber", out var phoneNumber))
        {
            return new RecipientWithPhoneNumber(phoneNumber.GetString() ?? throw new JsonException(""));
        }
        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, Recipient value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        if (value is RecipientWithId { Id: var id })
        {
            writer.WritePropertyName("id");
            writer.WriteStringValue(id);
        }
        else if (value is RecipientWithPhoneNumber { PhoneNumber: var phoneNumber })
        {
            writer.WritePropertyName("phoneNumber");
            writer.WriteStringValue(phoneNumber);
        }
        writer.WriteEndObject();
    }
}

[JsonSerializable(typeof(Transaction))]
[JsonSerializable(typeof(CreateTransactionRequest))]
public sealed partial class TransactionSerializerContext : JsonSerializerContext;
