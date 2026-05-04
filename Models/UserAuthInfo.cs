using Newtonsoft.Json;

namespace Luna.Api.Models;
    
    public class UserAuthInfo
    {
        [JsonProperty(PropertyName = "userId")]
        public string? Email { get; set; }
        [JsonProperty(PropertyName = "Role")]
        public string? Role { get; set; }
    }