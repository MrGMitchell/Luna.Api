using Luna.Api.Models;
using Microsoft.Azure.Cosmos;

namespace Luna.Api.Services.Football;

public class SubscriberService : ISubscriberService
{
    private readonly CosmosClient _cosmosClient;
    private readonly string _footballQuestionsDatabaseId;
    private readonly string _emailsContainerId;

    public SubscriberService(IConfiguration configuration, CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
        _footballQuestionsDatabaseId = configuration["FootballQuestionsDatabaseId"]!;
        _emailsContainerId = configuration["EmailsContainerId"]!;
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
            return response.StatusCode == System.Net.HttpStatusCode.Created;
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