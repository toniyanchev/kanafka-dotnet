using System.Text;
using System.Text.Json;
using Confluent.Kafka;

namespace Kanafka.Utilities;

public static class MessageExtensions
{
    public static T GetContent<T>(this Message<Guid, string> message)
        where T : struct
    {
        var messageBody = message.Value;

        var messageObj = JsonSerializer.Deserialize<T>(messageBody);
        return messageObj;
    }

    public static string? GetHeader(this Message<Guid, string> message, string key)
    {
        return message.Headers.TryGetLastBytes(key, out var header)
            ? Encoding.ASCII.GetString(header)
            : null;
    }

    public static Dictionary<string, string> GetHeaders(this Message<Guid, string> message)
        => message.Headers.ToDictionary(
            x => x.Key,
            y => Encoding.ASCII.GetString(y.GetValueBytes()));

    public static void AddHeader(this Message<Guid, string> message, string key, string value)
    {
        var headerValue = Encoding.ASCII.GetBytes(value);
        message.Headers.Add(key, headerValue);
    }
}