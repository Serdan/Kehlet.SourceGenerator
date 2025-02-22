using Microsoft.CodeAnalysis;

namespace Kehlet.SourceGenerator;

public static class LocationExtensions
{
    /// <summary>
    /// <see cref="Location"/>s may hold references to the <see cref="SyntaxTree"/>. This method creates a new <see cref="Location"/> with only cacheable data.
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    public static Location ToCacheable(this Location location) =>
        Location.Create(location.SourceTree?.FilePath ?? string.Empty, location.SourceSpan, location.GetLineSpan().Span);
}
