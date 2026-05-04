using Luna.Api.Models;
using Microsoft.Azure.Cosmos;
using System.Net;

namespace Luna.Api.Services.Football;

public class AuthenticationService : IAuthenticationService
{
    private readonly CosmosClient _cosmosClient;
    private readonly string _footballQuestionsDatabaseId;
    private readonly string _userRolesContainerId;

    public AuthenticationService(IConfiguration configuration, CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
        _footballQuestionsDatabaseId = configuration["FootballQuestionsDatabaseId"]!;
        _userRolesContainerId = configuration["FootballUserRolesContainerId"]!;
    }

    public async Task<HttpStatusCode> AssignRoleToUserAsync(UserAuthInfo userAuthInfo)
    {
        throw new NotImplementedException();
    }

    public async Task<UserAuthInfo> GetCurrentUserAsync(string userId)
    {
        Container container = _cosmosClient.GetContainer(_footballQuestionsDatabaseId, _userRolesContainerId);

        var queryDefinition = new QueryDefinition(
                "SELECT * FROM c WHERE c.userId = @userId"
            ).WithParameter("@userId", userId);

        using FeedIterator<UserAuthInfo> userfeed = container.GetItemQueryIterator<UserAuthInfo>(
            queryDefinition
        );

        var user = new UserAuthInfo();

        while (userfeed.HasMoreResults)
        {
            FeedResponse<UserAuthInfo> response = await userfeed.ReadNextAsync();

            user = response.First();
        }

        return user;
    }

    public async Task<List<UserAuthInfo>> GetAllUsersAsync()
    {
        Container container = _cosmosClient.GetContainer(_footballQuestionsDatabaseId, _userRolesContainerId);

        using FeedIterator<UserAuthInfo> userfeed = container.GetItemQueryIterator<UserAuthInfo>(
            queryText: $"SELECT * FROM u"
        );

        var users = new List<UserAuthInfo>();

        while (userfeed.HasMoreResults)
        {
            FeedResponse<UserAuthInfo> response = await userfeed.ReadNextAsync();
            users.AddRange([.. response]);
        }

        return users;
    }
}