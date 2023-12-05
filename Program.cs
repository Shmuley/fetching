using Fetch.Models;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

var file = new StreamReader("./input.yaml").ReadToEnd();

var deserializer = new DeserializerBuilder()
    .WithNamingConvention(LowerCaseNamingConvention.Instance)    
    .Build();

var doc = deserializer.Deserialize<List<FetchConfig>>(file);

System.Console.WriteLine("End");
