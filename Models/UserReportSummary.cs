using Newtonsoft.Json;

namespace Luna.Api.Models;

public class UserReportSummary
{
    [JsonProperty(PropertyName = "totalQuizzesTaken")]
    public int TotalQuizzesTaken { get; set; }

    [JsonProperty(PropertyName = "overallAccuracyPercentage")]
    public double OverallAccuracyPercentage { get; set; }

    [JsonProperty(PropertyName = "totalQuestionsAnswered")]
    public int TotalQuestionsAnswered { get; set; }

    [JsonProperty(PropertyName = "totalCorrectAnswers")]
    public int TotalCorrectAnswers { get; set; }

    [JsonProperty(PropertyName = "categoryBreakdown")]
    public List<CategoryStats>? CategoryBreakdown { get; set; }

    [JsonProperty(PropertyName = "recentQuizzes")]
    public List<QuizReport>? RecentQuizzes { get; set; }
}

public class CategoryStats
{
    [JsonProperty(PropertyName = "category")]
    public string? Category { get; set; }

    [JsonProperty(PropertyName = "totalQuestions")]
    public int TotalQuestions { get; set; }

    [JsonProperty(PropertyName = "correctAnswers")]
    public int CorrectAnswers { get; set; }

    [JsonProperty(PropertyName = "accuracyPercentage")]
    public double AccuracyPercentage => TotalQuestions > 0 ? (CorrectAnswers * 100.0) / TotalQuestions : 0;
}

public class QuizReport
{
    [JsonProperty(PropertyName = "quizResultId")]
    public string? QuizResultId { get; set; }

    [JsonProperty(PropertyName = "completedAt")]
    public DateTime CompletedAt { get; set; }

    [JsonProperty(PropertyName = "totalQuestions")]
    public int TotalQuestions { get; set; }

    [JsonProperty(PropertyName = "correctAnswers")]
    public int CorrectAnswers { get; set; }

    [JsonProperty(PropertyName = "accuracyPercentage")]
    public double AccuracyPercentage => TotalQuestions > 0 ? (CorrectAnswers * 100.0) / TotalQuestions : 0;

    [JsonProperty(PropertyName = "categoryStats")]
    public List<CategoryStats>? CategoryStats { get; set; }
}
