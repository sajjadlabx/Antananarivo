namespace Antananarivo.Core.Http;

public sealed class HttpParser
{
    public HttpRequest Parse(string rawRequest)
    {
        if (string.IsNullOrEmpty(rawRequest))
        {
            throw new FormatException("Invalid HTTP request.");
        }

        var lines = rawRequest.Split(
            "\r\n",
            StringSplitOptions.None);

        if (lines.Length == 0 || string.IsNullOrWhiteSpace(lines[0]))
        {
            throw new FormatException("Invalid HTTP request.");
        }

        var requestLineParts = lines[0].Split(' ');

        if (requestLineParts.Length != 3)
        {
            throw new FormatException("Invalid HTTP request line.");
        }

        var rawMethod = requestLineParts[0];
        var rawTarget = requestLineParts[1];
        var version = requestLineParts[2];

        // Parse method using implicit conversion - throws FormatException for unsupported methods
        var method = (HttpMethod)rawMethod;

        string path;
        string queryString = string.Empty;

        var queryIndex = rawTarget.IndexOf('?');
        if (queryIndex >= 0)
        {
            path = rawTarget[..queryIndex];
            queryString = rawTarget[queryIndex..];
        }
        else
        {
            path = rawTarget;
        }

        var headerEndIndex = -1;
        for (int i = 1; i < lines.Length; i++)
        {
            if (lines[i].Length == 0)
            {
                headerEndIndex = i;
                break;
            }
        }

        int headerLimit = headerEndIndex >= 0 ? headerEndIndex : lines.Length;

        string body = string.Empty;
        if (headerEndIndex >= 0 && headerEndIndex + 1 < lines.Length)
        {
            var bodyLines = lines[(headerEndIndex + 1)..];
            int end = bodyLines.Length;
            while (end > 0 && bodyLines[end - 1].Length == 0)
            {
                end--;
            }
            body = string.Join("\r\n", bodyLines[..end]);
        }

        var request = new HttpRequest
        {
            Method = method,
            Path = path,
            QueryString = queryString,
            Version = version,
            Body = body
        };

        for (int i = 1; i < headerLimit; i++)
        {
            var separatorIndex = lines[i].IndexOf(':');

            if (separatorIndex <= 0)
            {
                continue;
            }

            var name = lines[i][..separatorIndex].Trim();
            var value = lines[i][(separatorIndex + 1)..].Trim();

            if (name.Length > 0)
            {
                request.Headers[name] = value;
            }
        }

        return request;
    }
}
