using System.Net;
using Luna.Api.Models;

namespace Luna.Api.Services;

public interface ICosmosDbService
{
    Task<IEnumerable<UserCard>> GetUserCardsAsync();

    Task<HttpStatusCode> CreateUserIncomeAsync(Income userIncome);

    Task<HttpStatusCode> CreateUserExpenseAsync(Expense userExpense);

    Task<FootballQuestion> GetFootballQuestionAsync();
}