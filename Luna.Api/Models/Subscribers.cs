using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Luna.Api.Models;

public class Subscriber
{
    [JsonProperty(PropertyName = "id")]
    public string? id { get; set; }

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    [JsonProperty(PropertyName = "Email")]
    public string? Email { get; set; }

    [JsonProperty(PropertyName = "EmailId")]
    public string? EmailId { get; set; }

    [JsonProperty(PropertyName = "SubscriptionDate")]
    public DateTime SubscriptionDate { get; set; }
}