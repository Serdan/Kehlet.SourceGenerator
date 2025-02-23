using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Kehlet.SourceGenerator.Source;

internal static class AttributeDataExtensions
{
    public static Option<SafeLocation> GetLocation(this AttributeData attribute) =>
        attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation() is { } location
            ? Some(SafeLocation.From(location))
            : None;

    public static Option<TEnum> GetArgumentAsEnum<TEnum>(this AttributeData? attribute, int argumentIndex = 0)
        where TEnum : struct, Enum =>
        attribute is { ConstructorArguments: var args } && args.Length > argumentIndex && args[argumentIndex] is { Value: int value }
            ? EnumHelper.GetMember<TEnum>(value)
            : None;

    public static Option<TEnum> GetArgumentAsEnum<TEnum, TUnderlyingType>(this AttributeData? attribute, int argumentIndex = 0)
        where TEnum : struct, Enum
        where TUnderlyingType : unmanaged =>
        attribute is { ConstructorArguments: var args } && args.Length > argumentIndex && args[argumentIndex] is { Value: TUnderlyingType value }
            ? EnumHelper<TEnum, TUnderlyingType>.GetMember(value)
            : None;

    public static Option<TEnum> GetNamedArgumentAsEnum<TEnum>(this AttributeData? attribute, string propertyName)
        where TEnum : struct, Enum =>
        attribute?.NamedArguments.FirstOrDefault(x => x.Key == propertyName) is { Value.Value: int value }
            ? EnumHelper.GetMember<TEnum>(value)
            : None;

    public static Option<TEnum> GetNamedArgumentAsEnum<TEnum, TUnderlyingType>(this AttributeData? attribute, string propertyName)
        where TEnum : struct, Enum
        where TUnderlyingType : unmanaged =>
        attribute?.NamedArguments.FirstOrDefault(x => x.Key == propertyName) is { Value.Value: TUnderlyingType value }
            ? EnumHelper<TEnum, TUnderlyingType>.GetMember(value)
            : None;

    public static Option<T> GetNamedArgumentAs<T>(this AttributeData? attribute, string propertyName)
        where T : notnull =>
        attribute?.NamedArguments.FirstOrDefault(arg => arg.Key == propertyName) is { Value.Value: T value }
            ? Some(value)
            : None;

    public static ImmutableDictionary<string, Option<object>> GetNamedArguments(this AttributeData attribute)
    {
        static object? GetValue(TypedConstant value)
        {
            return value switch
            {
                { IsNull: true } => null,
                { Kind: TypedConstantKind.Array } => value.Values.Select(GetValue).ToImmutableArray(),
                _ => value.Value
            };
        }

        var dict = new Dictionary<string, Option<object>>();
        foreach (var (key, argument) in attribute.NamedArguments)
        {
            dict[key] = Option.OfObject(GetValue(argument));
        }

        return dict.ToImmutableDictionary();
    }
}

internal static class KeyValuePairExtensions
{
    public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> pair, out TKey key, out TValue value)
    {
        key = pair.Key;
        value = pair.Value;
    }
}
