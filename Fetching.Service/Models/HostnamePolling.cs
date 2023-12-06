namespace Fetching.Service.Models
{
    public class HostnamePolling
    {
        public HostnamePolling(string hostname, List<EndpointPolling> trackers)
        {
            Hostname = hostname;
            EndpointPollers = trackers;
            Up = 0;
            Down = 0;

        }
        public string Hostname { get; set; }
        public List<EndpointPolling> EndpointPollers { get; set; }

        public double Up { get; set; }
        public double Down { get; set; }

        public static List<HostnamePolling> FromList(List<EndpointPolling> endpointPollers)
        {
            List<HostnamePolling> hostnamePollers = new();
            var hostnames = endpointPollers.GroupBy(t => t.FetchConfig.Url.Host).ToList();

            foreach (var host in hostnames)
            {
                hostnamePollers.Add(new HostnamePolling(host.Key, host.ToList()));
            }

            return hostnamePollers;
        }

        public HostnamePolling AddUp()
        {
            Up++;
            return this;
        }

        public HostnamePolling AddDown()
        {
            Down++;
            return this;
        }

        public async Task PollHostnameAsync(HttpClient client)
        {
            foreach (var tracker in EndpointPollers)
            {
                if (await tracker.PollEndpointAsync(client))
                { 
                    AddUp();
                }
                else
                {
                    AddDown();
                }
            }

            Console.WriteLine(GetAvailability());
        }

        public string GetAvailability()
        {
            return $"{Hostname} has {Math.Round(Up / (Up + Down) * 100)}% availability percentage";
        }
    }
}
