using System.Text.RegularExpressions;

namespace SmolHttp;

public partial class UrlValidator
{
    [GeneratedRegex(@"^(\/\w+)+(\.)\w+")]
    private static partial Regex UrlRegex();

    public static bool IsValidUrl(ReadOnlySpan<char> url)
    {
        return UrlRegex().IsMatch(url);
    }
}