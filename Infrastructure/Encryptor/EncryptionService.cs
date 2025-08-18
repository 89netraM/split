using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Split.Domain.User;

namespace Split.Infrastructure.Encryptor;

public sealed class EncryptionService : IEncryptionService
{
    private readonly Aes aes;

    public EncryptionService(IOptions<EncryptionOptions> options)
    {
        aes = Aes.Create();
        aes.IV = Convert.FromBase64String(options.Value.IV);
        aes.Key = Convert.FromBase64String(options.Value.Key);
    }

    public string Encrypt(string value) =>
        Convert.ToBase64String(aes.EncryptEcb(Encoding.UTF8.GetBytes(value), PaddingMode.ISO10126));

    public string Decrypt(string token) =>
        Encoding.UTF8.GetString(aes.DecryptEcb(Convert.FromBase64String(token), PaddingMode.ISO10126));
}

public sealed class EncryptionOptions
{
    [Required]
    public required string IV { get; set; }

    [Required]
    public required string Key { get; set; }
}

[OptionsValidator]
public sealed partial class EncryptionOptionsValidator : IValidateOptions<EncryptionOptions>;

public static class EncryptionServiceExtensions
{
    public static IServiceCollection AddEncryptionService(this IServiceCollection services)
    {
        services.AddOptions<EncryptionOptions>().BindConfiguration("Encryption").ValidateOnStart();
        services.AddTransient<IValidateOptions<EncryptionOptions>, EncryptionOptionsValidator>();

        services.AddSingleton<IEncryptionService, EncryptionService>();

        return services;
    }
}
