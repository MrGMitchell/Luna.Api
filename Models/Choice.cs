using System.Text.Json.Serialization;

namespace Luna.Api.Models;
public class Choice
{
    [JsonPropertyName("choice1")]
    public string? Choice1 { get; set; }

    [JsonPropertyName("choice2")]
    public string? Choice2 { get; set; }

    [JsonPropertyName("choice3")]
    public string? Choice3 { get; set; }

    [JsonPropertyName("choice4")]
    public string? Choice4 { get; set; }
}