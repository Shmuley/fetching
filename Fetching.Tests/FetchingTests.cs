using System.Text;
using Fetching.Service.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Fetching.Tests;

public class Tests
{

    [Fact]
    public void ProperlyParse_YAMLFile()
    {
        var config = @"
            - headers:
                user-agent: fetch-synthetic-monitor
              method: GET
              name: fetch.com index page
              url: https://fetch.com/
              latencyThreshold: 200
            - headers:
                user-agent: fetch-synthetic-monitor
              method: GET
              name: fetch.com careers page
              url: https://fetch.com/careers
            - body: '{""foo"":""bar""}'
              headers:
                  content-type: application/json
                  user-agent: fetch-synthetic-monitor
              method: POST
              name: fetch.com some post endpoint
              url: https://fetch.com/some/post/endpoint
            - name: www.fetchrewards.com index page
              url: https://www.fetchrewards.com/ 
        ";

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        var parsedConfig = deserializer.Deserialize<List<EndpointConfig>>(config);
        var endpoints = EndpointPolling.FromList(parsedConfig);
        var hostnames = HostnamePolling.FromList(endpoints);

        Assert.True(parsedConfig.Count == 4);
        Assert.True(endpoints.Count == 4);
        Assert.True(hostnames.Count == 2);

        // Test that our latency override is in fact working and only applying to the one endpoint
        Assert.True(endpoints.First(e => e.FetchConfig.Name == "fetch.com index page")
        .FetchConfig.LatencyThreshold == 200);

        Assert.True(endpoints.First(e => e.FetchConfig.Name == "fetch.com some post endpoint")
        .FetchConfig.LatencyThreshold == 500);
    }

    [Fact]
    public void Defaults_Properly_Set()
    {
        var config = new EndpointConfig();

        Assert.True(config.LatencyThreshold == (long)500);
        Assert.Null(config.Body);
        Assert.Null(config.Headers);
        Assert.Null(config.Method);
    }

    [Fact]
    public void Math_Is_Right()
    {
        var config = new EndpointConfig();
        var endpoint = new EndpointPolling(config);

        Assert.True(double.IsNaN(Math.Round(endpoint.Up / (endpoint.Up + endpoint.Down) * 100)));

        endpoint.AddUp().AddUp().AddDown();
        Assert.True(Math.Round(endpoint.Up / (endpoint.Up + endpoint.Down) * 100) == 67);

        endpoint.AddDown();
        Assert.True(Math.Round(endpoint.Up / (endpoint.Up + endpoint.Down) * 100) == 50);

        endpoint.AddUp().AddUp().AddDown();
        Assert.True(Math.Round(endpoint.Up / (endpoint.Up + endpoint.Down) * 100) == 57);

        endpoint.AddUp().AddUp().AddUp();
        Assert.True(Math.Round(endpoint.Up / (endpoint.Up + endpoint.Down) * 100) == 70);
    }
}
