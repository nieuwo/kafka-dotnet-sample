using Confluent.Kafka;
using Kafka.Dotnet.Sample.Configurations;
using Kafka.Dotnet.Sample.Models;
using LZ4;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kafka.Dotnet.Sample.Services
{
    public class KafkaProducerService : IHostedService, IDisposable
    {
        private IProducer<string, string> _producer;
        private readonly ILogger<KafkaProducerService> _logger;
        private readonly KafkaConfiguration _kafkaConfiguration;

        public KafkaProducerService(ILogger<KafkaProducerService> logger, IOptions<KafkaConfiguration> kafkaConfigurationOptions)
        {
            _logger = logger ?? throw new ArgumentException(nameof(logger));
            _kafkaConfiguration = kafkaConfigurationOptions?.Value ?? throw new ArgumentException(nameof(kafkaConfigurationOptions));

            Init();
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Kafka Producer Service has started.");
                    await Produce(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Kafka Producer Service is stopping.");

            _producer.Flush(cancellationToken);

            await Task.CompletedTask;
        }

        public void Dispose()
        {
            _producer.Dispose();
        }

        private void Init()
        {
            //var pemFileWithKey = "./keystore/secure.pem";

            var config = new ProducerConfig()
            {
                BootstrapServers = _kafkaConfiguration.Brokers,
                ClientId = "Kafka.Dotnet.Sample",

                //SslCaLocation = pemFileWithKey,
                //SslCertificateLocation = pemFileWithKey,
                //SslKeyLocation = pemFileWithKey,

                //Debug = "broker,topic,msg",

                SecurityProtocol = SecurityProtocol.Plaintext,
                EnableDeliveryReports = false,
                QueueBufferingMaxMessages = 10000000,
                QueueBufferingMaxKbytes = 100000000,
                BatchNumMessages = 500,
                Acks = Acks.None,
                DeliveryReportFields = "none"
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        private async Task Produce(CancellationToken cancellationToken)
        {
            try
            {
                using (_logger.BeginScope("Kafka App Produce Sample Data"))
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        for (int i = 0; i < 1001; i++)
                        {
                            var json = new SampleData().ToString();

                            var msg = new Message<string, string>
                            {
                                Key = _kafkaConfiguration.Key,
                                Value = Convert.ToBase64String(LZ4Codec.Wrap(Encoding.UTF8.GetBytes(json)))
                            };
                            await _producer.ProduceAsync(_kafkaConfiguration.Topic, msg).ConfigureAwait(false);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, exception.Message);
            }
        }
    }
}