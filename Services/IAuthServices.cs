using RefreshToken.Models;

namespace RefreshToken.Services
{
    public interface IAuthServices
    {
        Task<AuthModel> RegisterAsync(RegisterModel model);

        Task<AuthModel> GetTokenAsync(LoginModel model);
        Task<string> AddRolesAsync(AddRoleModel model);

        Task<AuthModel> RefreshTokenAsync(string token);
      
    }
}
