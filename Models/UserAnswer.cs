using Newtonsoft.Json;

namespace Luna.Api.Models;

public class UserAnswer
{
    [JsonProperty(PropertyName = "QuestionId")]
    public string? QuestionId { get; set; }

    [JsonProperty(PropertyName = "Categories")]
    public List<string>? Categories { get; set; }

    [JsonProperty(PropertyName = "RuleNumber")]
    public string? RuleNumber { get; set; }

    [JsonProperty(PropertyName = "SelectedAnswer")]
    public string? SelectedAnswer { get; set; }

    [JsonProperty(PropertyName = "IsCorrect")]
    public bool IsCorrect { get; set; }

    [JsonProperty(PropertyName = "AnsweredAt")]
    public DateTime AnsweredAt { get; set; }
}
