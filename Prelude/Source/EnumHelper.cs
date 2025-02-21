using System.Collections.Frozen;

namespace Kehlet.SourceGenerator;

internal static class EnumHelper<TEnum, TValue>
    where TEnum : struct, Enum
    where TValue : unmanaged
{
    private static readonly FrozenDictionary<TValue, TEnum> members;

    static EnumHelper()
    {
        var builder = new Dictionary<TValue, TEnum>();
        foreach (TValue value in Enum.GetValues(typeof(TEnum)))
        {
            builder[value] = (TEnum) Enum.ToObject(typeof(TEnum), value);
        }

        members = builder.ToFrozenDictionary();
    }

    public static Option<TEnum> GetMember(TValue value) =>
        members.TryGetValue(value, out var member) ? Some(member) : None;

    public static bool HasMember(TValue value) => members.ContainsKey(value);
}

internal static class EnumHelper
{
    public static Option<TEnum> GetMember<TEnum, TValue>(TValue value)
        where TEnum : struct, Enum
        where TValue : unmanaged =>
        EnumHelper<TEnum, TValue>.GetMember(value);

    public static Option<TEnum> GetMember<TEnum>(int value)
        where TEnum : struct, Enum =>
        EnumHelper<TEnum, int>.GetMember(value);

    public static bool HasMember<TEnum, TValue>(TValue value)
        where TEnum : struct, Enum
        where TValue : unmanaged =>
        EnumHelper<TEnum, TValue>.HasMember(value);

    public static bool HasMember<TEnum>(int value)
        where TEnum : struct, Enum =>
        EnumHelper<TEnum, int>.HasMember(value);
}
