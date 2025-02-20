using System.Collections.Generic;
using System.Linq;

namespace Kehlet.SourceGenerator;

public static class EnumerableExtensions
{
    public static CacheArray<T> ToCacheArray<T>(this IEnumerable<T> self) =>
        CacheArray.Create(self.ToArray());

    public static CacheStack<T> ToCacheStack<T>(this IEnumerable<T> self) =>
        CacheStack.Create(self.ToArray());
}
