namespace Fetch.Models
{
    public class FetchConfig
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string Method { get; set; }
        public Dictionary<string, string>? Headers { get; set; }
        public string? Body { get; set; }

    }
}