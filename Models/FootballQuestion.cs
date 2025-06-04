namespace Luna.Api.Models;

public class FootballQuestion
{
    public string? id { get; set; }
    public string? QuestionId { get; set; }
    public string? Question { get; set; }
    public string? Ruling { get; set; }
    public DateTime? LastSent { get; set; }
}