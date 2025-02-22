using Microsoft.CodeAnalysis;

namespace Kehlet.SourceGenerator.Source;

internal static class AttributeDataExtensions
{
    public static Option<SafeLocation> GetLocation(this AttributeData attribute) =>
        attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation() is { } location
            ? Some(SafeLocation.From(location))
            : None;

    public static Option<TEnum> GetFirstArgumentAsEnum<TEnum>(this AttributeData? attribute) where TEnum : struct, Enum =>
        attribute is { ConstructorArguments: [{ Value: int value }, ..] }
            ? EnumHelper.GetMember<TEnum>(value)
            : None;

    public static Option<TEnum> GetFirstArgumentAsEnum<TEnum, TUnderlyingType>(this AttributeData? attribute)
        where TEnum : struct, Enum
        where TUnderlyingType : unmanaged =>
        attribute is { ConstructorArguments: [{ Value: TUnderlyingType value }, ..] }
            ? EnumHelper<TEnum, TUnderlyingType>.GetMember(value)
            : None;

    public static Option<TEnum> GetNamedArgumentAsEnum<TEnum>(this AttributeData? attribute, string name) where TEnum : struct, Enum =>
        attribute?.NamedArguments.FirstOrDefault(x => x.Key == name) is { Value.Value: int value }
            ? EnumHelper.GetMember<TEnum>(value)
            : None;

    public static Option<TEnum> GetNamedArgumentAsEnum<TEnum, TUnderlyingType>(this AttributeData? attribute, string name)
        where TEnum : struct, Enum
        where TUnderlyingType : unmanaged =>
        attribute?.NamedArguments.FirstOrDefault(x => x.Key == name) is { Value.Value: TUnderlyingType value }
            ? EnumHelper<TEnum, TUnderlyingType>.GetMember(value)
            : None;
}
