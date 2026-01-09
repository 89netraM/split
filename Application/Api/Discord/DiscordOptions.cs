using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Split.Application.Api.Discord;

public sealed class DiscordOptions
{
    [Required]
    public required string Token { get; set; }
}

[OptionsValidator]
public sealed partial class DiscordOptionsValidator : IValidateOptions<DiscordOptions>;

public static class DiscordOptionsServiceCollectionExtensions
{
    public static IServiceCollection AddDiscordOptions(this IServiceCollection services)
    {
        services.AddOptions<DiscordOptions>().BindConfiguration("Discord").ValidateOnStart();
        services.AddTransient<IValidateOptions<DiscordOptions>, DiscordOptionsValidator>();

        return services;
    }
}
