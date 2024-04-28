using System.ComponentModel.DataAnnotations;

namespace Kanafka;

public class KanafkaSettings
{
    [Required]
    public required string Url { get; set; }
}