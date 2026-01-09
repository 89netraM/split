using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Split.Domain.Primitives;

[DebuggerDisplay("{Type}:{Id}")]
public class AlternateUserId : IEquatable<AlternateUserId>
{
    public string Type { get; }
    public string Id { get; }

    public AlternateUserId(string type, string id)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException("AlternateUserIds type cannot be empty", nameof(id));
        }
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("AlternateUserId id cannot be empty", nameof(id));
        }

        Type = type;
        Id = id;
    }

    [ExcludeFromCodeCoverage]
    public override string ToString() => $"{Type}:{Id}";

    public bool Equals(AlternateUserId? other) => other is not null && Type.Equals(other.Type) && Id.Equals(other.Id);

    [ExcludeFromCodeCoverage]
    public override bool Equals(object? obj) => Equals(obj as AlternateUserId);

    [ExcludeFromCodeCoverage]
    public override int GetHashCode() => HashCode.Combine(Type, Id);

    [ExcludeFromCodeCoverage]
    public static bool operator ==(AlternateUserId? left, AlternateUserId? right) =>
        left is null ? right is null : left.Equals(right);

    [ExcludeFromCodeCoverage]
    public static bool operator !=(AlternateUserId? left, AlternateUserId? right) => !(left == right);
}
