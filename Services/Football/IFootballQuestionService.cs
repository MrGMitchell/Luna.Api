using Luna.Api.Models;
using System.Net;

namespace Luna.Api.Services.Football;

public interface IFootballQuestionService
{
    Task<FootballQuestion> GetDailyFootballQuestionAsync();
    Task<List<FootballQuestion>> GetQuizFootballQuestionsAsync(int numberOfQuestions);
    Task<FootballQuestion> GetTodaysFootballQuestionAsync();
    Task<List<QuizQuestion>> GetQuizQuestionsAsync();
    Task<HttpStatusCode> SaveQuizAnswersAsync(QuizAnswer quizAnswer);
    Task<UserReportSummary> GetUserQuizSummaryAsync(string userId);
    Task<List<QuizAnswer>> GetUserQuizHistoryAsync(string userId);
}