using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Split.Domain.Primitives;

[DebuggerDisplay("{Value}")]
public partial class TransactionId : IEquatable<TransactionId>
{
    public Guid Value { get; }

    public TransactionId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Transaction ID cannot be empty", nameof(value));
        }
        if (value.Version is not 7)
        {
            throw new ArgumentException("Transaction ID must be a version 7 GUID", nameof(value));
        }

        Value = value;
    }

    [ExcludeFromCodeCoverage]
    public override string ToString() => Value.ToString();

    public bool Equals(TransactionId? other) => other is not null && Value.Equals(other.Value);

    [ExcludeFromCodeCoverage]
    public override bool Equals(object? obj) => Equals(obj as TransactionId);

    [ExcludeFromCodeCoverage]
    public override int GetHashCode() => Value.GetHashCode();

    [ExcludeFromCodeCoverage]
    public static bool operator ==(TransactionId? left, TransactionId? right) =>
        left is null ? right is null : left.Equals(right);

    [ExcludeFromCodeCoverage]
    public static bool operator !=(TransactionId? left, TransactionId? right) => !(left == right);
}
