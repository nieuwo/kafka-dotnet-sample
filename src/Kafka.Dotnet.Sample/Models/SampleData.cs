using Kafka.Dotnet.Sample.Extensions;
using Newtonsoft.Json;
using System;

namespace Kafka.Dotnet.Sample.Models
{
    public class SampleData
    {
        public Guid Id { get; set; }
        public double Value { get; set; }
        public DateTime Created { get; set; }

        public SampleData()
        {
            Id = Guid.NewGuid();
            Value = NumberExtensions.RandomNumberBetween(0.00, 999999.999999);
            Created = DateTime.UtcNow;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }
    }
}