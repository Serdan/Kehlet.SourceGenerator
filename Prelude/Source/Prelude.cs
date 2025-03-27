#if !NO_GLOBAL_USINGS
global using Kehlet.SourceGenerator;

global using static Kehlet.SourceGenerator.Prelude;
#endif
using System;

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

    public static Unit Ignore<T>(T self) => unit;

    public static T Identity<T>(T value) => value;
}

internal static class PreludeExtensions
{
    public static Option<T> Some<T>(this T value) where T : notnull => new(value);
    public static ResultOk<T> Ok<T>(this T value) => new(value);
    public static ResultError<T> Error<T>(this T error) => new(error);
    public static Unit Ignore<T>(this T self) => unit;
    public static TResult Apply<T, TResult>(this T self, Func<T, TResult> f) => f(self);
}