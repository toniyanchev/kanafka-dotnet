using System.ComponentModel.DataAnnotations;

namespace Kanafka;

public class Settings
{
    [Required]
    public required string Url { get; set; }
}