using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Split.Domain.Primitives;

[DebuggerDisplay("{Value}")]
public partial class AuthKeyId : IEquatable<AuthKeyId>
{
    public string Value { get; }

    public AuthKeyId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Auth key ID cannot be null or empty", nameof(value));
        }

        Value = value;
    }

    [ExcludeFromCodeCoverage]
    public override string ToString() => Value.ToString();

    [ExcludeFromCodeCoverage]
    public bool Equals(AuthKeyId? other) => other is not null && Value == other.Value;

    [ExcludeFromCodeCoverage]
    public override bool Equals(object? obj) => Equals(obj as TransactionId);

    [ExcludeFromCodeCoverage]
    public override int GetHashCode() => Value.GetHashCode();

    [ExcludeFromCodeCoverage]
    public static bool operator ==(AuthKeyId a, AuthKeyId b) => a.Equals(b);

    [ExcludeFromCodeCoverage]
    public static bool operator !=(AuthKeyId a, AuthKeyId b) => !(a == b);
}
