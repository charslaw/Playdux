using System;

namespace Playdux.Utils;

public static class EquatableUtils
{
    public static bool NullableEquals<T, TEquatable>(TEquatable? obj, TEquatable? other)
        where TEquatable : IEquatable<T>
    {
        return (obj, other) switch
        {
            (null, null) => true,
            (null, _) => false,
            (_, null) => false,
            var (a, b) => a.Equals(b)
        };
    }
}