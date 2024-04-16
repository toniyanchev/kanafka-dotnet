using Confluent.Kafka;

namespace Kanafka.Consumer;

/// <summary>
/// Interface that your consumers must implement.
/// </summary>
public interface IKanafkaConsumer
{
    /// <summary> Method to handle the consummation of a Kafka message.</summary>
    /// <param name="message">Confluent.Kafka message class. Represents message data including: body, headers, id</param>
    /// <returns>Task</returns>
    Task ReceiveAsync(Message<string, string> message);
}