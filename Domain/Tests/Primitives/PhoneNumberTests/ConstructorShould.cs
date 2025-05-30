using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Split.Domain.Primitives;

namespace Split.Domain.Tests.Primitives.PhoneNumberTests;

[TestClass]
public class ConstructorShould
{
    [TestMethod]
    [DataRow("1234567890")]
    [DataRow("+46234567890")]
    public void ConstructValidPhoneNumbers(string phoneNumber)
    {
        // Should not throw an exception for valid phone numbers
        _ = new PhoneNumber(phoneNumber);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("123-456 78 90")]
    [DataRow("not-a-phone-number")]
    public void ThrowExceptionForInvalidPhoneNumbers(string phoneNumber)
    {
        Assert.ThrowsException<ArgumentException>(() => new PhoneNumber(phoneNumber));
    }
}
