#nullable enable
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Kehlet.SourceGenerator;

// Copied from RegexGenerator source. MIT license.
internal sealed class ObjectImmutableArraySequenceEqualityComparer : IEqualityComparer<ImmutableArray<object>>
{
    public bool Equals(ImmutableArray<object> left, ImmutableArray<object> right)
    {
        if (left.Length != right.Length)
        {
            return false;
        }

        for (var i = 0; i < left.Length; i++)
        {
            var areEqual = left[i] is { } leftElem
                ? leftElem.Equals(right[i])
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                : right[i] is null;

            if (!areEqual)
            {
                return false;
            }
        }

        return true;
    }

    public int GetHashCode(ImmutableArray<object> obj)
    {
        var hash = 0;
        for (var i = 0; i < obj.Length; i++)
        {
            hash = (hash, obj[i]).GetHashCode();
        }

        return hash;
    }
}

internal sealed class ImmutableArraySequenceEqualityComparer<T> : IEqualityComparer<ImmutableArray<T>>
{
    public bool Equals(ImmutableArray<T> left, ImmutableArray<T> right)
    {
        if (left.Length != right.Length)
        {
            return false;
        }

        for (var i = 0; i < left.Length; i++)
        {
            var areEqual = left[i] is { } leftElem
                ? leftElem.Equals(right[i])
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                : right[i] is null;

            if (!areEqual)
            {
                return false;
            }
        }

        return true;
    }

    public int GetHashCode(ImmutableArray<T> obj)
    {
        var hash = 0;
        for (var i = 0; i < obj.Length; i++)
        {
            hash = (hash, obj[i]).GetHashCode();
        }

        return hash;
    }
}

internal sealed class ImmutableStackSequenceEqualityComparer<T> : IEqualityComparer<ImmutableStack<T>>
{
    public bool Equals(ImmutableStack<T>? x, ImmutableStack<T>? y)
    {
        if (x is null && y is null)
        {
            return true;
        }

        if (x is null || y is null)
        {
            return false;
        }
        
        if (x.IsEmpty && y.IsEmpty)
        {
            return true;
        }

        if (x.IsEmpty || y.IsEmpty)
        {
            return false;
        }

        var xEnumerator = x.GetEnumerator();
        var yEnumerator = y.GetEnumerator();
        while (true)
        {
            var xNext = xEnumerator.MoveNext();
            var yNext = yEnumerator.MoveNext();
            switch (xNext, yNext)
            {
                case (true, true):
                    if (xEnumerator.Current?.Equals(yEnumerator.Current) ?? yEnumerator.Current is not null)
                    {
                        return false;
                    }

                    break;
                case (true, false) or (false, true): return false;
                case (false, false): return true;
            }
        }
    }

    public int GetHashCode(ImmutableStack<T> obj)
    {
        var hash = 0;
        foreach (var item in obj)
        {
            hash = (hash, item).GetHashCode();
        }

        return hash;
    }
}
