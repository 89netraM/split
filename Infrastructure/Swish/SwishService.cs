using System;
using System.Text.Json;
using Split.Domain.Primitives;

namespace Split.Infrastructure.Swish;

public class SwishService
{
    /// <summary>
    /// Builds a URL that opens the Swish app to the "Swisha" screen with recipient and amount pre-filled.
    /// </summary>
    /// <param name="phoneNumber">The recipients phone number.</param>
    /// <param name="amount">The amount to Swish.</param>
    /// <exception cref="UnswishableCurrencyException" />
    public Uri BuildRequestUrl(PhoneNumber phoneNumber, Money amount)
    {
        if (amount.Currency.Value is not "SEK")
        {
            throw new UnswishableCurrencyException(amount.Currency, nameof(amount));
        }

        var data = new PaymentData(new(phoneNumber.Value), new((double)amount.Amount));
        var builder = new UriBuilder("swish", "payment")
        {
            Query = $"data={JsonSerializer.Serialize(data, PaymentDataSerializerContext.Default.PaymentData)}",
        };
        return builder.Uri;
    }
}

public class UnswishableCurrencyException(Currency currency, string paramName)
    : ArgumentException(message: $"Currency {currency} cannot be swished.", paramName);
