global using Kehlet.SourceGenerator;
global using static Kehlet.SourceGenerator.Prelude;
using System.Collections;

namespace Kehlet.SourceGenerator;

internal static class Prelude
{
    // ReSharper disable once InconsistentNaming
    public static readonly Unit unit = default;

    public static Option<T> Some<T>(T value) where T : notnull => new(value);
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
