using System.Text.Json.Serialization;

namespace Split.Infrastructure.Swish;

internal record PaymentData(Payee Payee, Amount Amount, int Version = 1);

internal record Payee(string Value);

internal record Amount(double Value);

[JsonSerializable(typeof(PaymentData))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class PaymentDataSerializerContext : JsonSerializerContext;
