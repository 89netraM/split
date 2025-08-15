using Microsoft.Extensions.Options;
using Split.Infrastructure.Encryptor;

namespace Split.Infrastructure.Tests.PhoneNumberVerifier.Builders;

public static class EncryptionServiceBuilder
{
    public static EncryptionService Build(IOptions<EncryptionOptions>? options = null) =>
        new(
            options
                ?? Options.Create(
                    new EncryptionOptions()
                    {
                        IV = "1KZ2chpLyTXopVvRG2NZag==",
                        Key = "r208PSSceg4vk7DpvC0XoQBmxkC7hB6fSbdwYx0Sbjs=",
                    }
                )
        );
}
