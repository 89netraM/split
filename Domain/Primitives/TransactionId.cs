using System;
using System.Diagnostics;

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

        Value = value;
    }

    public override string ToString() => Value.ToString();

    public bool Equals(TransactionId? other) => other is not null && Value.Equals(other.Value);

    public override bool Equals(object? obj) => Equals(obj as TransactionId);

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(TransactionId? left, TransactionId? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(TransactionId? left, TransactionId? right) => !(left == right);
}
