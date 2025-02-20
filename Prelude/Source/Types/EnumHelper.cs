using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Kehlet.SourceGenerator;

/// <summary>
/// Static cache for enums
/// </summary>
/// <typeparam name="TEnum"></typeparam>
/// <typeparam name="TUnderlyingType"></typeparam>
internal static class EnumHelper<TEnum, TUnderlyingType>
    where TEnum : struct, Enum
    where TUnderlyingType : unmanaged
{
    private static readonly FrozenDictionary<TUnderlyingType, TEnum> members;
    public static readonly ImmutableArray<TEnum> Members;

    static EnumHelper()
    {
        var enumType = typeof(TEnum);
        var underlyingType = typeof(TUnderlyingType);
        var actualUnderlyingType = Enum.GetUnderlyingType(typeof(TEnum));
        if (actualUnderlyingType != underlyingType)
        {
            throw new InvalidOperationException(
                $"Incorrect underlying type for {enumType.FullName}. Given {underlyingType.FullName}, but should be {actualUnderlyingType.FullName}");
        }

        var builder = new Dictionary<TUnderlyingType, TEnum>();
        foreach (TUnderlyingType value in Enum.GetValues(typeof(TEnum)))
        {
            builder[value] = (TEnum) Enum.ToObject(typeof(TEnum), value);
        }

        members = builder.ToFrozenDictionary();
        Members = [..members.Values];
    }

    public static Option<TEnum> GetMember(TUnderlyingType value) =>
        members.TryGetValue(value, out var member) ? Some(member) : None;

    public static bool HasMember(TUnderlyingType value) => members.ContainsKey(value);
}

internal static class EnumHelper
{
    public static Option<TEnum> GetMember<TEnum, TUnderlyingType>(TUnderlyingType value)
        where TEnum : struct, Enum
        where TUnderlyingType : unmanaged =>
        EnumHelper<TEnum, TUnderlyingType>.GetMember(value);

    public static Option<TEnum> GetMember<TEnum>(int value)
        where TEnum : struct, Enum =>
        EnumHelper<TEnum, int>.GetMember(value);

    public static bool HasMember<TEnum, TUnderlyingType>(TUnderlyingType value)
        where TEnum : struct, Enum
        where TUnderlyingType : unmanaged =>
        EnumHelper<TEnum, TUnderlyingType>.HasMember(value);

    public static bool HasMember<TEnum>(int value)
        where TEnum : struct, Enum =>
        EnumHelper<TEnum, int>.HasMember(value);
}
