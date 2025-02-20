using System.Runtime.CompilerServices;

namespace Kehlet.SourceGeneration;

internal readonly struct OptionNone;

internal readonly struct Option<T>(T value) : IEquatable<Option<T>>
    where T : struct
{
    private readonly T value = value;

    public bool IsSome { get; } = true;
    public bool IsNone => !IsSome;

    public T UnsafeValue => IsSome ? value : throw new InvalidOperationException("Can't access value of None");

    public T? Case => IsSome ? value : null;

    public override string ToString() => IsSome ? $"Some {value.ToString()}" : "None";

    public override bool Equals(object? obj) => obj is Option<T> other && Equals(other);

    public bool Equals(Option<T> other)
    {
        return (IsSome, other.IsSome) switch
        {
            (true, true) => Equality.SmartEquals(value, other.value),
            (false, false) => true,
            _ => false
        };
    }

    public override int GetHashCode() => IsSome ? value.GetHashCode() : 0;

    public static implicit operator Option<T>(OptionNone _) => default;
}

internal static class Option
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Some<T>(T value) where T : struct => new(value);

    public static readonly OptionNone None = default;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<TResult> Map<TSource, TResult>(this Option<TSource> self, Func<TSource, TResult> mapping)
        where TSource : struct
        where TResult : struct =>
        self.IsSome ? Some(mapping(self.UnsafeValue)) : None;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<TResult> Bind<TSource, TResult>(
        this Option<TSource> self,
        Func<TSource, Option<TResult>> binder)
        where TSource : struct
        where TResult : struct =>
        self.IsSome ? binder(self.UnsafeValue) : None;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T DefaultValue<T>(this Option<T> self, T value) where T : struct =>
        self.IsSome ? self.UnsafeValue : value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T DefaultWith<T>(this Option<T> self, Func<T> defThunk) where T : struct =>
        self.IsSome ? self.UnsafeValue : defThunk();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Contains<T>(this Option<T> self, T item) where T : struct =>
        self.IsSome && Equality.SmartEquals(self.UnsafeValue, item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Exists<T>(this Option<T> self, Func<T, bool> predicate) where T : struct =>
        self.IsSome && predicate(self.UnsafeValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Filter<T>(Option<T> self, Func<T, bool> predicate) where T : struct =>
        self.IsSome && predicate(self.UnsafeValue) ? self : None;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Flatten<T>(this Option<Option<T>> self) where T : struct =>
        self.IsSome ? self.UnsafeValue : None;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSome<T>(this Option<T> self) where T : struct => self.IsSome;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNone<T>(this Option<T> self) where T : struct => self.IsNone;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> OfNullable<T>(T? value) where T : struct => value is null ? None : Some(value.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> OrElse<T>(this Option<T> self, Option<T> ifNone) where T : struct =>
        self.IsSome ? self : ifNone;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> OrElseWith<T>(this Option<T> self, Func<Option<T>> ifNoneThunk) where T : struct =>
        self.IsSome ? self : ifNoneThunk();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? ToNullable<T>(this Option<T> self) where T : struct => self.Case;
}
