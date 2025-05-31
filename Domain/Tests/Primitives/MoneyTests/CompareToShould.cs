using Microsoft.VisualStudio.TestTools.UnitTesting;
using Split.Domain.Primitives;

namespace Split.Domain.Tests.Primitives.MoneyTests;

[TestClass]
public class CompareToShould
{
    [TestMethod]
    public void ReturnZeroForTheSameObject()
    {
        // Arrange
        var money = new Money(100, new Currency("SEK"));

        // Act
        var result = money.CompareTo(money);

        // Assert
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void ReturnZeroForEqualAmountsInSameCurrency()
    {
        // Arrange
        var money1 = new Money(100, new Currency("SEK"));
        var money2 = new Money(100, new Currency("SEK"));

        // Act
        var result = money1.CompareTo(money2);

        // Assert
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void ReturnLessThanForSmallerAmount()
    {
        // Arrange
        var money1 = new Money(50, new Currency("SEK"));
        var money2 = new Money(100, new Currency("SEK"));

        // Act
        var result = money1.CompareTo(money2);

        // Assert
        Assert.IsTrue(result < 0);
    }

    [TestMethod]
    public void ReturnGreaterThanForLargerAmount()
    {
        // Arrange
        var money1 = new Money(150, new Currency("SEK"));
        var money2 = new Money(100, new Currency("SEK"));

        // Act
        var result = money1.CompareTo(money2);

        // Assert
        Assert.IsTrue(result > 0);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(100)]
    public void ReturnGreaterThanWhenComparingToNull(double amount)
    {
        // Arrange
        var money1 = new Money((decimal)amount, new Currency("SEK"));
        Money? money2 = null;

        // Act
        var result = money1.CompareTo(money2);

        // Assert
        Assert.IsTrue(result > 0);
    }

    [TestMethod]
    public void ThrowExceptionWhenComparingDifferentCurrencies()
    {
        // Arrange
        var money1 = new Money(100, new Currency("SEK"));
        var money2 = new Money(100, new Currency("USD"));

        // Act & Assert
        Assert.ThrowsException<CurrencyMismatchException>(() => money1.CompareTo(money2));
    }
}
