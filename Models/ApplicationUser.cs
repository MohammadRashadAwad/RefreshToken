using Microsoft.AspNetCore.Identity;

namespace RefreshToken.Models
{
    public class ApplicationUser:IdentityUser
    {
        public string Firstname { get; set; }
        public string LastName { get; set; }
        public List<RefreshTokens> ? RefreshTokens { get; set; }
    }
}
