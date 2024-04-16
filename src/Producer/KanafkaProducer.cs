using System.Globalization;
using Confluent.Kafka;
using Kanafka.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Kanafka.Producer;

public class KanafkaProducer(IOptions<Settings> options, IServiceScopeFactory serviceScopeFactory)
{
    private readonly IProducer<string, string> _confluentProducer = CreateProducer(options);

    public async Task SendAsync<TMessage>(string topic, TMessage message, CancellationToken cancellationToken)
        where TMessage : class
    {
        var kafkaMessage = MessageFactory.Create(message);
        await _confluentProducer.ProduceAsync(topic, kafkaMessage, cancellationToken);
    }

    public async Task SendAsync(string topic, string message, CancellationToken cancellationToken)
    {
        var kafkaMessage = MessageFactory.Create(message);
        await _confluentProducer.ProduceAsync(topic, kafkaMessage, cancellationToken);
    }

    public async Task SendAsync(string topic, Message<string, string> message, CancellationToken cancellationToken) =>
        await _confluentProducer.ProduceAsync(topic, message, cancellationToken);

    public async Task SendDelayedAsync<TMessage>(string topic, TMessage message, TimeSpan delayTime,
        CancellationToken cancellationToken)
        where TMessage : class
    {
        var kafkaMessage = MessageFactory.Create(message);

        if (delayTime > TimeSpan.FromSeconds(60))
            await DelayInDatabase(topic, kafkaMessage, DateTime.Now + delayTime, cancellationToken);
        else
            DelayInMemory(topic, kafkaMessage, (int)delayTime.TotalSeconds, cancellationToken);
    }

    public async Task SendDelayedAsync<TMessage>(string topic, TMessage message, DateTime delayTo,
        CancellationToken cancellationToken)
        where TMessage : class
    {
        var kafkaMessage = MessageFactory.Create(message);
        var now = DateTime.Now;
        var delaySeconds = (delayTo - now).TotalSeconds;

        if (delaySeconds > 60)
            await DelayInDatabase(topic, kafkaMessage, delayTo, cancellationToken);
        else
            DelayInMemory(topic, kafkaMessage, (int)delaySeconds, cancellationToken);
    }

    private async Task DelayInDatabase(string topic, Message<string, string> message, DateTime delayTo,
        CancellationToken cancellationToken)
    {
        message.AddHeader("X-Delay-Timestamp", delayTo.ToString(CultureInfo.CurrentCulture));
        message.AddHeader("X-Delay-Topic", topic);

        await _confluentProducer.ProduceAsync("kanafka-delayed-messages", message, cancellationToken);
    }

    private void DelayInMemory(string topic, Message<string, string> message, int forSeconds,
        CancellationToken cancellationToken)
    {
        var taskId = Guid.NewGuid();
        using var scope = serviceScopeFactory.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<Settings>>();
        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(forSeconds), cancellationToken);
                using var producer = CreateProducer(options);
                await producer.ProduceAsync(topic, message, cancellationToken);

                DelayedMessageCacher.DiscardThreadId(taskId);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }, cancellationToken);

        DelayedMessageCacher.CacheThreadId(taskId);
    }
    
    public void Dispose()
    {
        _confluentProducer.Dispose();
    }

    private static IProducer<string, string> CreateProducer(IOptions<Settings> options)
    {
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = options.Value.Url,
            EnableIdempotence = true,
            Acks = Acks.All,
            LingerMs = 50,
            ClientId = "oss",
            // SslCaLocation = options.Value.CaFilePath,
            // SslCertificateLocation = options.Value.CertFilePath,
            // SslKeyLocation = options.Value.KeyFilePath,
            // SecurityProtocol = SecurityProtocol.Ssl
        };
        return new ProducerBuilder<string, string>(producerConfig).Build();
    }
}