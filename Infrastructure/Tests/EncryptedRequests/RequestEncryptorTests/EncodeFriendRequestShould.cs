using System;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Split.Domain.Primitives;
using Split.Infrastructure.EncryptedRequests;

namespace Split.Infrastructure.Tests.EncryptedRequests.RequestEncryptorTests;

[TestClass]
public class EncodeFriendRequestShould
{
    [TestMethod]
    public void SuccessfullyEncodeAFriendRequest()
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
        var userId = new UserId("user-id");

        // Act
        var encodedRequest = encryptor.EncodeFriendRequest(userId);

        // Assert
        Assert.IsNotNull(encodedRequest);
        Assert.IsFalse(string.IsNullOrWhiteSpace(encodedRequest));
        Assert.IsFalse(encodedRequest.Contains(userId.Value));
    }

    [TestMethod]
    public void ProduceResultsThatAreDecodable()
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
        var userId = new UserId("user-id");

        // Act
        var encodedRequest = encryptor.EncodeFriendRequest(userId);
        var decodedRequest = encryptor.DecodeFriendRequest(encodedRequest);

        // Assert
        Assert.IsNotNull(decodedRequest);
        Assert.AreEqual(userId, decodedRequest);
    }
}
