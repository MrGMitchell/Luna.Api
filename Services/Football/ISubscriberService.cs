using Luna.Api.Models;

namespace Luna.Api.Services.Football;

public interface ISubscriberService
{
    Task<List<Subscriber>> GetSubscribersAsync();
    Task<bool> AddSubscriberAsync(Subscriber subscriber);
    Task<bool> DeleteSubscriberAsync(string email);
}