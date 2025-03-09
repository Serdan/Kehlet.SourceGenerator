using System;
using System.Collections.Immutable;

namespace Kehlet.SourceGenerator;

internal static class ImmutableDictionaryExtensions
{
    public static Option<T> GetValue<T>(this ImmutableDictionary<string, Option<T>> self, string key)
        where T : notnull =>
        self.TryGetValue(key, out var result) ? result : None;

    public static Option<T> GetValueAs<T>(this ImmutableDictionary<string, Option<object>> self, string key)
        where T : notnull =>
        self.TryGetValue(key, out var result) ? result.OfType<T>() : None;

    public static Option<TEnum> GetEnumValue<TEnum, TUnderlyingValue>(this ImmutableDictionary<string, Option<object>> self, string key)
        where TEnum : struct, Enum
        where TUnderlyingValue : unmanaged
    {
        if (!self.TryGetValue(key, out var result) || result.IsNone)
        {
            return None;
        }

        var value = result.OfType<TUnderlyingValue>().UnsafeValue;
        return EnumHelper.GetMember<TEnum, TUnderlyingValue>(value);
    }

    public static Option<TEnum> GetEnumValue<TEnum>(this ImmutableDictionary<string, Option<object>> self, string key)
        where TEnum : struct, Enum =>
        GetEnumValue<TEnum, int>(self, key);
}
