using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Split.Domain.Primitives;

[DebuggerDisplay("{Value}")]
public partial class Currency : IEquatable<Currency>
{
    [GeneratedRegex(@"^[A-Z]{3}$")]
    private static partial Regex CurrencyValidator { get; }

    public string Value { get; }

    public Currency(string value)
    {
        if (!CurrencyValidator.IsMatch(value))
        {
            throw new ArgumentException("Invalid currency format", nameof(value));
        }

        Value = value;
    }

    [ExcludeFromCodeCoverage]
    public override string ToString() => Value;

    public bool Equals(Currency? other) => other is not null && Value.Equals(other.Value);

    [ExcludeFromCodeCoverage]
    public override bool Equals(object? obj) => Equals(obj as Currency);

    [ExcludeFromCodeCoverage]
    public override int GetHashCode() => Value.GetHashCode();

    [ExcludeFromCodeCoverage]
    public static bool operator ==(Currency? left, Currency? right) =>
        left is null ? right is null : left.Equals(right);

    [ExcludeFromCodeCoverage]
    public static bool operator !=(Currency? left, Currency? right) => !(left == right);
}

public class CurrencyMismatchException(string message) : Exception(message);
