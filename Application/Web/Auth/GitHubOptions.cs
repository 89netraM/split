using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Split.Application.Web.Auth;

public class GitHubOptions
{
    [Required]
    public required string ClientId { get; set; }

    [Required]
    public required string ClientSecret { get; set; }
}

[OptionsValidator]
public partial class GitHubOptionsValidator : IValidateOptions<GitHubOptions>;

public class GitHubOAuthConfigureOptions(IOptions<GitHubOptions> githubOptions) : IConfigureNamedOptions<OAuthOptions>
{
    public void Configure(OAuthOptions options) => Configure(Options.DefaultName, options);

    public void Configure(string? name, OAuthOptions options)
    {
        if (name is not "GitHub")
        {
            return;
        }

        options.ClientId = githubOptions.Value.ClientId;
        options.ClientSecret = githubOptions.Value.ClientSecret;
    }
}

public static class GitHubOptionsServiceCollectionExtensions
{
    public static IServiceCollection AddGitHubOptions(this IServiceCollection services)
    {
        services.AddOptions<GitHubOptions>().BindConfiguration("Auth:GitHub").ValidateOnStart();
        services.AddSingleton<IValidateOptions<GitHubOptions>, GitHubOptionsValidator>();
        services.AddSingleton<IConfigureOptions<OAuthOptions>, GitHubOAuthConfigureOptions>();
        return services;
    }
}
