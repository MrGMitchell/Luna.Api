using Luna.Api.Models;
using Luna.Api.Services.CosmosDB;
using Microsoft.AspNetCore.Mvc;

namespace Luna.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly ILogger<AuthenticationController> _logger;
    private readonly ICosmosDbService _cosmosDb;

    public AuthenticationController(ILogger<AuthenticationController> logger, ICosmosDbService cosmosDb)
    {
        _logger = logger;
        _cosmosDb = cosmosDb;
    }

    [HttpGet("GetCurrentUser")]
    public async Task<UserAuthInfo> GetCurrentUser(string userId)
    {
        return await _cosmosDb.GetCurrentUserAsync(userId);
    }

    [HttpGet("GetAllUsers")]
    public async Task<List<UserAuthInfo>> GetAllUsers()
    {
        return await _cosmosDb.GetAllUsersAsync();
    }

    [HttpPost("AssignRoleToUser")]
    public async Task<IActionResult> AssignRoleToUserAsync([FromBody] UserAuthInfo userAuthInfo)
    {
        if (userAuthInfo == null || string.IsNullOrWhiteSpace(userAuthInfo.Email))
        {
            return BadRequest("Invalid user data.");
        }

        await _cosmosDb.AssignRoleToUserAsync(userAuthInfo);
        
        return Ok();
    }
}