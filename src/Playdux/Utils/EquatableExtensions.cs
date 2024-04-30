using System;

namespace Playdux.Utils;

public static class EquatableExtensions
{
    public static bool NullableEquals<T>(T? obj, T? other)
        where T : IEquatable<T>
    {
        if (!typeof(T).IsValueType && ReferenceEquals(obj, other)) return true;
        return (obj, other) switch
        {
            (null, null) => true,
            (null, _) => false,
            (_, null) => false,
            var (a, b) => a.Equals(b)
        };
    }
}