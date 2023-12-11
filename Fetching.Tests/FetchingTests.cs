using Fetching.Service.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Fetching.Tests;

public class FetchingUnitTests
{
  const string _config = @"
            - headers:
                user-agent: fetch-synthetic-monitor
                Accept-Encoding: gzip
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
                  Content-Encoding: gzip
                  Content-MD5: Q2hlY2sgSW50ZWdyaXR5IQ==
                  foo: bar
              method: POST
              name: fetch.com some post endpoint
              url: https://fetch.com/some/post/endpoint
            - name: www.fetchrewards.com index page
              url: https://www.fetchrewards.com/ 
            - body: '{""foo"":""bar""}'
              headers:
                  content-type: application/json
                  user-agent: fetch-synthetic-monitor
                  authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6Ikpva
              method: PUT
              name: fetch.com some PUT endpoint
              url: https://fetch.com/some/put/endpoint
            - body: '{""foo"":""bar""}'
              headers:
                  user-agent: fetch-synthetic-monitor
              method: PATCH
              name: fetch.com some patch endpoint
              url: https://fetch.com/some/patch/endpoint
            - method: POST
              headers:
                  content-type: application/json
                  user-agent: fetch-synthetic-monitor
              name: fetch.com some post wo body endpoint
              url: https://fetch.com/some/post/endpoint
            - method: POST
              name: fetch.com some post wo body or headers endpoint
              url: https://fetch.com/some/post/endpoint
            - body: '{""foo"":""bar""}'
              headers:
                  content-type: application/json
                  user-agent: fetch-synthetic-monitor
              name: fetch.com some get w body and content-type endpoint
              url: https://fetch.com/
            - body: '{""foo"":""bar""}'
              method: POST
              name: fetch.com some post wo headers endpoint
              url: https://fetch.com/some/post/endpoint
        ";

  const int _uniqeConfigs = 10;
  const int _uniqeHostnames = 2;
  private readonly IDeserializer _deserializer = new DeserializerBuilder()
      .WithNamingConvention(CamelCaseNamingConvention.Instance)
      .Build();

  [Fact]
  public void ProperlyParse_YAMLFile()
  {
    var parsedConfig = _deserializer.Deserialize<List<EndpointConfig>>(_config);
    var endpoints = EndpointPolling.FromList(parsedConfig);
    var hostnames = HostnamePolling.FromList(endpoints);

    Assert.Equal(_uniqeConfigs, parsedConfig.Count);
    Assert.Equal(_uniqeConfigs, endpoints.Count);
    Assert.Equal(_uniqeHostnames, hostnames.Count);

    // Test that our latency override is in fact working and only applying to the one endpoint
    Assert.True(endpoints.First(e => e.FetchConfig.Name == "fetch.com index page")
    .FetchConfig.LatencyThreshold == 200);

    Assert.True(endpoints.First(e => e.FetchConfig.Name == "fetch.com some post endpoint")
    .FetchConfig.LatencyThreshold == 500);
  }

  [Fact]
  public async void Endpoint_Polling_Is_Functional()
  {
    var parsedConfig = _deserializer.Deserialize<List<EndpointConfig>>(_config);
    var endpoints = EndpointPolling.FromList(parsedConfig);

    using (var client = new HttpClient())
    {
      foreach (var endpoint in endpoints)
      {
        await endpoint.PollEndpointAsync(client);
        Assert.True(endpoint.Up + endpoint.Down == 1);
      }
    }
  }

  [Fact]
  public async void Hostname_Polling_Is_Functional()
  {
    var parsedConfig = _deserializer.Deserialize<List<EndpointConfig>>(_config);
    var endpoints = EndpointPolling.FromList(parsedConfig);
    var hostnames = HostnamePolling.FromList(endpoints);

    using (var client = new HttpClient())
    {
      foreach (var hostname in hostnames)
      {
        await hostname.PollHostnameAsync(client);
      }
    }
  }

  [Fact]
  public void Defaults_Properly_Set()
  {
    var config = new EndpointConfig();

    Assert.Equal((long)500, config.LatencyThreshold);
    Assert.Null(config.Body);
    Assert.Null(config.Headers);
    Assert.Null(config.Method);
  }

  [Fact]
  public void Status_Is_Cumulative()
  {
    var config = new EndpointConfig();
    var endpoint = new EndpointPolling(config);

    Assert.True(double.IsNaN(Math.Round(endpoint.Up / (endpoint.Up + endpoint.Down) * 100)));

    endpoint.AddUp().AddUp().AddDown();
    Assert.Equal(67, Math.Round(endpoint.Up / (endpoint.Up + endpoint.Down) * 100));

    endpoint.AddDown();
    Assert.Equal(50, Math.Round(endpoint.Up / (endpoint.Up + endpoint.Down) * 100));

    endpoint.AddUp().AddUp().AddDown();
    Assert.Equal(57, Math.Round(endpoint.Up / (endpoint.Up + endpoint.Down) * 100));

    endpoint.AddUp().AddUp().AddUp();
    Assert.Equal(70, Math.Round(endpoint.Up / (endpoint.Up + endpoint.Down) * 100));
  }
}
