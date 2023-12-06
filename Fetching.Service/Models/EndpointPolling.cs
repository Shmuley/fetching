using System.Text;

namespace Fetching.Service.Models
{
    public class EndpointPolling
    {
        public EndpointPolling(EndpointConfig config)
        {
            FetchConfig = config;
            Up = 0;
            Down = 0;
        }

        public EndpointConfig FetchConfig { get; private set; }

        public double Up { get; private set; }
        public double Down { get; private set; }

        public static List<EndpointPolling> FromList(List<EndpointConfig> configs)
        {
            List<EndpointPolling> trackers = new();
            foreach (var config in configs)
            {
                trackers.Add(new EndpointPolling(config));
            }

            return trackers;
        }

        public async Task<bool> PollEndpointAsync(HttpClient client)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var response = await client.SendAsync(BuildRequestMessage());
            watch.Stop();
            if (response.IsSuccessStatusCode && watch.ElapsedMilliseconds < FetchConfig.LatencyThreshold)
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

        public EndpointPolling AddUp()
        {
            Up++;
            return this;
        }

        public EndpointPolling AddDown()
        {
            Down++;
            return this;
        }
        public string GetAvailability()
        {
            return $"{FetchConfig.Url} has {Math.Round(Up / (Up + Down) * 100)}% availability percentage";
        }
    }
}
