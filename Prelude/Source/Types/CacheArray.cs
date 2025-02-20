#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Kehlet.SourceGenerator;

/// <summary>
/// Cacheable wrapper type for <see cref="ImmutableArray{T}"/>.
/// </summary>
/// <param name="array">The array to wrap.</param>
/// <typeparam name="T">The type of array items.</typeparam>
[DebuggerDisplay("[{DebuggerString}]")]
[CollectionBuilder(typeof(CacheArray), nameof(CacheArray.Create))]
public readonly struct CacheArray<T>(ImmutableArray<T> array) : IEquatable<CacheArray<T>>, IImmutableList<T>
{
    public CacheArray() : this([]) { }

    private string DebuggerString => string.Join(", ", [..array.Take(3), ".."]);

    private readonly ImmutableArray<T> array = array;

    public ImmutableArray<T>.Enumerator GetEnumerator() => array.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) array).GetEnumerator();
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>) array).GetEnumerator();

    public bool Equals(CacheArray<T> other) => Equality<T>.ArrayComparer.Equals(array, other.array);

    public override bool Equals(object? obj) => obj is CacheArray<T> arr && Equals(arr);

    public override int GetHashCode() => Equality<T>.ArrayComparer.GetHashCode(array);

    public int Count => array.IsDefaultOrEmpty ? 0 : array.Length;

    public T this[int index] => array[index];

    public IImmutableList<T> Clear() => array.Clear();

    public int IndexOf(T item, int index, int count, IEqualityComparer<T>? equalityComparer) => array.IndexOf(item, index, count, equalityComparer);

    public int LastIndexOf(T item, int index, int count, IEqualityComparer<T>? equalityComparer) => array.LastIndexOf(item, index, count, equalityComparer);

    public IImmutableList<T> Add(T value) => array.Add(value);

    public IImmutableList<T> AddRange(IEnumerable<T> items) => array.AddRange(items);

    public IImmutableList<T> Insert(int index, T element) => array.Insert(index, element);

    public IImmutableList<T> InsertRange(int index, IEnumerable<T> items) => array.InsertRange(index, items);

    public IImmutableList<T> Remove(T value, IEqualityComparer<T>? equalityComparer = null) => array.Remove(value, equalityComparer);

    public IImmutableList<T> RemoveAll(Predicate<T> match) => array.RemoveAll(match);

    public IImmutableList<T> RemoveRange(IEnumerable<T> items, IEqualityComparer<T>? equalityComparer = null) => array.RemoveRange(items, equalityComparer);

    public IImmutableList<T> RemoveRange(int index, int count) => array.RemoveRange(index, count);

    public IImmutableList<T> RemoveAt(int index) => array.RemoveAt(index);

    public IImmutableList<T> SetItem(int index, T value) => array.SetItem(index, value);

    public IImmutableList<T> Replace(T oldValue, T newValue, IEqualityComparer<T>? equalityComparer = null) =>
        array.Replace(oldValue, newValue, equalityComparer);

    public static bool operator ==(CacheArray<T> x, CacheArray<T> y) => x.Equals(y);
    public static bool operator !=(CacheArray<T> x, CacheArray<T> y) => !(x == y);
}

internal static class CacheArray
{
    public static CacheArray<T> Create<T>(params ReadOnlySpan<T> items) => new(ImmutableArray.Create(items));
    public static CacheArray<T> Create<T>(ImmutableArray<T> array) => new(array);
    public static CacheArray<T> Create<T>(T[] array) => new(ImmutableArray.Create(array));

    public static string StringJoin<T>(this CacheArray<T> array, string separator) => string.Join(separator, array);
    public static string StringJoin<T>(this CacheArray<T> array, string separator, Func<T, string> selector) => string.Join(separator, array.Select(selector));
}
