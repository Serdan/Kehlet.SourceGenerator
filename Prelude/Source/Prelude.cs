global using Kehlet.SourceGenerator;
global using static Kehlet.SourceGenerator.Prelude;
using System.Collections.Immutable;

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

    public static T Identity<T>(T value) => value;
}
