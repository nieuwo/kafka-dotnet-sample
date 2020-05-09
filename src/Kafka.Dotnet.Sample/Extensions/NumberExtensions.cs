using System;

namespace Kafka.Dotnet.Sample.Extensions
{
    public static class NumberExtensions
    {
        private static readonly Random random = new Random();

        public static double RandomNumberBetween(double minValue, double maxValue)
        {
            var next = random.NextDouble();

            return minValue + (next * (maxValue - minValue));
        }
    }
}