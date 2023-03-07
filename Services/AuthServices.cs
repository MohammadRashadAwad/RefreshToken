using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RefreshToken.Helpers;
using RefreshToken.Models;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

namespace RefreshToken.Services
{
    public class AuthServices : IAuthServices
        
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JWT _jwt;
        private List<string> _errors = new List<string>();

        public AuthServices(UserManager<ApplicationUser> userManager, IOptions<JWT> jwt, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _jwt = jwt.Value;
            _roleManager = roleManager;
        }

        public async Task<AuthModel> RegisterAsync(RegisterModel model)
        {
            
            
            if(await _userManager.FindByEmailAsync(model.Email)!= null)
            {
                _errors.Add("Email is alrady exists");
                return new AuthModel() { Message=_errors };
            }

            if(await _userManager.FindByNameAsync(model.UserName) != null)
            {
               _errors.Add("Username is alrady exists");
                return new AuthModel() { Message = _errors };

            }

            var user = new ApplicationUser()
            {
                Firstname = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                UserName = model.UserName

            };

       var result=  await _userManager.CreateAsync(user,model.Password);

            if (!result.Succeeded)
            {
                
                foreach(var error in result.Errors)
                {
                    _errors.Add(error.Description);
                }

                return new AuthModel() { Message = _errors};

            }

           await _userManager.AddToRoleAsync(user, "User");
            var jwtSecurityToken = await CreateJwtToken(user);

            return new AuthModel()
            {
                Username=user.UserName,
                Email=user.Email,
                IsAuthenticated=true, 
                Roles =new List<string>(){ "User"},
                Token=new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                //ExpiresOn=jwtSecurityToken.ValidTo
                
               
                
            };



        }
       

        public async Task<AuthModel>GetTokenAsync(LoginModel model)
        {
           
            var authmodel = new AuthModel();
            var user = new EmailAddressAttribute().IsValid(model.UsernameOrEmail) ?
                await _userManager.FindByEmailAsync(model.UsernameOrEmail) :
                await _userManager.FindByNameAsync(model.UsernameOrEmail);

            if(user == null)
            {
                _errors.Add("Email or Username is incorrect");
                authmodel.Message = _errors;
                return authmodel;

            }
            if(!await _userManager.CheckPasswordAsync(user, model.Password))
            {
                _errors.Add("Password is incorrect");
                authmodel.Message = _errors;
                return authmodel;
            }
            var roles = await _userManager.GetRolesAsync(user);
            var token = await CreateJwtToken(user);
            authmodel.IsAuthenticated = true;
            authmodel.Email = user.Email;
            authmodel.Username = user.UserName;
            authmodel.Roles = roles.ToList();
            authmodel.Token = new JwtSecurityTokenHandler().WriteToken(token);

            // check user have any refresh token active or not
            if (user.RefreshTokens.Any(t => t.IsActive))
            {
                var refreshtoken = user.RefreshTokens.SingleOrDefault(t=>t.IsActive);
                authmodel.RefreshToken = refreshtoken.Token;
                authmodel.RefreshTokenExpireation = refreshtoken.ExpiresOn;
            }
            // create new refresh token
            else
            {
                var newRefreshToken = GenerateRefreshToken();
                authmodel.RefreshToken = newRefreshToken.Token;
                authmodel.RefreshTokenExpireation = newRefreshToken.ExpiresOn;
                // save refresh token in table refresh token
                user.RefreshTokens.Add(newRefreshToken);
                await _userManager.UpdateAsync(user);
            }
            // note refresh token response in cookie 
         

        
            
            return authmodel;

           } 
     
        public async Task<string> AddRolesAsync(AddRoleModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Userid);
            if (user == null)
                return "Invalid User";

            if(!await _roleManager.RoleExistsAsync(model.RoleName))
            {
                return "Invalid Role";
            }
            if(await _userManager.IsInRoleAsync(user, model.RoleName))
            {
                return "User is assigend this role";
            }

            var result = await _userManager.AddToRoleAsync(user, model.RoleName);
            return result.Succeeded ? string.Empty : "Errors";
        }
 
       
        public async Task<AuthModel> RefreshTokenAsync(string token)
        {
            var authModel = new AuthModel();

            // check The user have a refresh token same parameter token 
            var user = await _userManager.Users.SingleOrDefaultAsync(u=>u.RefreshTokens.Any(t=>t.Token == token));
            if( user == null)
            {
                authModel.IsAuthenticated = false;
                _errors.Add("Invalid Token");
                authModel.Message = _errors;
                return authModel;
                
            }
            var refreshtoken = user.RefreshTokens.Single(t=>t.Token == token);
             // check refresh token is active or not
            if(!refreshtoken.IsActive)
            {
                authModel.IsAuthenticated = false;
                _errors.Add("Inactive Token");
                authModel.Message = _errors;
                return authModel;
            }
            // revoked refresh token 
            refreshtoken.RevokedOn = DateTime.UtcNow;
            // generate new refresh token ..
            var newRefreshToken = GenerateRefreshToken();
            user.RefreshTokens.Add(newRefreshToken);
            await _userManager.UpdateAsync(user);

            var newJwtToken = await CreateJwtToken(user);
            authModel.IsAuthenticated = true;
            authModel.Token = new JwtSecurityTokenHandler().WriteToken(newJwtToken);
            authModel.RefreshToken = newRefreshToken.Token;
            authModel.RefreshTokenExpireation = newRefreshToken.ExpiresOn;
            authModel.Username = user.UserName;
            authModel.Email = user.Email;
            var roles = await _userManager.GetRolesAsync(user);
            authModel.Roles = roles.ToList();

      
            return authModel;
        }

        private async Task<JwtSecurityToken> CreateJwtToken(ApplicationUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user); // null
            var roles = await _userManager.GetRolesAsync(user); // User
            var roleClaims = new List<Claim>();  // {roles :user}

            foreach (var role in roles)
                roleClaims.Add(new Claim("roles", role));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName), //username 
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), //ddmsdk-deffe-d1e-
                new Claim(JwtRegisteredClaimNames.Email, user.Email), //email
                new Claim("uid", user.Id)  //user id
            }
            .Union(userClaims)
            .Union(roleClaims);
            // payload username jwt id email user id roles

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_jwt.DurationInMinutes),
                signingCredentials: signingCredentials);

            return jwtSecurityToken;
        }

        
        private RefreshTokens GenerateRefreshToken()
        {
            var randomNunmber = new byte[32];
            using var generator = new RNGCryptoServiceProvider();
            generator.GetBytes(randomNunmber);
            return new RefreshTokens() {
                Token = Convert.ToBase64String(randomNunmber),
                CreatedOn = DateTime.UtcNow,
                ExpiresOn = DateTime.UtcNow.AddDays(10)
            
            };

        }
    }
}
