using NSubstitute;
using Split.Domain.User;

namespace Split.Infrastructure.Tests.PhoneNumberVerifier.Builders;

public static class EncryptionServiceBuilder
{
    public static IEncryptionService Build()
    {
        var substitute = Substitute.For<IEncryptionService>();
        var secret = "secret";
        substitute.Encrypt(Arg.Any<string>()).Returns("encrypted").AndDoes(call => secret = call.ArgAt<string>(0));
        substitute.Decrypt(Arg.Any<string>()).Returns(_ => secret);
        return substitute;
    }
}
