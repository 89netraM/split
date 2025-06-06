using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Options;
using Split.Domain.Primitives;

namespace Split.Infrastructure.EncryptedRequests;

public class RequestEncryptor
{
    private readonly Aes aes;
    private readonly TimeProvider timeProvider;
    private readonly TimeSpan friendRequestTimeout;

    public RequestEncryptor(IOptions<EncryptorOptions> options, TimeProvider timeProvider)
    {
        aes = Aes.Create();
        aes.Key = Convert.FromBase64String(options.Value.Key);
        aes.IV = Convert.FromBase64String(options.Value.Iv);

        this.timeProvider = timeProvider;

        friendRequestTimeout = options.Value.FriendRequestTimeout;
    }

    public string EncodeFriendRequest(UserId userId) =>
        Encrypt(new(userId, timeProvider.GetUtcNow()), FriendRequestSerializer.Default.FriendRequest);

    private string Encrypt<T>(T payload, JsonTypeInfo<T> typeInfo)
    {
        using var encryptor = aes.CreateEncryptor();
        using var memoryStream = new MemoryStream();
        using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
        {
            JsonSerializer.Serialize(cryptoStream, payload, typeInfo);
        }
        return Convert.ToBase64String(memoryStream.ToArray());
    }

    public UserId? DecodeFriendRequest(string encodedRequest)
    {
        var payload = Decrypt(encodedRequest, FriendRequestSerializer.Default.FriendRequest);
        if (payload is null)
        {
            return null;
        }
        if (timeProvider.GetUtcNow() - payload.IssuedAt > friendRequestTimeout)
        {
            return null;
        }
        return payload.UserId;
    }

    private T? Decrypt<T>(string encodedPayload, JsonTypeInfo<T> typeInfo)
    {
        try
        {
            var payloadBytes = Convert.FromBase64String(encodedPayload);
            using var decryptor = aes.CreateDecryptor();
            using var memoryStream = new MemoryStream(payloadBytes);
            using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            return JsonSerializer.Deserialize(cryptoStream, typeInfo);
        }
        catch (FormatException)
        {
            return default;
        }
        catch (JsonException)
        {
            return default;
        }
        catch (CryptographicException)
        {
            return default;
        }
    }
}
