using System.ComponentModel.DataAnnotations;

namespace RefreshToken.Models
{
    public class RegisterModel
    {
        [Required,StringLength(50)]
        public string FirstName { get; set; }


        [Required, StringLength(50)]
        public string LastName { get; set; }

        [Required,StringLength(250)]
        public string Email { get; set; }

        [Required,StringLength (150)]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
