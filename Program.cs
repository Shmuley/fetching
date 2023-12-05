using Fetching.Models;
using Fetching.Parsers;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

internal class Program
{
    private async static Task Main(string[] args)
    {
        int pollInterval = 15;

        var filePath = CliInputParser.ParseInputArgs(args);
        var file = new StreamReader(filePath).ReadToEnd();

        var pollIntervalSpan = new TimeSpan(0, 0, pollInterval);
        var client = new HttpClient();

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        List<EndpointTracker> trackers = EndpointTracker.FromList(
            deserializer.Deserialize<List<FetchingConfig>>(file));
        
        List<HostnameTracker> hostnameTrackers = HostnameTracker.FromList(trackers);

        while (true)
        {
            foreach( var hostnameTracker in hostnameTrackers)
            {
                await hostnameTracker.TrackHostnameAsync(client);
            };
            Thread.Sleep(pollIntervalSpan);
        }
    }
}
