using System.Text.Json;
using Confluent.Kafka;
using Kanafka.Utilities;

namespace Kanafka.Storage;

public class FailedMessage
{
    public FailedMessage(ConsumeResult<string, string> consumeResult, Exception ex)
    {
        Topic = consumeResult.Topic;
        FailedOn = DateTime.UtcNow;
        MessageId = new Guid(consumeResult.Message.Key);
        MessageBody = consumeResult.Message.Value;
        messageHeaders = consumeResult.Message.GetHeaders();
        ExceptionType = ex.GetType().FullName;
        ExceptionMessage = ex.Message;
        ExceptionStackTrace = ex.StackTrace;
        InnerExceptionType = ex.InnerException?.GetType().FullName;
        InnerExceptionMessage = ex.InnerException?.Message;
        if (int.TryParse(consumeResult.Message.GetHeader("X-Retries"), out var retries))
            Retries = retries;
    }

    public DateTime FailedOn { get; set; }

    public string Topic { get; set; }

    public Guid MessageId { get; set; }

    public string MessageBody { get; set; }

    private Dictionary<string, string> messageHeaders { get; }
    public string? MessageHeaders => messageHeaders.Any() ? JsonSerializer.Serialize(messageHeaders) : null;

    public string? ExceptionType { get; set; }

    public string? ExceptionMessage { get; set; }

    public string? ExceptionStackTrace { get; set; }
    
    public string? InnerExceptionType { get; set; }

    public string? InnerExceptionMessage { get; set; }

    public int Retries { get; set; }
}