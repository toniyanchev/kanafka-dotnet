namespace Kanafka.Consumer;

/// <summary>
/// Use this class when you want to specify certain behavior of your consumer(s). 
/// </summary>
public class KanafkaConsumerOptions
{
    /// <summary>
    /// Specify the name of the group of consumer(s) that will be created.
    /// You need this setting when different consumers will consume from one topic.
    /// So each group will be identified by the broker with its name.
    /// </summary>
    public string? GroupName { get; private set; }

    /// <summary>
    /// Specify how many consumers will be working in parallel (useful ONLY when having many partitions in the topic you are subscribing for).
    /// </summary>
    public int GroupSize { get; private set; } = 1;

    /// <summary>Property is used to create a instance of ConsumerOptions with default values.</summary>
    /// <returns><see cref="KanafkaConsumerOptions"/></returns>
    public static KanafkaConsumerOptions Default => new();

    /// <summary><inheritdoc cref="GroupName"/></summary>
    /// <param name="groupName">This value is set to <see cref="GroupName"/></param>
    /// <returns><see cref="KanafkaConsumerOptions"/></returns>
    public KanafkaConsumerOptions WithGroupName(string groupName)
    {
        GroupName = groupName;
        return this;
    }

    /// <summary><inheritdoc cref="GroupSize"/></summary>
    /// <param name="partitions">This value is set to <see cref="GroupSize"/></param>
    /// <returns><see cref="KanafkaConsumerOptions"/></returns>
    public KanafkaConsumerOptions WithPartitions(int partitions)
    {
        GroupSize = partitions;
        return this;
    }
}