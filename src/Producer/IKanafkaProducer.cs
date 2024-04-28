using Confluent.Kafka;

namespace Kanafka.Producer;

/// <summary>
/// Factory from where can be produced kafka messages.
/// </summary>
public interface IKanafkaProducer : IDisposable
{
    /// <summary>Produces a message to a Kafka topic.</summary>
    /// <param name="topic">Name of the topic to which the message is produced.</param>
    /// <param name="message">Object of type: TMessage which is serialized to a JSON string and send as a message body.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <typeparam name="TMessage">Type of the message's body.</typeparam>
    Task SendAsync<TMessage>(string topic, TMessage message, CancellationToken cancellationToken)
        where TMessage : class;

    /// <summary>Produces a message to a Kafka topic.</summary>
    /// <param name="topic">Name of the topic to which the message is produced.</param>
    /// <param name="message">Message body as a string JSON.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendAsync(string topic, string message, CancellationToken cancellationToken);

    /// <summary>Produces a message to a Kafka topic.</summary>
    /// <param name="topic">Name of the topic to which the message is produced.</param>
    /// <param name="message">Kafka Message object.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendAsync(string topic, Message<Guid, string> message, CancellationToken cancellationToken);

    /// <summary>Produces a message to a Kafka topic.</summary>
    /// <param name="topic">Name of the topic to which the message is produced.</param>
    /// <param name="message">Object of type: TMessage which is serialized to a JSON string and send as a message body.</param>
    /// <param name="delayTime">The time of the message's delay.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <typeparam name="TMessage">Type of the message's body.</typeparam>
    Task SendDelayedAsync<TMessage>(string topic, TMessage message, TimeSpan delayTime,
        CancellationToken cancellationToken)
        where TMessage : class;

    /// <summary>Produces a message to a Kafka topic.</summary>
    /// <param name="topic">Name of the topic to which the message is produced.</param>
    /// <param name="message">Object of type: TMessage which is serialized to a JSON string and send as a message body.</param>
    /// <param name="delayTo">The time of the message's producing.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <typeparam name="TMessage">Type of the message's body.</typeparam>
    Task SendDelayedAsync<TMessage>(string topic, TMessage message, DateTime delayTo,
        CancellationToken cancellationToken)
        where TMessage : class;
}