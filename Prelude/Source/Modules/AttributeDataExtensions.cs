#nullable enable
using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Kehlet.SourceGenerator;

internal static class AttributeDataExtensions
{
    public static Option<TEnum> GetArgumentAsEnum<TEnum>(this AttributeData? attribute, int argumentIndex = 0)
        where TEnum : struct, Enum =>
        GetArgumentAsEnum<TEnum, int>(attribute, argumentIndex);

    public static Option<TEnum> GetArgumentAsEnum<TEnum, TUnderlyingType>(this AttributeData? attribute, int argumentIndex = 0)
        where TEnum : struct, Enum
        where TUnderlyingType : unmanaged =>
        attribute is { ConstructorArguments: var args } && args.Length > argumentIndex && args[argumentIndex] is { Value: TUnderlyingType value }
            ? EnumHelper<TEnum, TUnderlyingType>.GetMember(value)
            : None;

    public static Option<TEnum> GetNamedArgumentAsEnum<TEnum>(this AttributeData? attribute, string propertyName)
        where TEnum : struct, Enum =>
        GetNamedArgumentAsEnum<TEnum, int>(attribute, propertyName);

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

    /// <summary>
    /// Returns false if the property has been passed a value that is not in the enum, otherwise true.
    /// Does not account for flags.
    /// </summary>
    /// <param name="attribute"></param>
    /// <param name="argumentIndex"></param>
    /// <typeparam name="TEnum"></typeparam>
    /// <typeparam name="TUnderlyingType"></typeparam>
    /// <returns></returns>
    public static bool ValidateEnumArgument<TEnum, TUnderlyingType>(this AttributeData? attribute, int argumentIndex = 0)
        where TEnum : struct, Enum
        where TUnderlyingType : unmanaged =>
        // ReSharper disable once SimplifyConditionalTernaryExpression
        attribute is { ConstructorArguments: var args } && args.Length > argumentIndex && args[argumentIndex] is { Value: TUnderlyingType value }
            ? EnumHelper<TEnum, TUnderlyingType>.HasMember(value)
            : true;

    /// <summary>
    /// Returns false if the property has been passed a value that is not in the enum, otherwise true.
    /// Does not account for flags.
    /// </summary>
    /// <param name="attribute"></param>
    /// <param name="argumentIndex"></param>
    /// <typeparam name="TEnum"></typeparam>
    /// <returns></returns>
    public static bool ValidateEnumArgument<TEnum>(this AttributeData? attribute, int argumentIndex = 0)
        where TEnum : struct, Enum =>
        ValidateEnumArgument<TEnum, int>(attribute, argumentIndex);

    /// <summary>
    /// Returns false if the parameter has been passed a value that is not in the enum, otherwise true.
    /// Does not account for flags.
    /// </summary>
    /// <param name="attribute"></param>
    /// <param name="propertyName">Name of the named argument</param>
    /// <typeparam name="TEnum">The enum type</typeparam>
    /// <typeparam name="TUnderlyingType">The underlying type of TEnum</typeparam>
    /// <returns></returns>
    public static bool ValidateNamedEnumArgument<TEnum, TUnderlyingType>(this AttributeData? attribute, string propertyName)
        where TEnum : struct, Enum
        where TUnderlyingType : unmanaged =>
        // ReSharper disable once SimplifyConditionalTernaryExpression
        attribute?.NamedArguments.FirstOrDefault(pair => pair.Key == propertyName) is { Value.Value: TUnderlyingType value }
            ? EnumHelper<TEnum, TUnderlyingType>.HasMember(value)
            : true;

    /// <summary>
    /// Returns false if the parameter has been passed a value that is not in the enum, otherwise true.
    /// Does not account for flags.
    /// </summary>
    /// <param name="attribute"></param>
    /// <param name="propertyName">Name of the named argument</param>
    /// <typeparam name="TEnum">The enum type</typeparam>
    /// <returns></returns>
    public static bool ValidateNamedEnumArgument<TEnum>(this AttributeData? attribute, string propertyName)
        where TEnum : struct, Enum =>
        ValidateNamedEnumArgument<TEnum, int>(attribute, propertyName);


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

        var dict = ImmutableDictionary.CreateBuilder<string, Option<object>>();
        foreach (var (key, argument) in attribute.NamedArguments)
        {
            dict[key] = Option.OfObject(GetValue(argument));
        }

        return dict.ToImmutableDictionary();
    }
}
