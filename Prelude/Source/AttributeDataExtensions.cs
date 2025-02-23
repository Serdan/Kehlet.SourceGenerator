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

    public static Option<TEnum> GetNamedArgumentAsEnum<TEnum>(this AttributeData? attribute, string parameterName)
        where TEnum : struct, Enum =>
        attribute?.NamedArguments.FirstOrDefault(x => x.Key == parameterName) is { Value.Value: int value }
            ? EnumHelper.GetMember<TEnum>(value)
            : None;

    public static Option<TEnum> GetNamedArgumentAsEnum<TEnum, TUnderlyingType>(this AttributeData? attribute, string parameterName)
        where TEnum : struct, Enum
        where TUnderlyingType : unmanaged =>
        attribute?.NamedArguments.FirstOrDefault(x => x.Key == parameterName) is { Value.Value: TUnderlyingType value }
            ? EnumHelper<TEnum, TUnderlyingType>.GetMember(value)
            : None;
}
