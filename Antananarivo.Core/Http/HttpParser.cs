namespace Antananarivo.Core.Http;

public sealed class HttpParser
{
    public HttpRequest Parse(string rawRequest)
    {
        var lines = rawRequest.Split(
            "\r\n",
            StringSplitOptions.None);

        if (lines.Length == 0 || string.IsNullOrWhiteSpace(lines[0]))
        {
            throw new FormatException("Invalid HTTP request.");
        }

        var requestLine = lines[0].Split(' ');

        if (requestLine.Length != 3)
        {
            throw new FormatException("Invalid HTTP request line.");
        }

        var rawTarget = requestLine[1];
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
            if (string.IsNullOrWhiteSpace(lines[i]))
            {
                headerEndIndex = i;
                break;
            }
        }

        string body = string.Empty;
        if (headerEndIndex >= 0 && headerEndIndex + 1 < lines.Length)
        {
            body = string.Join("\r\n", lines[(headerEndIndex + 1)..]);
        }

        var request = new HttpRequest
        {
            Method = requestLine[0],
            Path = path,
            QueryString = queryString,
            Version = requestLine[2],
            Body = body
        };

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
            {
                break;
            }

            var separatorIndex = lines[i].IndexOf(':');

            if (separatorIndex <= 0)
            {
                continue;
            }

            var name = lines[i][..separatorIndex].Trim();
            var value = lines[i][(separatorIndex + 1)..].Trim();

            request.Headers[name] = value;
        }

        return request;
    }
}