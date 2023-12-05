namespace Fetch.Models
{
    public class FetchConfig
    {
        public string Name { get; set; }
        public Uri Url { get; set; }
        public string? Method { get; set; }
        public Dictionary<string, string>? Headers { get; set; }
        public string? Body { get; set; }

    }

    public class RequestTracker
    {
        public Uri Url { get; set; }
        public int Up { get; private set; }
        public int Down { get; private set; }

        public float GetAvailability()
        {
            return 100 * (Up / (Up + Down));
        }

        public RequestTracker AddUp() {
            Up = Up++;
            return this;
        }

        public RequestTracker AddDown() {
            Down = Down++;
            return this;
        }
    }
}