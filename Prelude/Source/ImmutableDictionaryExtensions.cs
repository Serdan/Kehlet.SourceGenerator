using System.Collections.Immutable;

namespace Kehlet.SourceGenerator.Source;

internal static class ImmutableDictionaryExtensions
{
    public static Option<T> GetValue<T>(this ImmutableDictionary<string, Option<T>> self, string key)
        where T : notnull =>
        self.TryGetValue(key, out var result) ? result : None;

    public static Option<T> GetValueAs<T>(this ImmutableDictionary<string, Option<object>> self, string key)
        where T : notnull =>
        self.TryGetValue(key, out var result) ? result.OfType<T>() : None;
}
