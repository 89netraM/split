using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Split.Utilities;

[DebuggerDisplay("Count = {Count}")]
[ExcludeFromCodeCoverage]
public class NonEmptyImmutableSet<T>(T first, params ImmutableHashSet<T> rest) : IImmutableSet<T>
{
    private readonly ImmutableHashSet<T> set = rest.Add(first);

    public bool IsEmpty => set.IsEmpty;

    public int Count => set.Count;

    public IImmutableSet<T> Add(T value) => set.Add(value);

    public IImmutableSet<T> Clear() => set.Clear();

    public bool Contains(T value) => set.Contains(value);

    public IImmutableSet<T> Except(IEnumerable<T> other) => set.Except(other);

    public IEnumerator<T> GetEnumerator() => set.GetEnumerator();

    public IImmutableSet<T> Intersect(IEnumerable<T> other) => set.Intersect(other);

    public bool IsProperSubsetOf(IEnumerable<T> other) => set.IsProperSubsetOf(other);

    public bool IsProperSupersetOf(IEnumerable<T> other) => set.IsProperSupersetOf(other);

    public bool IsSubsetOf(IEnumerable<T> other) => set.IsSubsetOf(other);

    public bool IsSupersetOf(IEnumerable<T> other) => set.IsSupersetOf(other);

    public bool Overlaps(IEnumerable<T> other) => set.Overlaps(other);

    public IImmutableSet<T> Remove(T value) => set.Remove(value);

    public bool SetEquals(IEnumerable<T> other) => set.SetEquals(other);

    public IImmutableSet<T> SymmetricExcept(IEnumerable<T> other) => set.SymmetricExcept(other);

    public bool TryGetValue(T equalValue, out T actualValue) => set.TryGetValue(equalValue, out actualValue);

    public IImmutableSet<T> Union(IEnumerable<T> other) => set.Union(other);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
