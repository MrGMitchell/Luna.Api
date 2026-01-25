using Luna.Api.Models;
using Microsoft.Azure.Cosmos;
using System.Net;

namespace Luna.Api.Services.SpendingPlan;

public class SpendingPlanService : ISpendingPlanService
{
    private readonly CosmosClient _cosmosClient;
    private readonly string _lunaDatabaseId;
    private readonly string _lunaContainerId;

    public SpendingPlanService(IConfiguration configuration, CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
        _lunaDatabaseId = configuration["LunaDatabaseId"]!;
        _lunaContainerId = configuration["LunaContainerId"]!;
    }

    public async Task<IEnumerable<UserCard>> GetUserCardsAsync()
    {
        var userCards = new List<UserCard>();

        Container container = _cosmosClient.GetContainer(_lunaDatabaseId, _lunaContainerId);

        using FeedIterator<Plan> planfeed = container.GetItemQueryIterator<Plan>(
            queryText: $"SELECT * FROM SpendingPlans p WHERE p.PlanId >= '{DateTime.Now.ToString("MMyyyy")}' AND p.Type = 'plan' Order By p.Year DESC"
        );

        // Iterate query result pages
        while (planfeed.HasMoreResults)
        {
            FeedResponse<Plan> response = await planfeed.ReadNextAsync();

            List<Plan> plans = new();

            plans = response.ToList<Plan>();

            foreach (var plan in plans)
            {
                using FeedIterator<Plan> feed = container.GetItemQueryIterator<Plan>(
                    queryText: $"SELECT * FROM SpendingPlans p WHERE p.PlanId = '{plan.PlanId}' AND p.Type IN ('income','expense', 'balance')"
                );

                // Iterate query result pages
                while (feed.HasMoreResults)
                {
                    FeedResponse<Plan> items = await feed.ReadNextAsync();

                    List<UserCard> userCardResults = [.. items
                    .GroupBy(g => new
                        {
                            g.User
                        })
                    .Select(uc => new UserCard {
                        id = uc.FirstOrDefault(t => t.Type == "balance")?.id ?? string.Empty,
                        PlanId = plan.PlanId,
                        Name = uc.Key.User,
                        StartDate = DateOnly.Parse($"{plan.Month} 1, {plan.Year}"),
                        EndDate = DateOnly.Parse($"{plan.Month} 1, {plan.Year}").AddMonths(1).AddDays(-1),
                        Month = plan.Month,
                        Year = plan.Year,
                        TotalIncome = uc.Where(t => t.Type == "income").Sum(a => a.Amount),
                        TotalExpenses = uc.Where(t => t.Type == "expense").Sum(a => a.Amount),
                        ExpensesPaid = uc.Where(t => t.Type == "expense" && t.ExpenseStatus).Sum(a => a.Amount),
                        ExpensesUnpaid = uc.Where(t => t.Type == "expense" && !t.ExpenseStatus).Sum(a => a.Amount),
                        CurrentBalance = uc.Where(t => t.Type == "balance").Sum(a => a.Amount),
                        Incomes = [.. uc.Where(t => t.Type == "income")],
                        Expenses = [.. uc.Where(t => t.Type == "expense")]
                    })];

                    userCards.AddRange(userCardResults);
                }
            }

            foreach (UserCard card in userCards)
            {
                card.EndingBalance = card.CurrentBalance - card.ExpensesUnpaid + card.Incomes.Where(d => d.PayDate > DateOnly.FromDateTime(DateTime.Now)).Sum(i => i.Amount ?? 0);
                card.Surplus = card.TotalIncome - (card.ExpensesPaid + card.ExpensesUnpaid);
            }
        }

        userCards = [.. userCards.OrderBy(uc => DateTime.Parse($"{uc.Year}-{uc.Month}-01"))];

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

    public async Task<HttpStatusCode> UpdateUserIncomeAsync(List<Income> incomes)
    {
        Container container = _cosmosClient.GetContainer(_lunaDatabaseId, _lunaContainerId);

        foreach (Income income in incomes)
        {
            ItemResponse<Income> response = await container.PatchItemAsync<Income>(
            id: income.id,
            partitionKey: new PartitionKey(income.PlanId),
            patchOperations: [
                PatchOperation.Replace("/Amount", income.Amount),
                PatchOperation.Replace("/PayDate", income.PayDate)
                ]
            );

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return response.StatusCode;
            }
        }

        return HttpStatusCode.OK;
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
                PatchOperation.Replace("/ExpenseDescription", expense.ExpenseDescription),
                PatchOperation.Replace("/Amount", expense.Amount),
                PatchOperation.Replace("/ExpenseDueDate", expense.ExpenseDueDate),
                PatchOperation.Replace("/ExpenseStatus", expense.ExpenseStatus)
                ]
            );

            DateTime nextPlanId = DateTime.ParseExact(expense.PlanId, "MMyyyy", null).AddMonths(1);

            await UpdateBalancesFromPreviousMonthAsync(expense.PlanId, nextPlanId.ToString("MMyyyy"));

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return response.StatusCode;
            }
        }

        return HttpStatusCode.OK;
    }

    public async Task<HttpStatusCode> CreateSpendingPlanTemplateAsync()
    {
        var now = DateTime.Now;

        Container container = _cosmosClient.GetContainer(_lunaDatabaseId, _lunaContainerId);

        var planId = now.Month.ToString("D2") + now.Year.ToString();

        SpendingPlanTemplate template = new()
        {
            id = Guid.NewGuid().ToString(),
            PlanId = planId,
            Month = now.ToString("MMMM"),
            Year = now.Year.ToString(),
            Type = "plan"
        };

        ItemResponse<SpendingPlanTemplate> planResponse = await container.CreateItemAsync<SpendingPlanTemplate>(template);

        var currMonth = DateTime.Now.ToString("MMMM");
        var currYear = DateTime.Now.Year.ToString();
        var startDate = DateOnly.Parse($"{currMonth} 1, {currYear}");
        var endDate = startDate.AddMonths(1).AddDays(-1);
        var userCards = new List<UserCard>();

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
                    ExpensesUnpaid = uc.Where(t => t.Type == "expense" && !t.ExpenseStatus).Sum(a => a.Amount),
                    CurrentBalance = uc.Where(t => t.Type == "balance").Sum(a => a.Amount),
                    Incomes = [.. uc.Where(t => t.Type == "income")]
                })];

                foreach (UserCard card in userCards)
                {
                    card.EndingBalance = card.CurrentBalance - card.ExpensesUnpaid;

                    if (card.Incomes != null)
                    {
                        foreach (var income in card.Incomes)
                        {
                            if(DateTime.Parse(income.PayDate.ToString()) > DateTime.Today)
                            {
                                card.EndingBalance += income.Amount;
                            }
                        }
                    }
                }
            }
        }

        currMonth = DateTime.Now.AddMonths(1).ToString("MMMM");
        currYear = DateTime.Now.AddMonths(1).Year.ToString();

        using FeedIterator<Plan> balanceUpdateFeed = container.GetItemQueryIterator<Plan>(
            queryText: $"SELECT * FROM SpendingPlans p WHERE p.Month = '{currMonth}' AND p.Year = '{currYear}'"
        );

        while (balanceUpdateFeed.HasMoreResults)
        {
            FeedResponse<Plan> response = await balanceUpdateFeed.ReadNextAsync();
            
            Plan plan = new();

            plan = response.First<Plan>();

            using FeedIterator<Plan> feed = container.GetItemQueryIterator<Plan>(
                queryText: $"SELECT * FROM SpendingPlans p WHERE p.PlanId = '{plan.PlanId}' AND p.Type IN ('balance')"
            );

            // Iterate query result pages
            while (feed.HasMoreResults)
            {
                
            }
            
        }

        using FeedIterator<Plan> templateFeed = container.GetItemQueryIterator<Plan>(
            queryText: $"SELECT * FROM SpendingPlans p WHERE p.PlanId = '{0}' AND p.Type IN ('income','expense', 'balance')"
        );

        var balances = new List<Balance>();
        var incomes = new List<Income>();
        var expenses = new List<Expense>();

        // Iterate query result pages
        while (templateFeed.HasMoreResults)
        {
            FeedResponse<Plan> items = await templateFeed.ReadNextAsync();

            foreach (var item in items)
            {
                if (item.Type == "balance")
                {
                    balances.Add(new Balance
                    {
                        id = Guid.NewGuid().ToString(),
                        PlanId = planId,
                        User = item.User,
                        Type = "balance",
                        Amount = item.Amount
                    });
                }
                else if (item.Type == "income")
                {
                    incomes.Add(new Income
                    {
                        id = Guid.NewGuid().ToString(),
                        PlanId = planId,
                        User = item.User,
                        Type = "income",
                        Amount = item.Amount,
                        PayDate = item.PayDate
                    });
                }
                else if (item.Type == "expense")
                {
                    expenses.Add(new Expense
                    {
                        id = Guid.NewGuid().ToString(),
                        PlanId = planId,
                        User = item.User,
                        Type = "expense",
                        Amount = item.Amount,
                        ExpenseDescription = item.ExpenseDescription,
                        ExpenseDueDate = item.ExpenseDueDate,
                        ExpenseStatus = false
                    });
                }
            }
        }

        // Create a new item
        foreach (var balance in balances)
        {
            ItemResponse<Balance> balanceResponse = await container.CreateItemAsync<Balance>(balance);    
        }

        foreach (var income in incomes)
        {
            ItemResponse<Income> incomeResponse = await container.CreateItemAsync<Income>(income);
        }

        foreach (var expense in expenses)
        {
            ItemResponse<Expense> expenseResponse = await container.CreateItemAsync<Expense>(expense);
        }
        
        return HttpStatusCode.Created;
    }

    public async Task<HttpStatusCode> UpdateBalancesFromPreviousMonthAsync(string previousPlanId, string targetPlanId)
    {
        Container container = _cosmosClient.GetContainer(_lunaDatabaseId, _lunaContainerId);

        var balances = new List<Balance>();
        using FeedIterator<Balance> balanceFeed = container.GetItemQueryIterator<Balance>(
            new QueryDefinition("SELECT * FROM c WHERE c.PlanId = @planId AND c.Type = 'balance'")
                .WithParameter("@planId", previousPlanId)
        );

        while (balanceFeed.HasMoreResults)
        {
            FeedResponse<Balance> resp = await balanceFeed.ReadNextAsync();
            balances.AddRange(resp);
        }

        var expenses = new List<Expense>();
        using FeedIterator<Expense> expenseFeed = container.GetItemQueryIterator<Expense>(
            new QueryDefinition("SELECT * FROM c WHERE c.PlanId = @planId AND c.Type = 'expense'")
                .WithParameter("@planId", previousPlanId)
        );

        while (expenseFeed.HasMoreResults)
        {
            FeedResponse<Expense> resp = await expenseFeed.ReadNextAsync();
            expenses.AddRange(resp);
        }

        var incomes = new List<Income>();
        using FeedIterator<Income> incomeFeed = container.GetItemQueryIterator<Income>(
            new QueryDefinition("SELECT * FROM c WHERE c.PlanId = @planId AND c.Type = 'income'")
                .WithParameter("@planId", targetPlanId)
        );

        while (incomeFeed.HasMoreResults)
        {
            FeedResponse<Income> resp = await incomeFeed.ReadNextAsync();
            incomes.AddRange(resp);
        }

        foreach (var bal in balances)
        {
            decimal userIncomes = incomes.Where(i => i.User == bal.User).Sum(i => i.Amount ?? 0);
            decimal userExpenses = expenses.Where(e => e.User == bal.User && e.ExpenseStatus == false).Sum(e => e.Amount ?? 0);
            decimal currentBalance = bal.Amount ?? 0;
            decimal endingBalance = currentBalance - userExpenses + userIncomes;

            var newBalance = new Balance();
            using FeedIterator<Balance> newBalanceFeed = container.GetItemQueryIterator<Balance>(
                new QueryDefinition("SELECT * FROM c WHERE c.User = @user AND c.PlanId = @targetPlanId AND c.Type = 'balance'")
                .WithParameter("@user", bal.User)
                .WithParameter("@targetPlanId", targetPlanId)
            );

            while (newBalanceFeed.HasMoreResults)
            {
                FeedResponse<Balance> resp = await newBalanceFeed.ReadNextAsync();
                newBalance = resp.FirstOrDefault();
            }

            if (newBalance != null)
            {
                ItemResponse<Balance> response = await container.PatchItemAsync<Balance>(
                    id: newBalance.id,
                    partitionKey: new PartitionKey(targetPlanId),
                    patchOperations: [
                        PatchOperation.Replace("/Amount", endingBalance.ToString())
                        ]
                );

                if (response.StatusCode != HttpStatusCode.Created && response.StatusCode != HttpStatusCode.OK)
                {
                    return response.StatusCode;
                }
            }
        }

        return HttpStatusCode.OK;
    }

    public async Task<HttpStatusCode> DeleteUserIncomeAsync(List<Income> incomes)
    {
        try
        {
            Container container = _cosmosClient.GetContainer(_lunaDatabaseId, _lunaContainerId);

            foreach (var income in incomes)
            {
                await container.DeleteItemAsync<Income>(income.id, new PartitionKey(income.PlanId));
            }
            return HttpStatusCode.OK;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error deleting from Cosmos DB: {ex.Message}");
            return HttpStatusCode.InternalServerError;
        }
    }

    public async Task<HttpStatusCode> DeleteUserExpenseAsync(List<Expense> Expenses)
    {
        try
        {
            Container container = _cosmosClient.GetContainer(_lunaDatabaseId, _lunaContainerId);

            foreach (var expense in Expenses)
            {
                await container.DeleteItemAsync<Expense>(expense.id, new PartitionKey(expense.PlanId));
            }
            return HttpStatusCode.OK;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error deleting from Cosmos DB: {ex.Message}");
            return HttpStatusCode.InternalServerError;
        }
    }

    public async Task<HttpStatusCode> UpdateCurrentBalanceAsync(UserCard userCard)
    {
        Container container = _cosmosClient.GetContainer(_lunaDatabaseId, _lunaContainerId);

        ItemResponse<Expense> response = await container.PatchItemAsync<Expense>(
            id: userCard.id,
            partitionKey: new PartitionKey(userCard.PlanId),
            patchOperations: [
                PatchOperation.Replace("/Amount", userCard.CurrentBalance.ToString())
                ]
        );

        DateTime nextPlanId = DateTime.ParseExact(userCard.PlanId, "MMyyyy", null).AddMonths(1);

        await UpdateBalancesFromPreviousMonthAsync(userCard.PlanId, nextPlanId.ToString("MMyyyy"));

        if (response.StatusCode != HttpStatusCode.OK)
        {
            return response.StatusCode;
        }

        return HttpStatusCode.OK;
    }
}