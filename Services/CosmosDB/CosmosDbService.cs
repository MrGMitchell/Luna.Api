using Luna.Api.Models;
using Luna.Api.Services;
using Microsoft.Azure.Cosmos;
using System.Net;

public class CosmosDbService : ICosmosDbService
{
    private readonly CosmosClient _cosmosClient;
    private readonly string _footballQuestionsDatabaseId;
    private readonly string _footballQuestionsContainerId;
    private readonly string _emailsContainerId;
    private readonly string _lunaDatabaseId;
    private readonly string _lunaContainerId;

    public CosmosDbService(IConfiguration configuration, CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
        _footballQuestionsDatabaseId = configuration["FootballQuestionsDatabaseId"]!;
        _footballQuestionsContainerId = configuration["FootballQuestionsContainerId"]!;
        _emailsContainerId = configuration["EmailsContainerId"]!;
        _lunaDatabaseId = configuration["LunaDatabaseId"]!;
        _lunaContainerId = configuration["LunaContainerId"]!;
    }

    public async Task<IEnumerable<UserCard>> GetUserCardsAsync()
    {
        var userCards = new List<UserCard>();
        var currMonth = DateTime.Now.ToString("MMMM");
        var currYear = DateTime.Now.Year.ToString();
        var startDate = DateOnly.Parse($"{currMonth} 1, {currYear}");
        var endDate = startDate.AddMonths(1).AddDays(-1);

        Container container = _cosmosClient.GetContainer(_lunaDatabaseId, _lunaContainerId);

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
        Container container = _cosmosClient.GetContainer(_lunaDatabaseId, _lunaContainerId);

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
        Container container = _cosmosClient.GetContainer(_lunaDatabaseId, _lunaContainerId);

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
        Container container = _cosmosClient.GetContainer(_lunaDatabaseId, _lunaContainerId);

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
        Container container = _cosmosClient.GetContainer(_footballQuestionsDatabaseId, _footballQuestionsContainerId);

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

        await container.UpsertItemAsync<FootballQuestion>(question, new PartitionKey(question.QuestionId));

        return question;
    }

    public async Task<List<FootballQuestion>> GetQuizFootballQuestionsAsync(int numberOfQuestions)
    {
        Container container = _cosmosClient.GetContainer(_footballQuestionsDatabaseId, _footballQuestionsContainerId);

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
        Container container = _cosmosClient.GetContainer(_footballQuestionsDatabaseId, _footballQuestionsContainerId);

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
        Container container = _cosmosClient.GetContainer(_footballQuestionsDatabaseId, _emailsContainerId);

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

    public async Task<bool> AddSubscriberAsync(Subscriber subscriber)
    {
        try
        {
            Database database = _cosmosClient.GetDatabase(_footballQuestionsDatabaseId);
            Container container = database.GetContainer(_emailsContainerId);

            Random rnd = new();
            int r = rnd.Next(short.MaxValue);
            var newSubscriber = new Subscriber { id = Guid.NewGuid().ToString(), EmailId = r.ToString(), Email = subscriber.Email, SubscriptionDate = DateTime.UtcNow };
            var response = await container.CreateItemAsync(newSubscriber);
            return response.StatusCode == HttpStatusCode.Created;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error saving to Cosmos DB: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteSubscriberAsync(string email)
    {
        try
        {
            Database database = _cosmosClient.GetDatabase(_footballQuestionsDatabaseId);
            Container container = database.GetContainer(_emailsContainerId);

            // Query to find the item by email
            var query = new QueryDefinition("SELECT * FROM c WHERE c.Email = @Email")
                .WithParameter("@Email", email);

            var iterator = container.GetItemQueryIterator<Subscriber>(query);

            if (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                if (response.Count == 0)
                {
                    Console.Error.WriteLine($"No subscriber found with email: {email}");
                    return false;
                }
                foreach (var item in response)
                {
                    await container.DeleteItemAsync<Subscriber>(item.id, new PartitionKey(item.EmailId));
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error deleting from Cosmos DB: {ex.Message}");
        }
        return await Task.FromResult(true);
    }
}