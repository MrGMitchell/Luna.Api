using Microsoft.AspNetCore.Mvc;
using Luna.Api.Services;
using Luna.Api.Models;

namespace Luna.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class UserCardController : ControllerBase
{
    private readonly ILogger<UserCardController> _logger;
    private readonly ICosmosDbService _cosmosDb;

    public UserCardController(ILogger<UserCardController> logger, ICosmosDbService cosmosDb)
    {
        _logger = logger;
        _cosmosDb = cosmosDb;
    }

    [HttpGet(Name = "GetUserCards")]
    public async Task<IEnumerable<UserCard>> Get()
    {
        return await _cosmosDb.GetUserCardsAsync();
    }
}
