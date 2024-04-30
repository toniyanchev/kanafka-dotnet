using System.ComponentModel.DataAnnotations;

namespace Kanafka;

public class KanafkaSettings
{
    [Required(ErrorMessage = "Missing configuration - \"Kanafka.Url\". Must specify kafka server url")]
    public required string Url { get; set; }
}