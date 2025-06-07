using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Split.Infrastructure.EncryptedRequests;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRequestEncryptor(this IServiceCollection services)
    {
        services.AddOptions<EncryptorOptions>().BindConfiguration("RequestEncryption").ValidateOnStart();
        services.AddTransient<IValidateOptions<EncryptorOptions>, EncryptorOptionsValidator>();
        services.AddSingleton<RequestEncryptor>();
        return services;
    }
}
