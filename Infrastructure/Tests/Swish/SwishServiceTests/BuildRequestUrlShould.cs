using System;
using System.Linq;
using System.Text.Json;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Split.Domain.Primitives;
using Split.Infrastructure.Swish;

namespace Split.Infrastructure.Tests.Swish.SwishServiceTests;

[TestClass]
public class BuildRequestUrlShould
{
    [TestMethod]
    public void ReturnASwishPaymentUrl()
    {
        // Arrange
        var phoneNumber = new PhoneNumber("+46987654321");
        var amount = new Money(100.0m, new("SEK"));
        var swishService = new SwishService();

        // Act
        var result = swishService.BuildRequestUrl(phoneNumber, amount);

        // Assert
        Assert.IsInstanceOfType<Uri>(result);
        Assert.AreEqual("swish", result.Scheme);
        Assert.AreEqual("payment", result.Host);
    }

    [TestMethod]
    public void ReturnAUrlWithDataQueryParameter()
    {
        // Arrange
        var phoneNumber = new PhoneNumber("+46987654321");
        var amount = new Money(100.0m, new("SEK"));
        var swishService = new SwishService();

        // Act
        var result = swishService.BuildRequestUrl(phoneNumber, amount);

        // Assert
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.Query));
        var query = HttpUtility.ParseQueryString(result.Query);
        Assert.IsNotNull(query["data"]);
    }

    [TestMethod]
    public void ReturnADataQueryParameter_WithValidJSON()
    {
        // Arrange
        var phoneNumber = new PhoneNumber("+46987654321");
        var amount = new Money(100.0m, new("SEK"));
        var swishService = new SwishService();

        // Act
        var result = swishService.BuildRequestUrl(phoneNumber, amount);

        // Assert
        var query = HttpUtility.ParseQueryString(result.Query);
        JsonSerializer.Deserialize<JsonDocument>(query["data"]!);
    }

    [TestMethod]
    public void ReturnADataQueryParameter_WithVersion1()
    {
        // Arrange
        var phoneNumber = new PhoneNumber("+46987654321");
        var amount = new Money(100.0m, new("SEK"));
        var swishService = new SwishService();

        // Act
        var result = swishService.BuildRequestUrl(phoneNumber, amount);

        // Assert
        var query = HttpUtility.ParseQueryString(result.Query);
        var json = JsonSerializer.Deserialize<JsonDocument>(query["data"]!)!;
        var version = json.RootElement.GetProperty("version").GetInt32();
        Assert.AreEqual(1, version);
    }

    [TestMethod]
    public void ReturnADataQueryParameter_WithThePayeeValueAsThePhoneNumber()
    {
        // Arrange
        var phoneNumber = new PhoneNumber("+46987654321");
        var amount = new Money(100.0m, new("SEK"));
        var swishService = new SwishService();

        // Act
        var result = swishService.BuildRequestUrl(phoneNumber, amount);

        // Assert
        var query = HttpUtility.ParseQueryString(result.Query);
        var json = JsonSerializer.Deserialize<JsonDocument>(query["data"]!)!;
        var payeeValue = json.RootElement.GetProperty("payee").GetProperty("value").GetString();
        Assert.AreEqual(phoneNumber.Value, payeeValue);
    }

    [TestMethod]
    public void ReturnADataQueryParameter_WithTheAmountValueAsTheAmount()
    {
        // Arrange
        var phoneNumber = new PhoneNumber("+46987654321");
        var amount = new Money(100.0m, new("SEK"));
        var swishService = new SwishService();

        // Act
        var result = swishService.BuildRequestUrl(phoneNumber, amount);

        // Assert
        var query = HttpUtility.ParseQueryString(result.Query);
        var json = JsonSerializer.Deserialize<JsonDocument>(query["data"]!)!;
        var payeeValue = json.RootElement.GetProperty("amount").GetProperty("value").GetDouble();
        Assert.IsTrue(decimal.Abs(amount.Amount - (decimal)payeeValue) < 0.001m);
    }

    [TestMethod]
    public void ThrowWhenCurrencyIsNotSEK()
    {
        // Arrange
        var phoneNumber = new PhoneNumber("+46987654321");
        var amount = new Money(100.0m, new("NOK"));
        var swishService = new SwishService();

        // Act
        var action = () => swishService.BuildRequestUrl(phoneNumber, amount);

        // Assert
        Assert.ThrowsExactly<UnswishableCurrencyException>(action);
    }
}
