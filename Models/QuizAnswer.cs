using Newtonsoft.Json;

namespace Luna.Api.Models;

public class QuizAnswer
{
    [JsonProperty(PropertyName = "id")]
    public string? id { get; set; }
    [JsonProperty(PropertyName = "userId")]
    public string? UserId { get; set; }

    [JsonProperty(PropertyName = "UserQuizId")]
    public string? UserQuizId { get; set; }

    [JsonProperty(PropertyName = "completedAt")]
    public DateTime CompletedAt { get; set; }

    [JsonProperty(PropertyName = "answers")]
    public List<UserAnswer>? Answers { get; set; }
}
