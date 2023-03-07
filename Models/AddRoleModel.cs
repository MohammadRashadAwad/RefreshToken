using System.ComponentModel.DataAnnotations;

namespace RefreshToken.Models
{
    public class AddRoleModel
    {
        [Required]
        public string Userid { get; set; }
        [Required]
        public string RoleName { get; set; }
    }
}
