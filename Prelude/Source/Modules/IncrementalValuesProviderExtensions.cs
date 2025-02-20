using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Kehlet.SourceGenerator;

internal static class IncrementalValuesProviderExtensions
{
    public static IncrementalValuesProvider<TResult> Select<TSource, TResult>(
        this IncrementalValuesProvider<TSource> self, Func<TSource, TResult> selector) =>
        self.Select((item, _) => selector(item));

    public static IncrementalValuesProvider<TResult> SelectMany<TSource, TResult>(
        this IncrementalValuesProvider<TSource> self,
        Func<TSource, ImmutableArray<TResult>> selector) =>
        self.SelectMany((item, _) => selector(item));

    public static IncrementalValuesProvider<TResult> Choose<TSource, TResult>(
        this IncrementalValuesProvider<TSource> self,
        Func<TSource, Option<TResult>> chooser)
        where TResult : notnull =>
        self.SelectMany(item => chooser(item) is { IsSome: true } opt ? [opt.UnsafeValue] : ImmutableArray<TResult>.Empty);

    public static IncrementalValuesProvider<TSource> Choose<TSource>(this IncrementalValuesProvider<Option<TSource>> self)
        where TSource : notnull =>
        self.SelectMany(item => item.IsSome ? [item.UnsafeValue] : ImmutableArray<TSource>.Empty);

    public static IncrementalValuesProvider<TSource> Values<TSource, TError>(this IncrementalValuesProvider<Result<TSource, TError>> self)
        where TSource : notnull
        where TError : notnull =>
        self.SelectMany(item => item.IsOk ? [item.UnsafeValue] : ImmutableArray<TSource>.Empty);

    public static IncrementalValuesProvider<TError> Errors<TSource, TError>(this IncrementalValuesProvider<Result<TSource, TError>> self)
        where TSource : notnull
        where TError : notnull =>
        self.SelectMany(item => item.IsOk ? ImmutableArray<TError>.Empty : [item.UnsafeError]);

    public static (IncrementalValuesProvider<TValue> Values, IncrementalValuesProvider<TError> Errors) Partition<TValue, TError>(
        this IncrementalValuesProvider<Result<TValue, TError>> self)
        where TValue : notnull
        where TError : notnull =>
        (self.Values(), self.Errors());

    public static (IncrementalValuesProvider<TSource>, IncrementalValuesProvider<TSource>) Partition<TSource>(
        this IncrementalValuesProvider<TSource> self,
        Func<TSource, bool> predicate) =>
        (self.Where(predicate), self.Where(item => !predicate(item)));
}
