using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Split.Domain.Primitives;

namespace Split.Domain.Tests.Primitives.MoneyTests;

[TestClass]
public class ConstructorShould
{
    [TestMethod]
    [DataRow(100, "SEK")]
    [DataRow(0.25, "USD")]
    [DataRow(0, "EUR")]
    public void ConstructValidMoney(double amount, string currencyCode)
    {
        // Should not throw an exception for valid money amounts and currency codes
        _ = new Money((decimal)amount, new Currency(currencyCode));
    }

    [TestMethod]
    [DataRow(-100)]
    [DataRow(-0.25)]
    [DataRow(-1)]
    public void ThrowExceptionForNegativeAmount(double amount)
    {
        // Should throw an exception for negative amounts
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new Money((decimal)amount, new Currency("SEK")));
    }
}
