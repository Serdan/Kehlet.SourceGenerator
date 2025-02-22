using System.Collections.Immutable;

namespace Kehlet.SourceGenerator.Source;

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
