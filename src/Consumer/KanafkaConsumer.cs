using Confluent.Kafka;
using Kanafka.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kanafka.Consumer;

internal class KanafkaConsumer<TConsumer> : BackgroundService
    where TConsumer : IKanafkaConsumer
{
    private readonly string _topic;
    private readonly ConsumerConfig _consumerConfig;
    private readonly ILogger<TConsumer> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    internal KanafkaConsumer(
        string topic,
        KanafkaConsumerOptions kanafkaConsumerOptions,
        IOptions<KanafkaSettings> options,
        ILogger<TConsumer> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        var settings = options.Value;
        _topic = topic;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _consumerConfig = new ConsumerConfig
        {
            BootstrapServers = settings.Url,
            // SslCaLocation = settings.CaFilePath,
            // SslCertificateLocation = settings.CertFilePath,
            // SslKeyLocation = settings.KeyFilePath,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            // SecurityProtocol = SecurityProtocol.Ssl,
            GroupId = kanafkaConsumerOptions.GroupName ?? $"{_topic}-consumer-group"
        };
    }

    protected override async Task ExecuteAsync(CancellationToken cancelationToken)
    {
        await Task.Yield();

        var consumer = new ConsumerBuilder<string, string>(_consumerConfig).Build();
        consumer.Subscribe(_topic);

        while (!cancelationToken.IsCancellationRequested)
        {
            ConsumeResult<string, string>? consumeResult = null;

            try
            {
                consumeResult = consumer.Consume(cancelationToken);
                _logger.LogInformation(
                    $"Received message: {consumeResult.Message.Key} from topic {_topic}[{consumeResult.Partition.Value}]");
                _logger.LogDebug(consumeResult.Message.Value);

                using var scope = _serviceScopeFactory.CreateScope();
                var instanceConsumer = scope.ServiceProvider.GetRequiredService<TConsumer>();
                await instanceConsumer.ReceiveAsync(consumeResult.Message);
            }
            catch (Exception e)
            {
                if (cancelationToken.IsCancellationRequested)
                    return;

                _logger.LogError("Error while consuming message");
                if (consumeResult is { Message: not null } and { Topic: not null })
                {
                    var failedMessage = new FailedMessage(consumeResult, e);
                    await FailConsuming(failedMessage, cancelationToken);
                }
                else
                {
                    _logger.LogCritical(e, "Can not log in db unhandled consumer exception without Message or Topic");
                }
            }
        }
    }

    private async Task FailConsuming(FailedMessage failedMessage, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var failedMessageHelper = scope.ServiceProvider.GetRequiredService<IFailedMessagePersister>();
            await failedMessageHelper.PersistAsync(failedMessage, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Can not log unhandled consumer exception in db");
        }
    }
}