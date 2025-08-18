using System;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Split.Domain.User;

namespace Split.Application.Api;

public sealed class JwtService
{
    private readonly SigningCredentials signingCredentials;
    private readonly string issuer;
    private readonly TimeSpan expiryTimeout;
    private readonly JwtSecurityTokenHandler tokenHandler = new();
    private readonly TimeProvider timeProvider;

    public JwtService(IOptions<JwtOptions> options, TimeProvider timeProvider)
    {
        var rsa = RSA.Create();
        rsa.ImportFromPem(options.Value.PrivateKey);
        signingCredentials = new(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256);

        issuer = options.Value.Issuer;
        expiryTimeout = options.Value.ExpiryTimeout;

        this.timeProvider = timeProvider;
    }

    public string GenerateToken(UserAggregate user)
    {
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: issuer,
            claims: [new(JwtRegisteredClaimNames.Sub, user.Id.Value)],
            expires: timeProvider.GetUtcNow().Add(expiryTimeout).UtcDateTime,
            signingCredentials: signingCredentials
        );
        return tokenHandler.WriteToken(token);
    }
}

public sealed class JwtOptions
{
    [Required]
    public required string Issuer { get; set; }

    [Required]
    public required string PrivateKey { get; set; }

    [Required]
    public required string PublicKey { get; set; }

    [Required]
    public required TimeSpan ExpiryTimeout { get; set; }
}

[OptionsValidator]
public sealed partial class JwtOptionsValidator : IValidateOptions<JwtOptions>;

public static class JwtServiceExtensions
{
    public static IServiceCollection AddJwtService(this IServiceCollection services)
    {
        services.AddOptions<JwtOptions>().BindConfiguration("Jwt").ValidateOnStart();
        services.AddTransient<IValidateOptions<JwtOptions>, JwtOptionsValidator>();

        services.AddSingleton<JwtService>();

        services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure(
                (JwtBearerOptions options, IOptions<JwtOptions> jwtOptions) =>
                {
                    var rsa = RSA.Create();
                    rsa.ImportFromPem(jwtOptions.Value.PublicKey);
                    options.TokenValidationParameters = new()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtOptions.Value.Issuer,
                        ValidAudience = jwtOptions.Value.Issuer,
                        IssuerSigningKey = new RsaSecurityKey(rsa),
                    };
                }
            );

        return services;
    }
}
