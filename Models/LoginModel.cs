using System.ComponentModel.DataAnnotations;

namespace RefreshToken.Models
{
    public class LoginModel
    {
        [Required]
        public string UsernameOrEmail { get; set; }
        [Required]
        public string Password { get; set; }

    }
}
