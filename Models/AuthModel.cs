using System.Text.Json.Serialization;

namespace RefreshToken.Models
{
    public class AuthModel
    {
        public List<string> Message { get; set; }
        public bool IsAuthenticated { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
        public string Token { get; set; }
        //public DateTime ExpiresOn { get; set; }

        // to Ignore prop when you return response use [JsonIgnore]

        [JsonIgnore]
        public string ? RefreshToken { get; set; }

        public DateTime RefreshTokenExpireation { get; set; }
    }
}
