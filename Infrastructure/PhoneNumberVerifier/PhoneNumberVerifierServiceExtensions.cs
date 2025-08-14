using HuaweiWifiSms.Grpc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Split.Infrastructure.PhoneNumberVerifier.Services;

namespace Split.Infrastructure.PhoneNumberVerifier;

public static class PhoneNumberVerifierServiceExtensions
{
    public static IServiceCollection AddPhoneNumberVerifierServices(this IServiceCollection services)
    {
        services.AddSmsSenderClient();

        services.AddOptions<CodeSenderOptions>().BindConfiguration("CodeSender").ValidateOnStart();
        services.AddTransient<IValidateOptions<CodeSenderOptions>, CodeSenderOptionsValidator>();
        services.AddTransient<CodeSenderService>();

        return services;
    }

    private static IServiceCollection AddSmsSenderClient(this IServiceCollection services)
    {
        services.AddOptions<SmsSenderOptions>().BindConfiguration("Grpc:SmsSender").ValidateOnStart();
        services.AddTransient<IValidateOptions<SmsSenderOptions>, SmsSenderOptionsValidator>();

        services
            .AddGrpcClient<SmsSender.SmsSenderClient>()
            .ConfigureHttpClient(
                (sp, client) =>
                    client.BaseAddress = sp.GetRequiredService<IOptions<SmsSenderOptions>>().Value.BaseAddress
            );

        return services;
    }
}
