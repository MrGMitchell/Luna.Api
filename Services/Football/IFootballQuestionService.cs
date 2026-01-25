using Luna.Api.Models;

namespace Luna.Api.Services.Football;

public interface IFootballQuestionService
{
    Task<FootballQuestion> GetDailyFootballQuestionAsync();
    Task<List<FootballQuestion>> GetQuizFootballQuestionsAsync(int numberOfQuestions);
    Task<FootballQuestion> GetTodaysFootballQuestionAsync();
    Task<List<QuizQuestion>> GetQuizQuestionsAsync();
}