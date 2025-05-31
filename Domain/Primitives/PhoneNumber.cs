using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Split.Domain.Primitives;

[DebuggerDisplay("{Value}")]
public partial class PhoneNumber : IEquatable<PhoneNumber>
{
    [GeneratedRegex(@"^\+?\d+$")]
    private static partial Regex PhoneNumberValidator { get; }

    public string Value { get; }

    public PhoneNumber(string value)
    {
        if (!PhoneNumberValidator.IsMatch(value))
        {
            throw new ArgumentException("Invalid phone number format", nameof(value));
        }

        Value = value;
    }

    [ExcludeFromCodeCoverage]
    public override string ToString() => Value;

    public bool Equals(PhoneNumber? other) => other is not null && Value.Equals(other.Value);

    [ExcludeFromCodeCoverage]
    public override bool Equals(object? obj) => Equals(obj as PhoneNumber);

    [ExcludeFromCodeCoverage]
    public override int GetHashCode() => Value.GetHashCode();

    [ExcludeFromCodeCoverage]
    public static bool operator ==(PhoneNumber? left, PhoneNumber? right) =>
        left is null ? right is null : left.Equals(right);

    [ExcludeFromCodeCoverage]
    public static bool operator !=(PhoneNumber? left, PhoneNumber? right) => !(left == right);
}
