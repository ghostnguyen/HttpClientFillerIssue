using System.Collections;
using System.Runtime.CompilerServices;

namespace HttpClientFiller.Extension;

public static class CollectionFormat
{
    public static string Multi(this IEnumerable items, [CallerArgumentExpression("items")] string? paramName = null)
    {
        var l = new List<string>();
        foreach (var item in items)
        {
            if (item != null)
                l.Add($"{paramName}={item}");
        }
        return string.Join('&', l);
    }

    public static string Csv(this IEnumerable items)
    {
        var l = new List<string>();
        foreach (var item in items)
        {
            if (item != null)
                l.Add($"{item}");
        }
        return string.Join("%2C", l);
    }

    public static string Pipe(this IEnumerable items)
    {
        var l = new List<string>();
        foreach (var item in items)
        {
            if (item != null)
                l.Add($"{item}");
        }
        return string.Join("|", l);
    }

    public static string Ssv(this IEnumerable items)
    {
        var l = new List<string>();
        foreach (var item in items)
        {
            if (item != null)
                l.Add($"{item}");
        }
        return string.Join(" ", l);
    }
}