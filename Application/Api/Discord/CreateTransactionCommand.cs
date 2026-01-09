using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Mediator;
using Split.Domain.Primitives;
using Split.Domain.User.Events;

namespace Split.Application.Api.Discord;

public sealed class CreateTransactionCommand(ISender sender) : InteractionModuleBase
{
    [SlashCommand("create-transaction", "Create a transaction from you to one recipient")]
    public async Task SendSplit(
        [Summary("amount", "The amount of money sent"), MinValue(0.0d)] double amountNumber,
        [Summary("recipient", "The recipient who split the money")] IUser receivingDiscordUser,
        [Summary("description", "An optional description of the transaction")] string description = ""
    )
    {
        await Context.Interaction.DeferAsync();

        var status = await CreateTransaction(Context.User, receivingDiscordUser, amountNumber, description);
        await Context.Interaction.ModifyOriginalResponseAsync(message =>
        {
            message.Content = status;
        });
    }

    private async Task<string> CreateTransaction(
        IUser discordUser,
        IUser receivingDiscordUser,
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
        var recipientResult = await sender.Send(new AlternateUserQuery(receivingDiscordUser.AlternateUserId()));
        if (recipientResult.User is not { } recipient)
        {
            return $"Cannot find Discord user {receivingDiscordUser.PrettyName()} in the Split app";
        }

        await sender.Send(
            new Domain.Transaction.Events.CreateTransactionRequest(amount, description, user.Id, [recipient.Id])
        );

        return $"Transaction created, you have sent {amountNumber:0.00} SEK to {receivingDiscordUser.PrettyName()}";
    }
}
