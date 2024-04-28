namespace Kanafka.Storage;

public interface IFailedMessagePersister
{
    Task<bool> PersistAsync(FailedMessage failedMessage);
}