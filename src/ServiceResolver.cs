using Kanafka.Consumer;
using Kanafka.Producer;
using Kanafka.Storage;
using Kanafka.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kanafka;

public static class ServiceResolver
{
    /// <summary>Must use this method when working with Kafka. It adds its required dependencies.</summary>
    /// <param name="services">Your application's service collection.</param>
    /// <param name="persistenceConfigurationFunc">Persistence configuration for Kanafka.</param>
    public static void AddKanafka(
        this IServiceCollection services,
        Func<StorageConfiguration> persistenceConfigurationFunc)
    {
        services
            .AddOptions<KanafkaSettings>()
            .BindConfiguration("Kanafka")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<IKanafkaProducer, KanafkaProducer>();
        EnableDelayedMessages();

        var persistenceConfiguration = persistenceConfigurationFunc();
        foreach (var service in persistenceConfiguration.ServiceCollection)
            services.Add(service);
    }

    /// <summary>Use this method to register your Kanafka consumers.</summary>
    /// <param name="services">Your application's service collection</param>
    /// <param name="topic">Topic your consumer is subscribing to.</param>
    /// <param name="kanafkaConsumerOptions">Options of your consumer (<see cref="KanafkaConsumerOptions"/>)</param>
    /// <typeparam name="TConsumer"></typeparam>
    public static void AddKanafkaConsumer<TConsumer>(this IServiceCollection services, string topic,
        KanafkaConsumerOptions? kanafkaConsumerOptions = null)
        where TConsumer : class, IKanafkaConsumer
    {
        services.AddScoped<TConsumer>();

        kanafkaConsumerOptions ??= KanafkaConsumerOptions.Default;
        var consumersGroupSize = kanafkaConsumerOptions.GroupSize;

        for (var i = 0; i < consumersGroupSize; i++)
        {
            services.AddSingleton(provider =>
            {
                var options = provider.GetRequiredService<IOptions<KanafkaSettings>>();
                var logger = provider.GetRequiredService<ILogger<TConsumer>>();
                var serviceScopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
                return (IHostedService)new KanafkaConsumer<TConsumer>(topic, kanafkaConsumerOptions, options, logger,
                    serviceScopeFactory);
            });
        }
    }

    /// <summary>
    /// Attaches a handler to application's exit event.
    /// The handler awaits the not produced delayed in memory kafka messages
    /// </summary>
    private static void EnableDelayedMessages()
    {
        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            while (true)
            {
                var backgroundThreadIds = DelayedMessageCacher.GetCachedThreadIds();

                if (backgroundThreadIds.Count == 0)
                    return;

                Console.WriteLine(
                    $"Waiting for delayed kafka messages with threads: {string.Join(',', backgroundThreadIds)}");

                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        };
    }
}