using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Kehlet.SourceGeneration;

internal readonly record struct ResultOk<T>(T Value);

internal readonly record struct ResultError<T>(T Error);

/// <summary>
/// Use the named constructors Result.Ok and Result.Error
/// </summary>
/// <param name="value"></param>
/// <param name="error"></param>
/// <param name="tag"></param>
/// <typeparam name="T"></typeparam>
/// <typeparam name="TError"></typeparam>
internal readonly struct Result<T, TError>(T value, TError error, int tag) : IEquatable<Result<T, TError>>
    where T : struct
    where TError : struct
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

    public T? ValueCase
    {
        get
        {
            Validate();
            return IsOk ? value : null;
        }
    }

    public TError? ErrorCase
    {
        get
        {
            Validate();
            return IsError ? error : null;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Result<T, TError>(ResultOk<T> result) => new(result.Value, default, 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Result<T, TError>(ResultError<TError> result) => new(default, result.Error, 2);

    public bool Equals(Result<T, TError> other)
    {
        Validate();
        other.Validate();
        return (IsOk, other.IsOk) switch
        {
            (true, true) => Equality.SmartEquals(value, other.value),
            (false, false) => Equality.SmartEquals(error, other.error),
            _ => false
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

    [Conditional("DEBUG")]
    private void Validate()
    {
        if (tag is 0)
        {
            throw new InvalidOperationException(
                "Result is invalid. Was defaulted rather than constructed by Ok() or Error()");
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
        where T : struct
        where TResult : struct
        where TError : struct =>
        self.IsOk ? Ok(mapping(self.UnsafeValue)) : Error(self.UnsafeError);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult, TError> Bind<T, TResult, TError>(
        this Result<T, TError> self,
        Func<T, Result<TResult, TError>> binder)
        where T : struct
        where TResult : struct
        where TError : struct =>
        self.IsOk ? binder(self.UnsafeValue) : Error(self.UnsafeError);
}
