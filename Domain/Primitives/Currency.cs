using System;
using System.Diagnostics;
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

    public override string ToString() => Value;

    public bool Equals(Currency? other) => other is not null && Value.Equals(other.Value);

    public override bool Equals(object? obj) => Equals(obj as Currency);

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(Currency? left, Currency? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(Currency? left, Currency? right) => !(left == right);
}

public class CurrencyMismatchException(string message) : Exception(message);
