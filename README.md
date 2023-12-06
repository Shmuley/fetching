## Building and Running

You can do this one of two ways:

### Install Dotnet SDK

All of the step here take place in the root of the repository unless otherwise stated

- Install the SDK
  - You will need to install .NET 8 SDK. Microsoft has a decent guide for installing on any OS, you can find that [here](https://dotnet.microsoft.com/en-us/download)
- Build the application
  - In the root of the repo, run ```dotnet build fetching.csproj -c Release -o build```
- Run the application
  - You will need to pass the application a valid YAML file as so ```dotnet ./build/fetching.dll <your-yaml-file>```

### Using Docker

If you prefer to use docker instead of installing the dotnet SDK, you can follow these instructions. Again, unless othertherwise stated, the following steps should be run in the root of the repo

- Install Docker
  - Honestly, if you don't already have Docker installed I'd recommend following the dotnet SDK based instructions above, but if you wish to proceed, instructions for installing can be followed [here](https://docs.docker.com/engine/install/)
- Build the container
  - ```docker build -t fetching .```
- Run the container
  - You will need to mount your yaml file in the docker container for this to work, you can do that using the following command: ```docker run -v <full-path-to-yaml-file>:/app/input.yaml --rm fetching:latest input.yaml```

## Additional Features

### Latency Threshold
During testing I would often wonder if my availability calculation was actually working, so I added a property for adjusting latency in the ```Models/EndpointConfig.cs``` called ```LatencyThreshold```. This made it possible to change the latency threshold for each individual endpoint by adding the equivalent property to the yaml file, ```latencyThreshold```, like so:
```yaml
- headers:
    user-agent: fetch-synthetic-monitor
  method: GET
  name: fetch.com index page
  url: https://fetch.com/
  # The sweetspot for this endpoint seems to be around 200
  # You get a good mix of UPs and DOWN here, any lower or higher tends to make things one sided
  latencyThreshold: 200
  (...)
```

### Endpoint Tracking
This was honestley a mistake, I misunderstood the instructions at first and was tracking each FQDN instead of aggregating by hostname, so I built in UP and DOWN tracking for each ```EndpointPolling``` object. This means if you wanted to extend this app to track the avaialbility of individual endpoints, the data is already there, and can be accessed like so
```csharp
// Program.cs

while (true)
{
    foreach( var hostnamePoll in hostnamePolling)
    {
        await hostnamePoll.PollHostnameAsync(client);

        foreach (var endpointPoll in hostnamePoll.EndpointPollers){
            Console.WriteLine(endpointPoll.GetAvailability());
        }
    };
    Thread.Sleep(pollIntervalSpan);
}
```
In the example above we are adding to the while loop in the main method to print the endpoint availability. This also turned out to be useful with troubleshooting individual endpoints.