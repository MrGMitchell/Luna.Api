using System.Net;
using Luna.Api.Models;

namespace Luna.Api.Services.CosmosDB;

public interface ICosmosDbService
{
    Task<IEnumerable<UserCard>> GetUserCardsAsync();

    Task<HttpStatusCode> CreateUserIncomeAsync(Income userIncome);

    Task<HttpStatusCode> UpdateUserIncomeAsync(List<Income> incomes);

    Task<HttpStatusCode> DeleteUserIncomeAsync(List<Income> incomes);

    Task<HttpStatusCode> CreateUserExpenseAsync(Expense userExpense);

    Task<HttpStatusCode> UpdateUserExpenseAsync(List<Expense> Expenses);

    Task<HttpStatusCode> DeleteUserExpenseAsync(List<Expense> Expenses);

    Task<FootballQuestion> GetDailyFootballQuestionAsync();

    Task<List<FootballQuestion>> GetQuizFootballQuestionsAsync(int numberOfQuestions);

    Task<List<QuizQuestion>> GetQuizQuestionsAsync();

    Task<FootballQuestion> GetTodaysFootballQuestionAsync();

    Task<List<Subscriber>> GetSubscribersAsync();

    Task<bool> AddSubscriberAsync(Subscriber subscriber);

    Task<bool> DeleteSubscriberAsync(string email);

    Task<HttpStatusCode> UpdateBalancesFromPreviousMonthAsync(string previousPlanId, string targetPlanId);

    Task<HttpStatusCode>CreateSpendingPlanTemplateAsync();

    Task<HttpStatusCode> UpdateCurrentBalanceAsync(UserCard userCard);
}