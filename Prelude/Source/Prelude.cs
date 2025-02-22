global using Kehlet.SourceGenerator;
global using static Kehlet.SourceGenerator.Prelude;
using System.Collections;
using System.Collections.Immutable;
using Kehlet.SourceGenerator.Source;

namespace Kehlet.SourceGenerator;

internal static class Prelude
{
    public static readonly Unit unit = default;

    public static Option<T> Some<T>(T value) where T : notnull => new(value);
    public static readonly OptionNone None = default;

    public static ResultOk<T> Ok<T>(T value) => new(value);
    public static ResultError<T> Error<T>(T error) => new(error);

    public static Unit Ignore(Action action)
    {
        action();
        return unit;
    }

    public static Unit Ignore<T>(this T self) => unit;

    public static readonly ObjectImmutableArraySequenceEqualityComparer SafeArrayComparer = new();
}

public static class Equality
{
    /// <summary>
    /// Pseudo-deep equality. Will use structural equality if <typeparamref name="T"/> implements <see cref="IStructuralEquatable"/>
    /// Otherwise falls back on default comparer.
    /// This is to ensure deep equality for nested <see cref="ImmutableArray{T}"/> in source generators.
    /// </summary>
    /// <param name="x">The first object to compare.</param>
    /// <param name="y">The second object to compare.</param>
    /// <typeparam name="T">The type of objects to compare.</typeparam>
    /// <returns>true if the specified objects are equal; otherwise, false.</returns>
    public static bool SmartEquals<T>(T x, T y) =>
        typeof(IStructuralEquatable).IsAssignableFrom(typeof(T))
            ? StructuralComparisons.StructuralEqualityComparer.Equals(x, y)
            : EqualityComparer<T>.Default.Equals(x, y);
}
