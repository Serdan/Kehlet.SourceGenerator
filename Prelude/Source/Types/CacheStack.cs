using System.Collections;
using System.Collections.Immutable;

namespace Kehlet.SourceGenerator;

public readonly struct CacheStack<T>(ImmutableStack<T> stack) : IImmutableStack<T>, IEquatable<CacheStack<T>>
{
    private readonly ImmutableStack<T> stack = stack;

    public ImmutableStack<T>.Enumerator GetEnumerator() => stack.GetEnumerator();

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>) stack).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) stack).GetEnumerator();

    public IImmutableStack<T> Clear() => stack.Clear();

    public IImmutableStack<T> Push(T value) => stack.Push(value);

    public IImmutableStack<T> Pop() => stack.Pop();

    public T Peek() => stack.Peek();

    public bool IsEmpty => stack.IsEmpty;

    public bool Equals(CacheStack<T> other) => Equality<T>.StackComparer.Equals(stack, other.stack);

    public override bool Equals(object? obj) => obj is CacheStack<T> other && Equals(other);

    public override int GetHashCode() => Equality<T>.StackComparer.GetHashCode(stack);

    public static bool operator ==(CacheStack<T> x, CacheStack<T> y) => x.Equals(y);
    public static bool operator !=(CacheStack<T> x, CacheStack<T> y) => !(x == y);
}

internal static class CacheStack
{
    public static CacheStack<T> Create<T>(params ReadOnlySpan<T> items) => new(ImmutableStack.Create(items));
}
