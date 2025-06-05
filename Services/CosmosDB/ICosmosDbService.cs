using System.Net;
using Luna.Api.Models;

namespace Luna.Api.Services;

public interface ICosmosDbService
{
    Task<IEnumerable<UserCard>> GetUserCardsAsync();

    Task<HttpStatusCode> CreateUserIncomeAsync(Income userIncome);

    Task<HttpStatusCode> CreateUserExpenseAsync(Expense userExpense);

    Task<HttpStatusCode> UpdateUserExpenseAsync(List<Expense> Expenses);

    Task<FootballQuestion> GetDailyFootballQuestionAsync();

    Task<List<FootballQuestion>> GetQuizFootballQuestionsAsync(int numberOfQuestions);

    Task<List<QuizQuestion>> GetQuizQuestionsAsync();

    Task<FootballQuestion> GetTodaysFootballQuestionAsync();

    Task<List<Subscriber>> GetSubscribersAsync();

    Task<bool> AddSubscriberAsync(Subscriber subscriber);

    Task<bool> DeleteSubscriberAsync(string email);
}