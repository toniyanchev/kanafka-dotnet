using Microsoft.Extensions.Caching.Memory;

namespace Kanafka.Utilities;

public static class DelayedMessageCacher
{
    private const string CacheKey = "backgroundThreadIds";
    private static readonly IMemoryCache MemoryCache = new MemoryCache(new MemoryCacheOptions());
    private static readonly object Lock = new();

    public static void CacheThreadId(Guid threadId)
    {
        lock (Lock)
        {
            var cachedThreads = GetCachedThreadIds();
            cachedThreads.Add(threadId);
            MemoryCache.Set(CacheKey, cachedThreads);
        }
    }

    public static void DiscardThreadId(Guid threadId)
    {
        lock (Lock)
        {
            var cachedThreads = GetCachedThreadIds();

            cachedThreads.Remove(threadId);
            MemoryCache.Set(CacheKey, cachedThreads);
        }
    }

    private static List<Guid> GetCachedThreadIds()
    {
        lock (Lock)
        {
            var hasThreadIds = MemoryCache.TryGetValue<List<Guid>>(CacheKey, out var threadIds);
            return (hasThreadIds ? threadIds : new List<Guid>()) ?? new List<Guid>();
        }
    }
}