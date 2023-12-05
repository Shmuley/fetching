namespace Fetching.Models
{
    public class FetchingConfig
    {
        public FetchingConfig()
        {
            LatencyTimeout = new TimeSpan(0, 0, 0, 0, 500).Milliseconds;
        }

        public string Name { get; set; }

        public Uri Url { get; set; }

        public string? Method { get; set; }

        public Dictionary<string, string>? Headers { get; set; }

        public string? Body { get; set; }

        public long LatencyTimeout { get; set; }
    }
}
