using System.Text.Json.Serialization;

namespace Luna.Api.Models;

public class QuizQuestion
{
    [property: JsonPropertyName("id")]
    public string? id { get; set; }

    [property: JsonPropertyName("QuestionId")]
    public string? QuestionId { get; set; }

    [property: JsonPropertyName("Question")]
    public string? Question { get; set; }

    [property: JsonPropertyName("CorrectAnswer")]
    public string? CorrectAnswer { get; set; }

    [property: JsonPropertyName("Choices")]
    public List<string>? Choices { get; set; }
}