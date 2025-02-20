#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Kehlet.SourceGenerator;

/// <summary>
/// Cacheable wrapper type for <see cref="ImmutableStack{T}"/>.
/// </summary>
/// <param name="stack">Stack to wrap.</param>
/// <typeparam name="T">Type of stack items.</typeparam>
[CollectionBuilder(typeof(CacheStack), nameof(CacheStack.Create))]
public readonly struct CacheStack<T>(ImmutableStack<T> stack) : IImmutableStack<T>, IEquatable<CacheStack<T>>
{
    private readonly ImmutableStack<T> stack = stack;

    public ImmutableStack<T>.Enumerator GetEnumerator() => stack.GetEnumerator();

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>) stack).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) stack).GetEnumerator();
    IImmutableStack<T> IImmutableStack<T>.Push(T value) => stack.Push(value);
    IImmutableStack<T> IImmutableStack<T>.Pop() => stack.Pop();
    IImmutableStack<T> IImmutableStack<T>.Clear() => stack.Clear();

    public CacheStack<T> Push(T value) => CacheStack.Create(stack.Push(value));
    public CacheStack<T> Pop() => CacheStack.Create(stack.Pop());
    public CacheStack<T> Pop(out T value) => CacheStack.Create(stack.Pop(out value));
    public CacheStack<T> Clear() => CacheStack.Create(stack.Clear());

    public T Peek() => stack.Peek();

    public bool IsEmpty => stack.IsEmpty;

    public bool Equals(CacheStack<T> other) => Equality<T>.StackComparer.Equals(stack, other.stack);

    public override bool Equals(object? obj) => obj is CacheStack<T> other && Equals(other);

    public override int GetHashCode() => Equality<T>.StackComparer.GetHashCode(stack);

    public static bool operator ==(CacheStack<T> x, CacheStack<T> y) => x.Equals(y);
    public static bool operator !=(CacheStack<T> x, CacheStack<T> y) => !(x == y);

    public static CacheStack<T> Empty => new([]);
}

internal static class CacheStack
{
    public static CacheStack<T> Create<T>(params ReadOnlySpan<T> items) => new(ImmutableStack.Create(items));
    public static CacheStack<T> Create<T>(ImmutableStack<T> stack) => new(stack);
    public static CacheStack<T> Create<T>(T[] stack) => new(ImmutableStack.Create(stack));
}
