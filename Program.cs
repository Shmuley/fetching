using System.Text;
using Fetch.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

internal class Program
{
    private static void Main(string[] args)
    {
        var timeout = new TimeSpan(0, 0, 0, 0, 500).TotalMilliseconds;
        var client = new HttpClient();
        var file = new StreamReader("./input.yaml").ReadToEnd();

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(LowerCaseNamingConvention.Instance)
            .Build();

        var endpoints = deserializer.Deserialize<List<FetchConfig>>(file);

        foreach (var endpoint in endpoints)
        {
            var method = !string.IsNullOrEmpty(endpoint.Method) ? new HttpMethod(endpoint.Method) : HttpMethod.Get;
            var req = new HttpRequestMessage(method, endpoint.Url);


            if (endpoint.Headers != null)
            {
                foreach (var header in endpoint.Headers)
                {
                    if (header.Key == "content-type" && !string.IsNullOrEmpty(endpoint.Body))
                    {
                        req.Content = new StringContent(endpoint.Body, Encoding.UTF8, header.Value);
                    }
                    else
                    {
                        req.Headers.Add(header.Key, header.Value);
                    }
                }
            }

            var watch = System.Diagnostics.Stopwatch.StartNew();
            var response = client.Send(req);
            watch.Stop();
            if (response.IsSuccessStatusCode && watch.ElapsedMilliseconds < timeout)
            {
                Console.WriteLine($"UP : {watch.ElapsedMilliseconds}");
            }
            else
            {
                Console.WriteLine($"DOWN : {watch.ElapsedMilliseconds}");
            }
        };
    }
}