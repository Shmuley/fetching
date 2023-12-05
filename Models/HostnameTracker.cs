namespace Fetching.Models
{
    public class HostnameTracker
    {
        public HostnameTracker(string hostname, List<EndpointTracker> trackers)
        {
            Hostname = hostname;
            Trackers = trackers;
            Up = 0;
            Down = 0;

        }
        public string Hostname { get; set; }
        public List<EndpointTracker> Trackers { get; set; }

        public double Up { get; set; }
        public double Down { get; set; }

        public static List<HostnameTracker> FromList(List<EndpointTracker> trackers)
        {
            List<HostnameTracker> hostnameTrackers = new();
            var hostnames = trackers.GroupBy(t => t.FetchConfig.Url.Host).ToList();

            foreach (var host in hostnames)
            {
                hostnameTrackers.Add(new HostnameTracker(host.Key, host.ToList()));
            }

            return hostnameTrackers;
        }

        public HostnameTracker AddUp()
        {
            Up++;
            return this;
        }

        public HostnameTracker AddDown()
        {
            Down++;
            return this;
        }

        public async Task TrackHostnameAsync(HttpClient client)
        {
            foreach (var tracker in Trackers)
            {
                if (await tracker.TrackEndpointAsync(client))
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
