using System.Text.Json;
using System.Text.Json.Serialization;
using Confluent.Kafka;

namespace Kanafka.Utilities;

public static class MessageFactory
{
    public static Message<string, string> Create<TMessage>(TMessage messageBody)
        where TMessage : notnull
    {
        var messageType = messageBody.GetType();
        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        var jsonMessage = JsonSerializer.Serialize(messageBody, messageType, options);
        var kafkaMessage = new Message<string, string>
        {
            Key = Guid.NewGuid().ToString(),
            Value = jsonMessage,
            Headers = new Headers()
        };

        var className = messageType.Name;
        kafkaMessage.AddHeader("X-Type", className);

        return kafkaMessage;
    }

    public static Message<string, string> Create(string messageBody)
    {
        var kafkaMessage = new Message<string, string>
        {
            Key = Guid.NewGuid().ToString(),
            Value = messageBody,
            Headers = new Headers()
        };

        return kafkaMessage;
    }
}