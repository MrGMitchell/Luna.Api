using Luna.Api.Models;
using Luna.Api.Services;
using Azure.Core;
using Azure.Identity;
using Microsoft.Azure.Cosmos;
using System.Net;

public class CosmosDbService : ICosmosDbService
{
    public async Task<IEnumerable<UserCard>> GetUserCardsAsync()
    {
        var userCards = new List<UserCard>();
        var currMonth = DateTime.Now.ToString("MMMM");
        var currYear = DateTime.Now.Year.ToString();
        var startDate = DateOnly.Parse($"{currMonth} 1, {currYear}");
        var endDate = startDate.AddMonths(1).AddDays(-1);

        // Credential class for testing on a local machine or Azure services
        TokenCredential credential = new DefaultAzureCredential();

        // New instance of CosmosClient class using a connection string
        using CosmosClient client = new("https://luna-cosmos-db.documents.azure.com:443/", "JtGG09InqTX71w0Wz9AS8VujJI1okCf6cR7tNTM4KYwX679soJNzSdOCSzIV36FwdVtrn9VFsJteACDbA53ZlQ==");

        Container container = client.GetContainer("Luna", "SpendingPlan");

        using FeedIterator<Plan> planfeed = container.GetItemQueryIterator<Plan>(
            queryText: $"SELECT * FROM SpendingPlans p WHERE p.Month = '{currMonth}' AND p.Year = '{currYear}'"
        );

        // Iterate query result pages
        while (planfeed.HasMoreResults)
        {
            FeedResponse<Plan> response = await planfeed.ReadNextAsync();

            Plan plan = new();

            plan = response.First<Plan>();

            using FeedIterator<Plan> feed = container.GetItemQueryIterator<Plan>(
                queryText: $"SELECT * FROM SpendingPlans p WHERE p.PlanId = '{plan.PlanId}' AND p.Type IN ('income','expense', 'balance')"
            );

            // Iterate query result pages
            while (feed.HasMoreResults)
            {
                FeedResponse<Plan> items = await feed.ReadNextAsync();

                userCards = [.. items
                .GroupBy(g => new
                    {
                        g.User
                    })
                .Select(uc => new UserCard {
                    PlanId = plan.PlanId,
                    Name = uc.Key.User,
                    StartDate = startDate,
                    EndDate = endDate,
                    Month = currMonth,
                    Year = currYear,
                    TotalIncome = uc.Where(t => t.Type == "income").Sum(a => a.Amount),
                    TotalExpenses = uc.Where(t => t.Type == "expense").Sum(a => a.Amount),
                    ExpensesPaid = uc.Where(t => t.Type == "expense" && t.ExpenseStatus).Sum(a => a.Amount),
                    ExpensesUnpaid = uc.Where(t => t.Type == "expense" && !t.ExpenseStatus).Sum(a => a.Amount),
                    CurrentBalance = uc.Where(t => t.Type == "balance").Sum(a => a.Amount),
                    Incomes = [.. uc.Where(t => t.Type == "income")],
                    Expenses = [.. uc.Where(t => t.Type == "expense")]
                })];

                foreach (UserCard card in userCards)
                {
                    card.EndingBalance = card.CurrentBalance - card.ExpensesUnpaid;
                    card.Surplus = card.TotalIncome - (card.ExpensesPaid + card.ExpensesUnpaid);
                }
            }
        }

        return userCards;
    }

    public async Task<HttpStatusCode> CreateUserIncomeAsync(Income userIncome)
    {
        // Credential class for testing on a local machine or Azure services
        TokenCredential credential = new DefaultAzureCredential();

        // New instance of CosmosClient class using a connection string
        using CosmosClient client = new("https://luna-cosmos-db.documents.azure.com:443/", "JtGG09InqTX71w0Wz9AS8VujJI1okCf6cR7tNTM4KYwX679soJNzSdOCSzIV36FwdVtrn9VFsJteACDbA53ZlQ==");

        Container container = client.GetContainer("Luna", "SpendingPlan");

        // Create a new item
        Income income = new()
        {
            id = Guid.NewGuid().ToString(),
            PlanId = userIncome.PlanId,
            User = userIncome.User,
            Type = "income",
            Amount = userIncome.Amount,
            PayDate = userIncome.PayDate
        };

        // Create a new item
        ItemResponse<Income> response = await container.CreateItemAsync<Income>(income);

        return response.StatusCode;
    }

    public async Task<HttpStatusCode> CreateUserExpenseAsync(Expense userExpense)
    {
        // Credential class for testing on a local machine or Azure services
        TokenCredential credential = new DefaultAzureCredential();

        // New instance of CosmosClient class using a connection string
        using CosmosClient client = new("https://luna-cosmos-db.documents.azure.com:443/", "JtGG09InqTX71w0Wz9AS8VujJI1okCf6cR7tNTM4KYwX679soJNzSdOCSzIV36FwdVtrn9VFsJteACDbA53ZlQ==");

        Container container = client.GetContainer("Luna", "SpendingPlan");

        // Create a new item
        Expense expense = new()
        {
            id = Guid.NewGuid().ToString(),
            PlanId = userExpense.PlanId,
            User = userExpense.User,
            Type = "expense",
            Amount = userExpense.Amount,
            ExpenseDescription = userExpense.ExpenseDescription,
            ExpenseDueDate = userExpense.ExpenseDueDate,
            ExpenseStatus = false,
        };

        // Create a new item
        ItemResponse<Expense> response = await container.CreateItemAsync<Expense>(expense);

        return response.StatusCode;
    }

    public async Task<HttpStatusCode> UpdateUserExpenseAsync(List<Expense> expenses)
    {
        // Credential class for testing on a local machine or Azure services
        TokenCredential credential = new DefaultAzureCredential();

        // New instance of CosmosClient class using a connection string
        using CosmosClient client = new("https://luna-cosmos-db.documents.azure.com:443/", "JtGG09InqTX71w0Wz9AS8VujJI1okCf6cR7tNTM4KYwX679soJNzSdOCSzIV36FwdVtrn9VFsJteACDbA53ZlQ==");

        Container container = client.GetContainer("Luna", "SpendingPlan");

        foreach (Expense expense in expenses)
        {
            ItemResponse<Expense> response = await container.PatchItemAsync<Expense>(
            id: expense.id,
            partitionKey: new PartitionKey(expense.PlanId),
            patchOperations: [
                PatchOperation.Replace("/ExpenseStatus", 1)
                ]
            );

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return response.StatusCode;
            }
        }

        return HttpStatusCode.OK;
    }

    public async Task<FootballQuestion> GetDailyFootballQuestionAsync()
    {
        // Credential class for testing on a local machine or Azure services
        TokenCredential credential = new DefaultAzureCredential();

        // New instance of CosmosClient class using a connection string
        using CosmosClient client = new("https://luna-cosmos-db.documents.azure.com:443/", "JtGG09InqTX71w0Wz9AS8VujJI1okCf6cR7tNTM4KYwX679soJNzSdOCSzIV36FwdVtrn9VFsJteACDbA53ZlQ==");

        Container container = client.GetContainer("FootballQuestions", "Questions");

        using FeedIterator<FootballQuestion> questionfeed = container.GetItemQueryIterator<FootballQuestion>(
            queryText: $"SELECT * FROM Questions q WHERE q.LastSent < DateTimeAdd(\"dd\",-30,GetCurrentDateTime())"
        );

        var question = new FootballQuestion();

        while (questionfeed.HasMoreResults)
        {
            FeedResponse<FootballQuestion> response = await questionfeed.ReadNextAsync();

            Random rnd = new Random();

            int r = rnd.Next(response.Count);

            question = response.ToList<FootballQuestion>()[r];
        }

        question.LastSent = DateTime.Now;

        await container.UpsertItemAsync<FootballQuestion>(question);

        return question;
    }

    public async Task<List<FootballQuestion>> GetQuizFootballQuestionsAsync(int numberOfQuestions)
    {
        // New instance of CosmosClient class using a connection string
        using CosmosClient client = new("https://luna-cosmos-db.documents.azure.com:443/", "JtGG09InqTX71w0Wz9AS8VujJI1okCf6cR7tNTM4KYwX679soJNzSdOCSzIV36FwdVtrn9VFsJteACDbA53ZlQ==");

        Container container = client.GetContainer("FootballQuestions", "Questions");

        using FeedIterator<FootballQuestion> questionfeed = container.GetItemQueryIterator<FootballQuestion>(
            queryText: $"SELECT * FROM Questions q"
        );

        var questions = new List<FootballQuestion>();

        while (questionfeed.HasMoreResults)
        {
            FeedResponse<FootballQuestion> response = await questionfeed.ReadNextAsync();

            Random rnd = new Random();

            while (numberOfQuestions-- > 0)
            {
                questions.Add(response.ToList<FootballQuestion>()[rnd.Next(response.Count)]);
            }
        }

        return questions;
    }

    public async Task<FootballQuestion> GetTodaysFootballQuestionAsync()
    {
        // Credential class for testing on a local machine or Azure services
        TokenCredential credential = new DefaultAzureCredential();

        // New instance of CosmosClient class using a connection string
        using CosmosClient client = new("https://luna-cosmos-db.documents.azure.com:443/", "JtGG09InqTX71w0Wz9AS8VujJI1okCf6cR7tNTM4KYwX679soJNzSdOCSzIV36FwdVtrn9VFsJteACDbA53ZlQ==");

        Container container = client.GetContainer("FootballQuestions", "Questions");

        using FeedIterator<FootballQuestion> questionfeed = container.GetItemQueryIterator<FootballQuestion>(
            queryText: $"SELECT Top 1 * FROM c Order By c.LastSent DESC"
        );

        var question = new FootballQuestion();

        while (questionfeed.HasMoreResults)
        {
            FeedResponse<FootballQuestion> response = await questionfeed.ReadNextAsync();

            question = response.First<FootballQuestion>();
        }

        return question;
    }

    public async Task<List<Subscriber>> GetSubscribersAsync()
    { 
        // New instance of CosmosClient class using a connection string
        using CosmosClient client = new("https://luna-cosmos-db.documents.azure.com:443/", "JtGG09InqTX71w0Wz9AS8VujJI1okCf6cR7tNTM4KYwX679soJNzSdOCSzIV36FwdVtrn9VFsJteACDbA53ZlQ==");

        Container container = client.GetContainer("FootballQuestions", "Emails");

        using FeedIterator<Subscriber> subscriberfeed = container.GetItemQueryIterator<Subscriber>(
            queryText: $"SELECT * FROM Questions s"
        );

        var subscribers = new List<Subscriber>();

        while (subscriberfeed.HasMoreResults)
        {
            FeedResponse<Subscriber> response = await subscriberfeed.ReadNextAsync();
            subscribers.AddRange([.. response]);
        }

        return subscribers;
    }
}