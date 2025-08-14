using Microsoft.VisualStudio.TestTools.UnitTesting;
using Split.Infrastructure.Encryptor;

namespace Split.Infrastructure.Tests.Encryptor.EncryptionServiceTests;

[TestClass]
public sealed class DecryptShould
{
    [TestMethod]
    public void ReturnOriginalInput()
    {
        // Arrange
        var options = EncryptionOptionsBuilder.Build();
        var input = "Hej";
        var encryptionService = new EncryptionService(options);
        var encrypted = encryptionService.Encrypt(input);

        // Act
        var decrypted = encryptionService.Decrypt(encrypted);

        // Assert
        Assert.AreEqual(input, decrypted);
    }
}
