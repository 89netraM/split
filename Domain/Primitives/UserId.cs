using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Split.Domain.Primitives;

[DebuggerDisplay("{Value}")]
public class UserId : IEquatable<UserId>
{
    public string Value { get; }

    public UserId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("UserId cannot be empty", nameof(value));
        }

        Value = value;
    }

    [ExcludeFromCodeCoverage]
    public override string ToString() => Value;

    public bool Equals(UserId? other) => other is not null && Value.Equals(other.Value);

    [ExcludeFromCodeCoverage]
    public override bool Equals(object? obj) => Equals(obj as UserId);

    [ExcludeFromCodeCoverage]
    public override int GetHashCode() => Value.GetHashCode();

    [ExcludeFromCodeCoverage]
    public static bool operator ==(UserId? left, UserId? right) => left is null ? right is null : left.Equals(right);

    [ExcludeFromCodeCoverage]
    public static bool operator !=(UserId? left, UserId? right) => !(left == right);
}
