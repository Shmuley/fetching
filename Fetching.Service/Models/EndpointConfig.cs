namespace Fetching.Service.Models
{
    public class EndpointConfig
    {
        public EndpointConfig()
        {
            // This will set the defualt for the latency timeout in the event it is not supplied in the YAML file
            // Even though this is not part of the YAML spec, it makes it easy to troubleshoot the latency calculations
            LatencyThreshold = new TimeSpan(0, 0, 0, 0, 500).Milliseconds;
        }

        public string Name { get; set; }

        public Uri Url { get; set; }

        public string? Method { get; set; }

        public Dictionary<string, string>? Headers { get; set; }

        public string? Body { get; set; }

        public long LatencyThreshold { get; set; }
    }
}
