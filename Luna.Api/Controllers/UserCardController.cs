using Microsoft.AspNetCore.Mvc;
using Luna.Api.Services;
using Luna.Api.Models;

namespace Luna.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class UserCardController : ControllerBase
{
    private readonly ILogger<UserCardController> _logger;
    private readonly ICosmosService _cosmosdb;
    private readonly ISqlServerService _sql;

    public UserCardController(ILogger<UserCardController> logger, ICosmosService cosmosdb, ISqlServerService sql)
    {
        _logger = logger;
        _cosmosdb = cosmosdb;
        _sql = sql;
    }

    [HttpGet(Name = "GetUserCards")]
    public IEnumerable<UserCard> Get()
    {
        return _sql.GetUserCards();
    }
}
