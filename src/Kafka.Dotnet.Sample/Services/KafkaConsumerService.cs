using Confluent.Kafka;
using Kafka.Dotnet.Sample.Configurations;
using Kafka.Dotnet.Sample.Models;
using LZ4;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kafka.Dotnet.Sample.Services
{
    public class KafkaConsumerService : IHostedService, IDisposable
    {
        private readonly ILogger<KafkaConsumerService> _logger;
        private readonly KafkaConfiguration _kafkaConfiguration;
        private IConsumer<string, string> _consumer;

        public KafkaConsumerService(ILogger<KafkaConsumerService> logger, IOptions<KafkaConfiguration> kafkaConfigurationOptions)
        {
            _logger = logger ?? throw new ArgumentException(nameof(logger));
            _kafkaConfiguration = kafkaConfigurationOptions?.Value ?? throw new ArgumentException(nameof(kafkaConfigurationOptions));
            
            Init();
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Kafka Consumer Service has started.");

                    _consumer.Subscribe(new List<string>() { _kafkaConfiguration.Topic });

                    await Consume(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Kafka Consumer Service is stopping.");

            _consumer.Close();

            await Task.CompletedTask;
        }

        public void Dispose()
        {
            _consumer.Dispose();
        }

        private void Init()
        {
            //var pemFileWithKey = "./keystore/secure.pem";

            var config = new ConsumerConfig()
            {
                BootstrapServers = _kafkaConfiguration.Brokers,

                //SslCaLocation = pemFileWithKey,
                //SslCertificateLocation = pemFileWithKey,
                //SslKeyLocation = pemFileWithKey,

                //Debug = "broker,topic,msg",

                GroupId = _kafkaConfiguration.ConsumerGroup,
                SecurityProtocol = SecurityProtocol.Plaintext,
                EnableAutoCommit = false,
                StatisticsIntervalMs = 5000,
                SessionTimeoutMs = 6000,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnablePartitionEof = true
            };

            _consumer = new ConsumerBuilder<string, string>(config).SetStatisticsHandler((_, kafkaStatistics) => LogKafkaStats(kafkaStatistics)).
                SetErrorHandler((_, e) => LogKafkaError(e)).Build();
        }

        private void LogKafkaStats(string kafkaStatistics)
        {
            var stats = JsonConvert.DeserializeObject<KafkaStatistics>(kafkaStatistics);

            if (stats?.topics != null && stats.topics.Count > 0)
            {
                foreach (var topic in stats.topics)
                {
                    foreach (var partition in topic.Value.Partitions)
                    {
                        Task.Run(() =>
                        {
                            var logMessage = $"FxRates:KafkaStats Topic: {topic.Key} Partition: {partition.Key} PartitionConsumerLag: {partition.Value.ConsumerLag}";
                            _logger.LogInformation(logMessage);
                        });
                    }
                }
            }
        }

        private void LogKafkaError(Error ex)
        {
            Task.Run(() =>
            {
                var error = $"Kafka Exception: ErrorCode:[{ex.Code}] Reason:[{ex.Reason}] Message:[{ex.ToString()}]";
                _logger.LogError(error);
            });
        }

        private async Task Consume(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(cancellationToken);

                    if (consumeResult?.Message == null) continue;

                    if (consumeResult.Topic.Equals(_kafkaConfiguration.Topic))
                    {
                        await Task.Run(() =>
                        {
                            var json = Encoding.UTF8.GetString(LZ4Codec.Unwrap(Convert.FromBase64String(consumeResult.Message.Value)));
                            _logger.LogInformation($"[{consumeResult.Message.Key}] {consumeResult.Topic} - {json}");
                        }, cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
            }
        }
    }
}