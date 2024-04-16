using System.Text;
using System.Text.Json;
using Confluent.Kafka;

namespace Kanafka.Utilities;

public static class MessageHelper
{
    public static Message<string, string> CreateMessage<TMessage>(TMessage messageBody)
        where TMessage : notnull
    {
        var messageType = messageBody.GetType();
        var jsonMessage = JsonSerializer.Serialize(messageBody, messageType);
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

    public static Message<string, string> CreateMessage(string messageBody)
    {
        var kafkaMessage = new Message<string, string>
        {
            Key = Guid.NewGuid().ToString(),
            Value = messageBody,
            Headers = new Headers()
        };

        return kafkaMessage;
    }

    public static T GetContent<T>(this Message<string, string> message)
        where T : struct
    {
        var messageBody = message.Value;

        var messageObj = JsonSerializer.Deserialize<T>(messageBody);
        return messageObj;
    }

    public static string? GetHeader(this Message<string, string> message, string key)
    {
        return message.Headers.TryGetLastBytes(key, out var header)
            ? Encoding.ASCII.GetString(header)
            : null;
    }

    public static Dictionary<string, string> GetHeaders(this Message<string, string> message)
        => message.Headers.ToDictionary(
            x => x.Key,
            y => Encoding.ASCII.GetString(y.GetValueBytes()));

    public static void AddHeader(this Message<string, string> message, string key, string value)
    {
        var headerValue = Encoding.ASCII.GetBytes(value);
        message.Headers.Add(key, headerValue);
    }
}