using System.Text;
using System.Text.Json;
using Confluent.Kafka;

namespace Kanafka.Utilities;

public static class MessageExtensions
{
    public static T GetContent<T>(this Message<string, string> message)
        where T : new()
    {
        var messageBody = message.Value;
        var messageObj = JsonSerializer.Deserialize<T>(messageBody);

        return messageObj ??
            throw new InvalidOperationException($"Kafka message could not be deserialized to type {typeof(T)}");
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