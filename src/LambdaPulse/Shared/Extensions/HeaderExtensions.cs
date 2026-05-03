using System.Globalization;

namespace LambdaPulse.Server.Shared.Extensions;

public static class HeaderExtensions
{
    /// <summary>
    /// Parses 'Cookie' header from HTTP request. Returns a dictionary of cookie names and values.
    /// E.g. Cookie: sessionId=12345; theme=dark;
    /// </summary>
    public static Dictionary<string, string> ParseCookies(this IDictionary<string, string> headers)
    {
        //Cookie names are case-insensitive
        var cookies = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (!headers.TryGetValue("Cookie", out var header))
        {
            return cookies;
        }

        var cookiesRaw = header.Split(';', StringSplitOptions.RemoveEmptyEntries);

        foreach (var cookieRaw in cookiesRaw)
        {
            var cookieKeyValue = cookieRaw.Split('=', 2);
            if (cookieKeyValue.Length == 2)
            {
                cookies[cookieKeyValue[0].Trim()] = cookieKeyValue[1].Trim();
            }
        }

        return cookies;
    }

    /// <summary>
    /// Parses Accept header from HTTP request. Returns a queue of MIME types, ordered by preference.
    /// E.g. Accept: text/html, application/xml;q=0.9, */*;q=0.8
    /// </summary>
    public static Queue<string> ParseAcceptHeader(this IDictionary<string, string> headers)
    {
        //list to hold intermediate mime type entries
        var mimeTypeEntries = new List<MimeTypeEntry>();

        headers.TryGetValue("Accept", out var acceptHeaderValue);

        if (string.IsNullOrWhiteSpace(acceptHeaderValue))
        {
            return new Queue<string>();
        }

        var entries = acceptHeaderValue.Split(',');
        for (int i = 0; i < entries.Length; i++)
        {
            var components = entries[i].Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var mimeType = components[0];

            //set a default q value, if not specified
            var qValue = 1.0;

            //can contain custom parameters. E.g application/xml;version=2;q=0.9
            for (int j = 1; j < components.Length; j++)
            {
                if (components[j].StartsWith("q=", StringComparison.OrdinalIgnoreCase))
                {
                    var qValueString = components[j][2..];
                    if (double.TryParse(qValueString, CultureInfo.InvariantCulture, out var parsedQValue))
                    {
                        qValue = parsedQValue;
                    }
                    break;
                }
            }
            mimeTypeEntries.Add(new MimeTypeEntry(mimeType, qValue, i));
        }

        var sortedMimeTypeEntries = mimeTypeEntries.OrderByDescending(e => e.QValue).ThenBy(e => e.OriginalIndex).Select(e => e.MimeType);

        return new Queue<string>(sortedMimeTypeEntries);
    }
    //holds intermediate mime type entry with q value and original index for sorting
    private sealed record MimeTypeEntry(string MimeType, double QValue, int OriginalIndex);

}