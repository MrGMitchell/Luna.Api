using Microsoft.AspNetCore.Mvc;
using Luna.Api.Services.CosmosDB;
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

    [HttpGet("GetUserCards")]
    public async Task<IEnumerable<UserCard>> Get()
    {
        return await _cosmosDb.GetUserCardsAsync();
    }

    [HttpPut("UpdateCurrentBalance")]
    public async Task<IActionResult> UpdateCurrentBalanceAsync(UserCard userCard)
    {
        var result = await _cosmosDb.UpdateCurrentBalanceAsync(userCard);
        if (result == System.Net.HttpStatusCode.OK)
        {
            return Ok();
        }
        else if (result == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound();
        }
        else
        {
            return StatusCode((int)result);
        }
    }
}
