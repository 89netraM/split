using System;
using System.Diagnostics;

namespace Split.Domain.Primitives;

[DebuggerDisplay("{Value}")]
public class UserId
{
    public Guid Value { get; }

    public UserId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("UserId cannot be empty", nameof(value));
        }

        Value = value;
    }

    public override string ToString() => Value.ToString();
}
