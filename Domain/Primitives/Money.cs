using System;
using System.Diagnostics;

namespace Split.Domain.Primitives;

[DebuggerDisplay("{ToString()}")]
public class Money : IComparable<Money>, IEquatable<Money>
{
    public decimal Amount { get; }
    public Currency Currency { get; }

    public Money(decimal amount, Currency currency)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative");
        }

        Amount = amount;
        Currency = currency;
    }

    public override string ToString() => $"{Amount} {Currency}";

    public bool Equals(Money? other) =>
        other is not null && Amount.Equals(other.Amount) && Currency.Equals(other.Currency);

    public int CompareTo(Money? other)
    {
        if (other is null)
        {
            return 1;
        }
        if (Currency != other.Currency)
        {
            throw new CurrencyMismatchException("Cannot compare Money with different currencies");
        }

        return Amount.CompareTo(other.Amount);
    }

    public override bool Equals(object? obj) => Equals(obj as Money);

    public override int GetHashCode() => HashCode.Combine(Amount, Currency);

    public static bool operator ==(Money? left, Money? right) => left is null ? right is null : left.Equals(right);

    public static bool operator !=(Money? left, Money? right) => !(left == right);

    public static bool operator <(Money left, Money right) => left.CompareTo(right) < 0;

    public static bool operator <=(Money left, Money right) => left.CompareTo(right) <= 0;

    public static bool operator >(Money left, Money right) => left.CompareTo(right) > 0;

    public static bool operator >=(Money left, Money right) => left.CompareTo(right) >= 0;
}
