using System.Text.Json;
using Confluent.Kafka;

namespace Kanafka.Utilities;

public static class MessageFactory
{
    public static Message<Guid, string> Create<TMessage>(TMessage messageBody)
        where TMessage : notnull
    {
        var messageType = messageBody.GetType();
        var jsonMessage = JsonSerializer.Serialize(messageBody, messageType);
        var kafkaMessage = new Message<Guid, string>
        {
            Key = Guid.NewGuid(),
            Value = jsonMessage,
            Headers = new Headers()
        };

        var className = messageType.Name;
        kafkaMessage.AddHeader("X-Type", className);

        return kafkaMessage;
    }

    public static Message<Guid, string> Create(string messageBody)
    {
        var kafkaMessage = new Message<Guid, string>
        {
            Key = Guid.NewGuid(),
            Value = messageBody,
            Headers = new Headers()
        };

        return kafkaMessage;
    }
}