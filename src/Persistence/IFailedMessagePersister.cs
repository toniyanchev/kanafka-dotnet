namespace Kanafka.Persistence;

public interface IFailedMessagePersister
{
    Task<bool> PersistAsync(FailedMessage failedMessage);
}