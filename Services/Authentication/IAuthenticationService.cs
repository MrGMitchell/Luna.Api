using Luna.Api.Models;
using System.Net;

namespace Luna.Api.Services.Football;

public interface IAuthenticationService
{
    Task<HttpStatusCode> AssignRoleToUserAsync(UserAuthInfo userAuthInfo);
    Task<List<UserAuthInfo>> GetAllUsersAsync();
    Task<UserAuthInfo> GetCurrentUserAsync(string userId);
}