using Luna.Api.Models;
using Luna.Api.Services;
using Azure.Core;
using Azure.Identity;
using Microsoft.Azure.Cosmos;

public class CosmosDbService : ICosmosDbService
{
    public async Task<IEnumerable<UserCard>> GetUserCardsAsync()
    {
        var cards = new List<UserCard>();

        // Credential class for testing on a local machine or Azure services
        TokenCredential credential = new DefaultAzureCredential();

        // New instance of CosmosClient class using a connection string
        using CosmosClient client = new("https://luna-cosmos-db.documents.azure.com:443/", "JtGG09InqTX71w0Wz9AS8VujJI1okCf6cR7tNTM4KYwX679soJNzSdOCSzIV36FwdVtrn9VFsJteACDbA53ZlQ==");

        Container container = client.GetContainer("Luna", "SpendingPlans");

        using FeedIterator<Plan> planfeed = container.GetItemQueryIterator<Plan>(
            queryText: $"SELECT * FROM SpendingPlans p WHERE p.Month = '{DateTime.Now.ToString("MMMM")}' AND p.Year = '{DateTime.Now.Year}'"
        );

        // Iterate query result pages
        while (planfeed.HasMoreResults)
        {
            FeedResponse<Plan> response = await planfeed.ReadNextAsync();

            Plan plan = new();

            plan = response.First<Plan>();

            using FeedIterator<Plan> feed = container.GetItemQueryIterator<Plan>(
                queryText: $"SELECT * FROM SpendingPlans p WHERE p.PlanId = '{plan.PlanId}' AND p.Type IN ('income','expense')"
            );

            // Iterate query result pages
            while (feed.HasMoreResults)
            {
                FeedResponse<Plan> items = await feed.ReadNextAsync();

                var userCards = items
                .GroupBy(g => new
                    {
                        g.User
                    })
                .Select(uc => new UserCard {
                    Name = uc.Key.User,
                    TotalIncome = uc.Where(t => t.Type == "income").Sum(i => int.Parse(i.Amount)).ToString(),
                    ExpensesPaid = uc.Where(t => t.Type == "expense" && t.ExpenseStatus).Sum(i => int.Parse(i.Amount)).ToString(),
                    ExpensesUnpaid = uc.Where(t => t.Type == "expense" && !t.ExpenseStatus).Sum(i => int.Parse(i.Amount)).ToString(),
                });

                foreach(UserCard item in userCards){
                    var card = new UserCard {
                        PlanId = plan.PlanId,
                        Name = item.Name,
                        TotalIncome = item.TotalIncome,
                        ExpensesPaid = item.ExpensesPaid,
                        ExpensesUnpaid = item.ExpensesUnpaid
                    };
                    cards.Add(card);
                }
            }
        }

        return cards;
    }
}