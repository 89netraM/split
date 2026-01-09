using System.Linq;
using Discord;
using Split.Domain.Primitives;
using Split.Domain.User;

namespace Split.Application.Api.Discord;

public static class DiscordUserExtensions
{
    private const string DiscordIdType = "discord";

    public static AlternateUserId AlternateUserId(this IUser user) => new(DiscordIdType, user.Id.ToString());

    public static string PrettyName(this IUser user) =>
        user is IGuildUser { DisplayName: var displayName } ? displayName : user.Username;

    public static ulong? DiscordId(this UserAggregate user) =>
        user.AlternateIds.FirstOrDefault(ai => ai.Type is DiscordIdType) is { Id: var discordIdString }
        && ulong.TryParse(discordIdString, out var discordId)
            ? discordId
            : null;
}
