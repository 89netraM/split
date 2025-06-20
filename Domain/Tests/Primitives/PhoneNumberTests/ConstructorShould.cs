using Microsoft.VisualStudio.TestTools.UnitTesting;
using Split.Domain.Primitives;

namespace Split.Domain.Tests.Primitives.PhoneNumberTests;

[TestClass]
public class ConstructorShould
{
    [TestMethod]
    [DataRow("+46234567890")]
    [DataRow("+47987654321")]
    public void ConstructValidPhoneNumbers(string phoneNumber)
    {
        // Should not throw an exception for valid phone numbers
        _ = new PhoneNumber(phoneNumber);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("1234567890")]
    [DataRow("123-456 78 90")]
    [DataRow("not-a-phone-number")]
    public void ThrowExceptionForInvalidPhoneNumbers(string phoneNumber)
    {
        Assert.ThrowsExactly<PhoneNumberFormatException>(() => new PhoneNumber(phoneNumber));
    }
}
