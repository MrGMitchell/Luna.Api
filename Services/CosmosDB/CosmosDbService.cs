using Luna.Api.Models;
using System.Net;
using Luna.Api.Services.Football;
using Luna.Api.Services.SpendingPlan;

namespace Luna.Api.Services.CosmosDB;

public class CosmosDbService : ICosmosDbService
{
    private readonly ISpendingPlanService _spendingPlanService;
    private readonly IFootballQuestionService _footballQuestionService;
    private readonly ISubscriberService _subscriberService;
    private readonly IAuthenticationService _authenticationService;

    public CosmosDbService(ISpendingPlanService spendingPlanService, IFootballQuestionService footballQuestionService, ISubscriberService subscriberService, IAuthenticationService authenticationService)
    {
        _spendingPlanService = spendingPlanService;
        _footballQuestionService = footballQuestionService;
        _subscriberService = subscriberService;
        _authenticationService = authenticationService;
    }

    public async Task<UserAuthInfo> GetCurrentUserAsync(string userId)
    {
        return await _authenticationService.GetCurrentUserAsync(userId);
    }

    public async Task<List<UserAuthInfo>> GetAllUsersAsync()
    {
        return await _authenticationService.GetAllUsersAsync();
    }

    public async Task<IEnumerable<UserCard>> GetUserCardsAsync()
    {
        return await _spendingPlanService.GetUserCardsAsync();
    }

    public async Task<HttpStatusCode> CreateUserIncomeAsync(Income userIncome)
    {
        return await _spendingPlanService.CreateUserIncomeAsync(userIncome);
    }

    public async Task<HttpStatusCode> UpdateUserIncomeAsync(List<Income> incomes)
    {
        return await _spendingPlanService.UpdateUserIncomeAsync(incomes);
    }

    public async Task<HttpStatusCode> DeleteUserIncomeAsync(List<Income> incomes)
    {
        return await _spendingPlanService.DeleteUserIncomeAsync(incomes);
    }

    public async Task<HttpStatusCode> CreateUserExpenseAsync(Expense userExpense)
    {
        return await _spendingPlanService.CreateUserExpenseAsync(userExpense);
    }

    public async Task<HttpStatusCode> UpdateUserExpenseAsync(List<Expense> expenses)
    {
        return await _spendingPlanService.UpdateUserExpenseAsync(expenses);
    }

    public async Task<HttpStatusCode> DeleteUserExpenseAsync(List<Expense> expenses)
    {
        return await _spendingPlanService.DeleteUserExpenseAsync(expenses);
    }

    public async Task<FootballQuestion> GetDailyFootballQuestionAsync()
    {
        return await _footballQuestionService.GetDailyFootballQuestionAsync();
    }

    public async Task<List<FootballQuestion>> GetQuizFootballQuestionsAsync(int numberOfQuestions)
    {
        return await _footballQuestionService.GetQuizFootballQuestionsAsync(numberOfQuestions);
    }

    public async Task<List<QuizQuestion>> GetQuizQuestionsAsync()
    {
        return await _footballQuestionService.GetQuizQuestionsAsync();
    }

    public async Task<FootballQuestion> GetTodaysFootballQuestionAsync()
    {
        return await _footballQuestionService.GetTodaysFootballQuestionAsync();
    }

    public async Task<List<Subscriber>> GetSubscribersAsync()
    {
        return await _subscriberService.GetSubscribersAsync();
    }

    public async Task<bool> AddSubscriberAsync(Subscriber subscriber)
    {
        return await _subscriberService.AddSubscriberAsync(subscriber);
    }

    public async Task<bool> DeleteSubscriberAsync(string email)
    {
        return await _subscriberService.DeleteSubscriberAsync(email);
    }

    public async Task<HttpStatusCode> UpdateBalancesFromPreviousMonthAsync(string previousPlanId, string targetPlanId)
    {
        return await _spendingPlanService.UpdateBalancesFromPreviousMonthAsync(previousPlanId, targetPlanId);
    }

    public async Task<HttpStatusCode> CreateSpendingPlanTemplateAsync()
    {
        return await _spendingPlanService.CreateSpendingPlanTemplateAsync();
    }

    public async Task<HttpStatusCode> UpdateCurrentBalanceAsync(UserCard userCard)
    {
        return await _spendingPlanService.UpdateCurrentBalanceAsync(userCard);
    }

    public async Task<HttpStatusCode> SaveQuizAnswersAsync(QuizAnswer quizAnswer)
    {
        return await _footballQuestionService.SaveQuizAnswersAsync(quizAnswer);
    }

    public async Task<UserReportSummary> GetUserQuizSummaryAsync(string userId)
    {
        return await _footballQuestionService.GetUserQuizSummaryAsync(userId);
    }

    public async Task<List<QuizAnswer>> GetUserQuizHistoryAsync(string userId)
    {
        return await _footballQuestionService.GetUserQuizHistoryAsync(userId);
    }

    public async Task<HttpStatusCode> AssignRoleToUserAsync(UserAuthInfo userAuthInfo)
    {
        return await _authenticationService.AssignRoleToUserAsync(userAuthInfo);
    }
}