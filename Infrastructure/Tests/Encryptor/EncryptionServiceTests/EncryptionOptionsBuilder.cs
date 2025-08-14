using Microsoft.Extensions.Options;
using Split.Infrastructure.Encryptor;

namespace Split.Infrastructure.Tests.Encryptor.EncryptionServiceTests;

public static class EncryptionOptionsBuilder
{
    public static IOptions<EncryptionOptions> Build() =>
        Build(iv: "1KZ2chpLyTXopVvRG2NZag==", key: "r208PSSceg4vk7DpvC0XoQBmxkC7hB6fSbdwYx0Sbjs=");

    public static IOptions<EncryptionOptions> Build(string iv, string key) =>
        Options.Create(new EncryptionOptions() { IV = iv, Key = key });
}
