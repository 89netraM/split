using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Mediator;
using Microsoft.Extensions.Logging;
using Split.Domain.Primitives;
using Split.Domain.User;
using Split.Domain.User.Events;

namespace Split.Application.Api.Discord;

public sealed class CreateTransactionCommand(ILogger<CreateTransactionCommand> logger, ISender sender)
    : InteractionModuleBase
{
    [SlashCommand("create-transaction", "Create a transaction from you to one or more recipients")]
    public async Task SendSplit(
        [Summary("amount", "The amount of money sent"), MinValue(0.0d)] double amountNumber,
        [Summary("recipient", "The recipient (user or role) who split the money")]
            IMentionable receivingDiscordUserOrRole,
        [Summary("recipient-2", "A second recipient who split the money")] IUser? receivingDiscordUser2 = null,
        [Summary("recipient-3", "A third recipient who split the money")] IUser? receivingDiscordUser3 = null,
        [Summary("description", "A description of the transaction")] string description = ""
    )
    {
        await Context.Interaction.DeferAsync();

        List<IUser> receivingDiscordUsers;
        if (receivingDiscordUserOrRole is IUser user)
        {
            receivingDiscordUsers = [user];
        }
        else if (receivingDiscordUserOrRole is SocketRole role)
        {
            receivingDiscordUsers = [.. role.Members.Cast<IUser>()];
        }
        else
        {
            logger.LogError(
                "Received recipient of unknown type {RecipientType}.",
                receivingDiscordUserOrRole.GetType().FullName
            );
            await Context.Interaction.ModifyOriginalResponseAsync(message =>
            {
                message.Content = "Could not interpret recipient";
            });
            return;
        }
        if (receivingDiscordUser2 is not null)
        {
            receivingDiscordUsers.Add(receivingDiscordUser2);
        }
        if (receivingDiscordUser3 is not null)
        {
            receivingDiscordUsers.Add(receivingDiscordUser3);
        }

        var status = await CreateTransaction(Context.User, receivingDiscordUsers, amountNumber, description);
        await Context.Interaction.ModifyOriginalResponseAsync(message =>
        {
            message.Content = status;
        });
    }

    private async Task<string> CreateTransaction(
        IUser discordUser,
        List<IUser> receivingDiscordUsers,
        double amountNumber,
        string description
    )
    {
        var defaultCurrency = new Currency("SEK");
        if (amountNumber < 0.0d)
        {
            return $"Amount cannot be less than 0, you tried to send {amountNumber:0.00}";
        }
        var amount = new Domain.Primitives.Money((decimal)amountNumber, defaultCurrency);

        var userResult = await sender.Send(new AlternateUserQuery(discordUser.AlternateUserId()));
        if (userResult.User is not { } user)
        {
            return $"Cannot find Discord user {discordUser.PrettyName()} in the Split app";
        }
        if (receivingDiscordUsers is [])
        {
            return "Cannot create a transaction with zero recipients";
        }
        var recipientsResult = await receivingDiscordUsers
            .ToAsyncEnumerable()
            .SelectAwait<IUser, (UserAggregate? user, string? missingName)>(async discordUser =>
            {
                var recipientResult = await sender.Send(new AlternateUserQuery(discordUser.AlternateUserId()));
                return recipientResult.User is { } recipient ? (recipient, null) : (null, discordUser.PrettyName());
            })
            .ToArrayAsync();
        if (recipientsResult.Any(r => r.missingName is not null))
        {
            var missingNames = recipientsResult
                .Where(r => r.missingName is not null)
                .Select(r => r.missingName)
                .ToArray() switch
            {
                [] => throw new ZeroUsersException(),
                [var name] => $" {name}",
                [var first, var last] => $"s {first} and {last}",
                [.. var rest, var last] => $"s {string.Join(", ", rest)}, and {last}",
            };
            return $"Cannot find Discord user{missingNames} in the Split app";
        }
        var recipients = recipientsResult.Select(r => r.user!).ToArray();

        await sender.Send(
            new Domain.Transaction.Events.CreateTransactionRequest(
                amount,
                description,
                user.Id,
                [.. recipients.Select(u => u.Id)]
            )
        );

        var recipientNames = receivingDiscordUsers switch
        {
            [] => throw new ZeroUsersException(),
            [var u] => u.PrettyName(),
            [var first, var last] => $"{first.PrettyName()} and {last.PrettyName()}",
            [.. var rest, var last] =>
                $"{string.Join(", ", rest.Select(u => u.PrettyName()))}, and {last.PrettyName()}",
        };
        var message = description is "" ? "" : $" with the message {description}";
        return $"Transaction created, {discordUser.PrettyName()} have sent {amountNumber:0.00} SEK to {recipientNames}{message}";
    }
}

file sealed class ZeroUsersException()
    : Exception("The list contained zero users, even though that had been checked for before.");
