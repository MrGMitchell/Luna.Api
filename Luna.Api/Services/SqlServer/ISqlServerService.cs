using Luna.Api.Models;

namespace Luna.Api.Services;

public interface ISqlServerService
{
    public IEnumerable<UserCard> GetUserCards();
}