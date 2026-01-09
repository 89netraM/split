using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Mediator;
using Split.Domain.Primitives;
using Split.Domain.User.Events;

namespace Split.Application.Api.Discord;

public sealed class ViewBalancesCommand(ISender sender) : InteractionModuleBase
{
    [SlashCommand("view-balances", "Returns a list of your balances towards other users")]
    public async Task ViewBalances()
    {
        await Context.Interaction.DeferAsync(ephemeral: true);

        var status = await GetBalances(Context.User);
        await Context.Interaction.ModifyOriginalResponseAsync(message =>
        {
            message.Content = status;
        });
    }

    private async Task<string> GetBalances(IUser discordUser)
    {
        var userResult = await sender.Send(new AlternateUserQuery(discordUser.AlternateUserId()));
        if (userResult.User is not { } user)
        {
            return $"Cannot find Discord user {discordUser.PrettyName()} in the Split app";
        }

        var balanceResult = await sender.Send(new Domain.Transaction.Events.BalanceQuery(user.Id));

        var balanceLines = await balanceResult
            .Balances.Where(b => b.Amount.Amount > 0.0m)
            .ToAsyncEnumerable()
            .SelectAwait(async b =>
                b.From == user.Id
                    ? $":green_square: {await GetNameOfUser(b.To)} owes you {b.Amount.Amount:0.00} {b.Amount.Currency}"
                    : $":red_square: You owe {await GetNameOfUser(b.From)} {b.Amount.Amount:0.00} {b.Amount.Currency}"
            )
            .ToListAsync();

        return string.Join("\n", balanceLines);
    }

    private async Task<string?> GetNameOfUser(UserId userId)
    {
        if (await sender.Send(new UserQuery(userId)) is not { User: { } user })
        {
            return "<unknown user>";
        }

        if (user.DiscordId() is not ulong discordId)
        {
            return user.Name;
        }

        IUser? discordUser =
            Context.Guild is { } guild && await guild.GetUserAsync(discordId) is { } guildUser
                ? guildUser
                : await Context.Client.GetUserAsync(discordId);
        return discordUser?.PrettyName() ?? user.Name;
    }
}
