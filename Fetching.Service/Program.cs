using Fetching.Service.Models;
using Fetching.Service.Parsers;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

internal class Program
{
    private async static Task Main(string[] args)
    {
        int pollInterval = 15;
        var pollIntervalSpan = new TimeSpan(0, 0, pollInterval);

        var filePath = FetchingParsers.ParseInputArgs(args);
        var file = new StreamReader(filePath).ReadToEnd();

        var client = new HttpClient();

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        List<EndpointPolling> endpointPolling = EndpointPolling.FromList(
            deserializer.Deserialize<List<EndpointConfig>>(file));
        
        List<HostnamePolling> hostnamePolling = HostnamePolling.FromList(endpointPolling);

        while (true)
        {
            foreach( var hostnamePoll in hostnamePolling)
            {
                await hostnamePoll.PollHostnameAsync(client);
            };
            Thread.Sleep(pollIntervalSpan);
        }
    }
}
