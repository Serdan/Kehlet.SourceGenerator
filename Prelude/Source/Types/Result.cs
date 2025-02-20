#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Kehlet.SourceGenerator;

[DebuggerDisplay("Ok {Value}")]
internal readonly record struct ResultOk<T>(T Value);

[DebuggerDisplay("Error {Error}")]
internal readonly record struct ResultError<T>(T Error);

/// <summary>
/// Use the named constructors Result.Ok and Result.Error
/// </summary>
/// <param name="value"></param>
/// <param name="error"></param>
/// <param name="tag"></param>
/// <typeparam name="T"></typeparam>
/// <typeparam name="TError"></typeparam>
[DebuggerDisplay("{ToString()}")]
internal readonly struct Result<T, TError>(T value, TError error, int tag) : IEquatable<Result<T, TError>>
    where T : notnull
    where TError : notnull
{
    private readonly int tag = tag;
    private readonly T value = value;
    private readonly TError error = error;

    public bool IsOk
    {
        get
        {
            Validate();
            return tag == 1;
        }
    }

    public bool IsError
    {
        get
        {
            Validate();
            return tag == 2;
        }
    }

    public T UnsafeValue
    {
        get
        {
            Validate();
            return IsOk ? value : throw new InvalidOperationException("Can't access value of Error");
        }
    }

    public TError UnsafeError
    {
        get
        {
            Validate();
            return IsError ? error : throw new InvalidOperationException("Can't access error of Ok");
        }
    }

    public override string ToString() => IsOk ? $"Ok {value}" : $"Error {error}";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Result<T, TError>(ResultOk<T> result) => new(result.Value, default!, 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Result<T, TError>(ResultError<TError> result) => new(default!, result.Error, 2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Result<T, TError>(T value) => new(value, default!, 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Result<T, TError>(TError error) => new(default!, error, 2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, TError> Ok(T value) => new(value, default!, 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, TError> Error(TError error) => new(default!, error, 2);

    public bool Equals(Result<T, TError> other)
    {
        Validate();
        other.Validate();
        return (IsOk, other.IsOk) switch
        {
            (true, true)   => EqualityComparer<T>.Default.Equals(value, other.value),
            (false, false) => EqualityComparer<TError>.Default.Equals(error, other.error),
            _              => false
        };
    }

    public override bool Equals(object? obj)
    {
        Validate();
        return obj is Result<T, TError> other && Equals(other);
    }

    public override int GetHashCode()
    {
        Validate();
        return IsOk ? value.GetHashCode() : error.GetHashCode();
    }

    public static bool operator ==(Result<T, TError> x, Result<T, TError> y) => x.Equals(y);
    public static bool operator !=(Result<T, TError> x, Result<T, TError> y) => !(x == y);

    [Conditional("DEBUG")]
    private void Validate()
    {
        if (tag is 0)
        {
            throw new InvalidOperationException(
                "Result instance is invalid. Was defaulted rather than constructed by Ok() or Error()");
        }
    }
}

internal static class Result
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ResultOk<T> Ok<T>(T value) => new(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ResultError<T> Error<T>(T error) => new(error);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult, TError> Map<T, TResult, TError>(this Result<T, TError> self, Func<T, TResult> mapping)
        where T : notnull
        where TResult : notnull
        where TError : notnull =>
        self.IsOk ? Ok(mapping(self.UnsafeValue)) : Error(self.UnsafeError);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult, TError> Bind<T, TResult, TError>(
        this Result<T, TError> self,
        Func<T, Result<TResult, TError>> binder)
        where T : notnull
        where TResult : notnull
        where TError : notnull =>
        self.IsOk ? binder(self.UnsafeValue) : Error(self.UnsafeError);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult, TError> Select<TSource, TResult, TError>(this Result<TSource, TError> self, Func<TSource, Result<TResult, TError>> selector)
        where TSource : notnull
        where TResult : notnull
        where TError : notnull =>
        self.Bind(selector);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult, TError> Select<TSource, TResult, TError>(this Result<TSource, TError> self, Func<TSource, TResult> selector)
        where TSource : notnull
        where TResult : notnull
        where TError : notnull =>
        self.Map(selector);

    public static Result<TResult, TError> SelectMany<TSource, TMiddle, TResult, TError>(
        this Result<TSource, TError> self,
        Func<TSource, Result<TMiddle, TError>> firstSelector,
        Func<TSource, TMiddle, Result<TResult, TError>> resultSelector)
        where TSource : notnull
        where TMiddle : notnull
        where TResult : notnull
        where TError : notnull
    {
        if (self.IsError)
        {
            return self.UnsafeError;
        }

        var middle = firstSelector(self.UnsafeValue);
        if (middle.IsError)
        {
            return middle.UnsafeError;
        }

        return resultSelector(self.UnsafeValue, middle.UnsafeValue);
    }

    public static Result<TResult, TError> SelectMany<TSource, TMiddle, TResult, TError>(
        this Result<TSource, TError> self,
        Func<TSource, Result<TMiddle, TError>> firstSelector,
        Func<TSource, TMiddle, TResult> resultSelector)
        where TSource : notnull
        where TMiddle : notnull
        where TResult : notnull
        where TError : notnull
    {
        if (self.IsError)
        {
            return self.UnsafeError;
        }

        var middle = firstSelector(self.UnsafeValue);
        if (middle.IsError)
        {
            return middle.UnsafeError;
        }

        return resultSelector(self.UnsafeValue, middle.UnsafeValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TSource, TError> Where<TSource, TError>(this Result<TSource, TError> self, Func<TSource, bool> predicate)
        where TSource : notnull
        where TError : notnull =>
        self.IsOk && predicate(self.UnsafeValue) ? self : self.UnsafeError;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T DefaultValue<T, TError>(this Result<T, TError> self, T value)
        where T : notnull
        where TError : notnull =>
        self.IsOk ? self.UnsafeValue : value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T DefaultWith<T, TError>(this Result<T, TError> self, Func<T> defThunk)
        where T : notnull
        where TError : notnull =>
        self.IsOk ? self.UnsafeValue : defThunk();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Exists<T, TError>(this Result<T, TError> self, Func<T, bool> predicate)
        where T : notnull
        where TError : notnull =>
        self.IsOk && predicate(self.UnsafeValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOk<T, TError>(Result<T, TError> result)
        where T : notnull
        where TError : notnull =>
        result.IsOk;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsError<T, TError>(Result<T, TError> result)
        where T : notnull
        where TError : notnull =>
        result.IsError;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Unit Iter<T, TError>(this Result<T, TError> self, Func<T, Unit> action)
        where T : notnull
        where TError : notnull =>
        self.IsOk ? action(self.UnsafeValue) : unit;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, TError2> MapError<T, TError, TError2>(this Result<T, TError> self, Func<TError, TError2> mapping)
        where T : notnull
        where TError : notnull
        where TError2 : notnull =>
        self.IsError ? Error(mapping(self.UnsafeError)) : Ok(self.UnsafeValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> ToOption<T, TError>(this Result<T, TError> self)
        where T : notnull
        where TError : notnull =>
        self.IsOk ? Some(self.UnsafeValue) : None;
}
