using System.Net;
using Luna.Api.Models;

namespace Luna.Api.Services.SpendingPlan;

public interface ISpendingPlanService
{
    Task<IEnumerable<UserCard>> GetUserCardsAsync();
    Task<HttpStatusCode> CreateUserIncomeAsync(Income userIncome);
    Task<HttpStatusCode> UpdateUserIncomeAsync(List<Income> incomes);
    Task<HttpStatusCode> DeleteUserIncomeAsync(List<Income> incomes);
    Task<HttpStatusCode> CreateUserExpenseAsync(Expense userExpense);
    Task<HttpStatusCode> UpdateUserExpenseAsync(List<Expense> expenses);
    Task<HttpStatusCode> DeleteUserExpenseAsync(List<Expense> expenses);
    Task<HttpStatusCode> CreateSpendingPlanTemplateAsync();
    Task<HttpStatusCode> UpdateBalancesFromPreviousMonthAsync(string previousPlanId, string targetPlanId);
    Task<HttpStatusCode> UpdateCurrentBalanceAsync(UserCard userCard);
}