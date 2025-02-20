using System.Collections.Generic;
using System.Collections.Immutable;

namespace Kehlet.SourceGenerator;

internal static class Equality<T>
{
    public static readonly IEqualityComparer<ImmutableArray<T>> ArrayComparer = new ImmutableArraySequenceEqualityComparer<T>();
    public static readonly IEqualityComparer<ImmutableStack<T>> StackComparer = new ImmutableStackSequenceEqualityComparer<T>();
}
