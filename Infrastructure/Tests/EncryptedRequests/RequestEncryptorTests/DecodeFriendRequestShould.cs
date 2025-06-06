using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Split.Domain.Primitives;
using Split.Infrastructure.EncryptedRequests;

namespace Split.Infrastructure.Tests.EncryptedRequests.RequestEncryptorTests;

[TestClass]
public class DecodeFriendRequestShould
{
    [TestMethod]
    public void ReturnNull_WhenReceivingInvalidBase64String()
    {
        // Arrange
        var options = new OptionsWrapper<EncryptorOptions>(
            new EncryptorOptions { Key = TestKeys.Key, Iv = TestKeys.Iv }
        );
        var timeProvider = new FakeTimeProvider(new(2025, 06, 06, 16, 10, 00, new(02, 00, 00)))
        {
            AutoAdvanceAmount = TimeSpan.FromMinutes(1),
        };
        var encryptor = new RequestEncryptor(options, timeProvider);

        // Act
        var encodedRequest = encryptor.DecodeFriendRequest("this-is-not-a-valid-base64");

        // Assert
        Assert.IsNull(encodedRequest);
    }

    [TestMethod]
    public void ReturnNull_WhenReceivingInvalidEncryptedString()
    {
        // Arrange
        var options = new OptionsWrapper<EncryptorOptions>(
            new EncryptorOptions { Key = TestKeys.Key, Iv = TestKeys.Iv }
        );
        var timeProvider = new FakeTimeProvider(new(2025, 06, 06, 16, 10, 00, new(02, 00, 00)))
        {
            AutoAdvanceAmount = TimeSpan.FromMinutes(1),
        };
        var encryptor = new RequestEncryptor(options, timeProvider);

        // Act
        var encodedRequest = encryptor.DecodeFriendRequest("aGVsbG8gZnJpZW5k");

        // Assert
        Assert.IsNull(encodedRequest);
    }

    [TestMethod]
    [DataRow("not json at all")]
    [DataRow("{\"UserId\":\"user-id\"}")]
    [DataRow("{\"UserId\":\"user-id\",\"IssuedAt\":\"2025-06-06T16:10:00Z\"")]
    public void ReturnNull_WhenReceivingAnEncryptedStringWithMalformedJson(string jsonPayload)
    {
        // Arrange
        var options = new OptionsWrapper<EncryptorOptions>(
            new EncryptorOptions { Key = TestKeys.Key, Iv = TestKeys.Iv }
        );
        var timeProvider = new FakeTimeProvider(new(2025, 06, 06, 16, 10, 00, new(02, 00, 00)))
        {
            AutoAdvanceAmount = TimeSpan.FromMinutes(1),
        };
        var encryptor = new RequestEncryptor(options, timeProvider);

        var aes = Aes.Create();
        aes.Key = Convert.FromBase64String(TestKeys.Key);
        aes.IV = Convert.FromBase64String(TestKeys.Iv);
        using var cryptographEncryptor = aes.CreateEncryptor();
        using var memoryStream = new MemoryStream();
        using (var cryptoStream = new CryptoStream(memoryStream, cryptographEncryptor, CryptoStreamMode.Write))
        using (var writer = new StreamWriter(cryptoStream))
        {
            writer.Write(jsonPayload);
        }
        var encryptedPayload = Convert.ToBase64String(memoryStream.ToArray());

        // Act
        var encodedRequest = encryptor.DecodeFriendRequest(encryptedPayload);

        // Assert
        Assert.IsNull(encodedRequest);
    }

    [TestMethod]
    public void ReturnNull_WhenDecodingAnExpiredRequest()
    {
        // Arrange
        var options = new OptionsWrapper<EncryptorOptions>(
            new EncryptorOptions
            {
                Key = TestKeys.Key,
                Iv = TestKeys.Iv,
                FriendRequestTimeout = TimeSpan.FromMinutes(5),
            }
        );
        var timeProvider = new FakeTimeProvider(new(2025, 06, 06, 16, 10, 00, new(02, 00, 00)))
        {
            AutoAdvanceAmount = TimeSpan.FromMinutes(1),
        };
        var encryptor = new RequestEncryptor(options, timeProvider);

        // Act
        var encodedRequest = encryptor.EncodeFriendRequest(new("user-id"));
        timeProvider.Advance(TimeSpan.FromMinutes(6));
        var decodedRequest = encryptor.DecodeFriendRequest(encodedRequest);

        // Assert
        Assert.IsNull(decodedRequest);
    }
}
