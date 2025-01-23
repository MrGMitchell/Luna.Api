using Luna.Api.Models;
using Luna.Api.Services;
using Azure.Core;
using Azure.Identity;
using Microsoft.Azure.Cosmos;

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

        Container container = client.GetContainer("Luna", "SpendingPlans");

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
                    StartDate = startDate.ToString(),
                    EndDate = endDate.ToString(),
                    TotalIncome = uc.Where(t => t.Type == "income").Sum(a => int.Parse(a.Amount)).ToString(),
                    ExpensesPaid = uc.Where(t => t.Type == "expense" && t.ExpenseStatus).Sum(a => int.Parse(a.Amount)).ToString(),
                    ExpensesUnpaid = uc.Where(t => t.Type == "expense" && !t.ExpenseStatus).Sum(a => int.Parse(a.Amount)).ToString(),
                    CurrentBalance = uc.Where(t => t.Type == "balance").Sum(a => int.Parse(a.Amount)).ToString(),
                    Incomes = [.. uc.Where(t => t.Type == "income")],
                    Expenses = [.. uc.Where(t => t.Type == "expense")]
                })];

                foreach(UserCard card in userCards){                    
                     card.EndingBalance = (int.Parse(card.CurrentBalance) - int.Parse(card.ExpensesUnpaid)).ToString();
                     card.Surplus = (int.Parse(card.TotalIncome) - (int.Parse(card.ExpensesPaid) + int.Parse(card.ExpensesUnpaid))).ToString();
                }
            }
        }

        return userCards;
    }
}