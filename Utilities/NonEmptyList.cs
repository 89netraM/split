using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Split.Utilities;

[DebuggerDisplay("Count = {Count}")]
[ExcludeFromCodeCoverage]
public class NonEmptyList<T> : IList<T>
{
    private readonly List<T> list;

    public int Count => list.Count;

    public bool IsReadOnly => ((IList<T>)list).IsReadOnly;

    public T this[int index]
    {
        get => list[index];
        set => list[index] = value;
    }

    public NonEmptyList(T first, params List<T> rest)
    {
        rest.Add(first);
        list = rest;
    }

    [Obsolete("For EF Core only", error: true)]
    public NonEmptyList()
    {
        list = [];
    }

    public int IndexOf(T item) => list.IndexOf(item);

    public void Insert(int index, T item) => list.Insert(index, item);

    public void RemoveAt(int index)
    {
        if (Count <= 1)
        {
            throw new InvalidOperationException("Cannot remove the last item from a NonEmptyList.");
        }
        list.RemoveAt(index);
    }

    public void Add(T item) => list.Add(item);

    public void Clear() =>
        throw new InvalidOperationException("Cannot clear a NonEmptyList, it must always contain at least one item.");

    public bool Contains(T item) => list.Contains(item);

    public void CopyTo(T[] array, int arrayIndex) => list.CopyTo(array, arrayIndex);

    public bool Remove(T item)
    {
        if (Count <= 1)
        {
            throw new InvalidOperationException("Cannot remove the last item from a NonEmptyList.");
        }
        return list.Remove(item);
    }

    public IEnumerator<T> GetEnumerator() => list.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
