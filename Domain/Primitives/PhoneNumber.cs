using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Split.Domain.Primitives;

[DebuggerDisplay("{Value}")]
public partial class PhoneNumber
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

    public override string ToString() => Value;
}
