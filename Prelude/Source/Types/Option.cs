#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Kehlet.SourceGenerator;

[DebuggerDisplay("None")]
internal readonly struct OptionNone;

[DebuggerDisplay("{ToString()}")]
internal readonly struct Option<T>(T value) : IEquatable<Option<T>>
    where T : notnull
{
    private readonly T value = value;

    public bool IsSome { get; } = true;

    public bool IsNone => !IsSome;

    public T UnsafeValue => IsSome ? value : throw new InvalidOperationException("Can't access value of None");

    public override string ToString() => IsSome ? $"Some {value.ToString()}" : "None";

    public override bool Equals(object? obj) => obj is Option<T> other && Equals(other);

    public bool Equals(Option<T> other)
    {
        return (IsSome, other.IsSome) switch
        {
            (true, true)   => EqualityComparer<T>.Default.Equals(value, other.value),
            (false, false) => true,
            _              => false
        };
    }

    public override int GetHashCode() => IsSome ? value.GetHashCode() : 0;

    public static implicit operator Option<T>(OptionNone _) => default;

    public static implicit operator Option<T>(T value) => new(value);

    public static Option<T> Some(T value) => new(value);

    public static Option<T> None => default;
}

internal static class Option
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Some<T>(T value) where T : notnull => new(value);

    public static readonly OptionNone None = default;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<TResult> Map<TSource, TResult>(this Option<TSource> self, Func<TSource, TResult> mapping)
        where TSource : notnull
        where TResult : notnull =>
        self.IsSome ? Some(mapping(self.UnsafeValue)) : None;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<TResult> Bind<TSource, TResult>(
        this Option<TSource> self,
        Func<TSource, Option<TResult>> binder)
        where TSource : notnull
        where TResult : notnull =>
        self.IsSome ? binder(self.UnsafeValue) : None;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<TResult> Select<TSource, TResult>(this Option<TSource> self, Func<TSource, TResult> selector)
        where TSource : notnull
        where TResult : notnull =>
        self.Map(selector);

    public static Option<TResult> SelectMany<TSource, TMiddle, TResult>(
        this Option<TSource> self,
        Func<TSource, Option<TMiddle>> middleSelector,
        Func<TSource, TMiddle, TResult> resultSelector)
        where TSource : notnull
        where TMiddle : notnull
        where TResult : notnull
    {
        if (self.IsNone)
        {
            return None;
        }

        var middle = middleSelector(self.UnsafeValue);
        if (middle.IsNone)
        {
            return None;
        }

        return resultSelector(self.UnsafeValue, middle.UnsafeValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<TSource> Where<TSource>(this Option<TSource> self, Func<TSource, bool> predicate)
        where TSource : notnull =>
        self.IsSome && predicate(self.UnsafeValue) ? self : None;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNullIfNotNull(nameof(value))]
    public static T? DefaultValue<T>(this Option<T> self, T? value) where T : notnull =>
        self.IsSome ? self.UnsafeValue : value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T DefaultWith<T>(this Option<T> self, Func<T> defThunk) where T : notnull =>
        self.IsSome ? self.UnsafeValue : defThunk();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Contains<T>(this Option<T> self, T item) where T : notnull =>
        self.IsSome && self.UnsafeValue.Equals(item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Exists<T>(this Option<T> self, Func<T, bool> predicate) where T : notnull =>
        self.IsSome && predicate(self.UnsafeValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Filter<T>(Option<T> self, Func<T, bool> predicate) where T : notnull =>
        self.IsSome && predicate(self.UnsafeValue) ? self : None;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Flatten<T>(this Option<Option<T>> self) where T : notnull =>
        self.IsSome ? self.UnsafeValue : None;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSome<T>(Option<T> option) where T : notnull => option.IsSome;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNone<T>(Option<T> option) where T : notnull => option.IsNone;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> OfNullable<T>(T? value) where T : struct =>
        value is not null ? Some(value.Value) : None;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> OfObject<T>(T? value) where T : class =>
        value is not null ? Some(value) : None;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> OrElse<T>(this Option<T> self, Option<T> ifNone) where T : notnull =>
        self.IsSome ? self : ifNone;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> OrElseWith<T>(this Option<T> self, Func<Option<T>> ifNoneThunk) where T : notnull =>
        self.IsSome ? self : ifNoneThunk();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? ToNullable<T>(this Option<T> self) where T : struct =>
        self.IsSome ? self.UnsafeValue : null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? ToObject<T>(this Option<T> self) where T : class =>
        self.IsSome ? self.UnsafeValue : null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Cast<T>(this Option<object> self)
        where T : notnull =>
        self.IsSome ? (T) self.UnsafeValue : None;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<TTo> Cast<TFrom, TTo>(this Option<TFrom> self)
        where TTo : TFrom
        where TFrom : notnull =>
        self.IsSome ? (TTo) self.UnsafeValue : None;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> OfType<T>(this Option<object> self)
        where T : notnull =>
        self.IsSome ? (self.UnsafeValue is T t ? t : None) : None;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> ToOption<T>(this T? value) where T : struct => value is null ? None : Some(value.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> ToOption<T>(this T? value) where T : class => value is null ? None : Some(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TResult Match<T, TResult>(this Option<T> self, Func<T, TResult> some, Func<TResult> none)
        where T : notnull
        where TResult : notnull =>
        self.IsSome switch
        {
            true  => some(self.UnsafeValue),
            false => none()
        };
}
