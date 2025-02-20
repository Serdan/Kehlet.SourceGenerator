global using static Kehlet.SourceGeneration.Prelude;
using System.Collections;

namespace Kehlet.SourceGeneration;

internal static class Prelude
{
    public static readonly Unit Unit = default;
    
    public static Option<T> Some<T>(T value) where T : struct => new(value);
    public static readonly OptionNone None = default;

    public static ResultOk<T> Ok<T>(T value) => new(value);
    public static ResultError<T> Error<T>(T error) => new(error);
}

public static class Equality
{
    public static bool SmartEquals<T>(T a, T b) =>
        typeof(IStructuralEquatable).IsAssignableFrom(typeof(T))
            ? StructuralComparisons.StructuralEqualityComparer.Equals(a, b)
            : EqualityComparer<T>.Default.Equals(a, b);
}
