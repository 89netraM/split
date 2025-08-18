using Microsoft.VisualStudio.TestTools.UnitTesting;
using Split.Infrastructure.Encryptor;

namespace Split.Infrastructure.Tests.Encryptor.EncryptionServiceTests;

[TestClass]
public sealed class EncryptShould
{
    [TestMethod]
    public void NotRevealInput()
    {
        // Arrange
        var options = EncryptionOptionsBuilder.Build();
        var input = "Hej";
        var encryptionService = new EncryptionService(options);

        // Act
        var output = encryptionService.Encrypt(input);

        // Assert
        Assert.DoesNotContain(input, output);
    }
}
