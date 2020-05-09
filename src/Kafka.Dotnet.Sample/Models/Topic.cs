using Newtonsoft.Json;
using System.Collections.Generic;

namespace Kafka.Dotnet.Sample.Models
{
    public class TopicData
    {
        [JsonProperty("topic")]
        public string Topic { get; set; }

        [JsonProperty("partitions")]
        public IReadOnlyDictionary<string, PartitionData> Partitions { get; set; }
    }
}