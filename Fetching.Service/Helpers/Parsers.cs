using System.Text;
using Fetching.Service.Models;

namespace Fetching.Service.Parsers
{
    public static class FetchingParsers
    {
        public static string ParseInputArgs(string[] args)
        {
            if (args.Length != 1)
            {
                throw new NotSupportedException("This application requires exactly one argument in the form of a file path.");
            }

            try
            {
                return Path.GetFullPath(args[0]);
            }
            catch (Exception excep)
            {
                throw new FileNotFoundException($"Could not find valid file at path {args[0]}", excep);
            }
        }

        public static HttpRequestMessage ParseHttpHeaders(HttpRequestMessage request, EndpointConfig config)
        {
            var headers = config.Headers;

            if (null != headers)
            {
                if (headers.TryGetValue("content-type", out string? contentTypeValue))
                {
                    request.Content = new StringContent(config.Body ?? "",
                    Encoding.UTF8, contentTypeValue);
                    headers.Remove("content-type");
                }
                else
                {
                    request.Content = new StringContent(config.Body ?? "", Encoding.UTF8);
                }

                foreach (var header in headers)
                {
                    try
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                    catch (Exception)
                    {
                        try
                        {
                            request.Content.Headers.Add(header.Key, header.Value);
                        }
                        catch (InvalidOperationException ex)
                        {
                            throw new Exception($"Header {header.Key} could not be parsed with value of ${header.Value}", ex);
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                }
            }

            return request;
        }
    }
}
