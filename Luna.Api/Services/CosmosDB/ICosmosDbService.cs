using Luna.Api.Models;

namespace Luna.Api.Services;

public interface ICosmosDbService
{
    Task<IEnumerable<UserCard>> GetUserCardsAsync();
}