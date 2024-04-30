namespace Kanafka.Storage;

public interface IFailedMessagePersister
{
    Task PersistAsync(FailedMessage failedMessage, CancellationToken cancellationToken);
}