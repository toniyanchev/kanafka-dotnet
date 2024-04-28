using Microsoft.Extensions.DependencyInjection;

namespace Kanafka.Storage;

public class StorageConfiguration
{
    public IServiceCollection ServiceCollection { get; set; }

    public StorageConfiguration(IServiceCollection serviceCollection)
    {
        ServiceCollection = serviceCollection;
    }
}