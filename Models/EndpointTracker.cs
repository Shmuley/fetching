using System.Net;
using System.Text;

namespace Fetching.Models
{
    public class EndpointTracker
    {
        public EndpointTracker(FetchingConfig config)
        {
            FetchConfig = config;
            Up = 0;
            Down = 0;
        }

        public FetchingConfig FetchConfig { get; private set; }

        public double Up { get; private set; }
        public double Down { get; private set; }

        public static List<EndpointTracker> FromList(List<FetchingConfig> configs)
        {
            List<EndpointTracker> trackers = new();
            foreach (var config in configs)
            {
                trackers.Add(new EndpointTracker(config));
            }

            return trackers;
        }

        public async Task<bool> TrackEndpointAsync(HttpClient client)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var response = await client.SendAsync(BuildRequestMessage());
            watch.Stop();
            if (response.IsSuccessStatusCode && watch.ElapsedMilliseconds < FetchConfig.LatencyTimeout)
            {
                AddUp();
                return true;

            }
            else
            {
                AddDown();
                return false;
            }
        }

        public HttpRequestMessage BuildRequestMessage()
        {
            // Parse http method, if null then set to GET
            var method = !string.IsNullOrEmpty(FetchConfig.Method) ? new HttpMethod(FetchConfig.Method) : HttpMethod.Get;
            var request = new HttpRequestMessage(method, FetchConfig.Url);

            if (null != FetchConfig.Headers)
            {
                foreach (var header in FetchConfig.Headers)
                {
                    if (header.Key == "content-type" && method == HttpMethod.Post)
                    {
                        request.Content = new StringContent(FetchConfig.Body ?? "{}", Encoding.UTF8, header.Value);
                    }
                    else
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                }
            }

            return request;
        }

        public EndpointTracker AddUp()
        {
            Up++;
            return this;
        }

        public EndpointTracker AddDown()
        {
            Down++;
            return this;
        }
    }
}
