using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Split.Domain.Primitives;

namespace Split.Domain.Tests.Primitives.CurrencyTests;

[TestClass]
public class ConstructorShould
{
    [TestMethod]
    [DataRow("SEK")]
    [DataRow("EUR")]
    [DataRow("USD")]
    public void ConstructValidCurrencies(string currencyCode)
    {
        // Should not throw an exception for valid currency codes
        _ = new Currency(currencyCode);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("kr")]
    [DataRow("sek")]
    public void ThrowExceptionForInvalidCurrencyCodes(string currencyCode)
    {
        Assert.ThrowsException<ArgumentException>(() => new Currency(currencyCode));
    }
}
