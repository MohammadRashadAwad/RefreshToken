using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RefreshToken.Models;
using RefreshToken.Services;

namespace RefreshToken.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthServices _authServices;
        public AuthController( IAuthServices authServices)
        {
            _authServices = authServices;
        }

        [HttpPost("register")]

        public async Task<IActionResult> RegisterAsync ([FromBody]RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authServices.RegisterAsync(model);
            if (!result.IsAuthenticated)
                return BadRequest(result.Message);

            SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpireation);
            return Ok(result);
        }

        [HttpPost("login")]

        public async Task<IActionResult> GetTokenAsunc([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authServices.GetTokenAsync(model);
            if (!result.IsAuthenticated)
                return BadRequest(result.Message);


            if (!string.IsNullOrEmpty(result.RefreshToken))
            {
                SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpireation);
            }  

            return Ok(result);
        }

        [HttpGet("refreshtoken")]

        public async Task<IActionResult> RefreshTokenAsync()
        {
            var refreshtoken = Request.Cookies["refreshtoken"];
            var result = await _authServices.RefreshTokenAsync(refreshtoken);
            if (!result.IsAuthenticated)
            {
                return BadRequest(new
                {
                    Message =result.Message,
                    IsAuthenticated =result.IsAuthenticated
                });
            }
            SetRefreshTokenInCookie(result.RefreshToken,result.RefreshTokenExpireation);
            return Ok(result);
        }

        [HttpPost("addrole")]
        public async Task<IActionResult> AddRoleAsync([FromBody] AddRoleModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authServices.AddRolesAsync(model);
            if (!string.IsNullOrEmpty(result))
                return BadRequest(result);

           
            return Ok(model);
        }

        private void SetRefreshTokenInCookie(string refreshtoken,DateTime expired)
        {
            var cookieOption = new CookieOptions( )
            {
                HttpOnly = true,
                Expires= expired.ToLocalTime(),
               
            };
            Response.Cookies.Append("refreshtoken",refreshtoken,cookieOption);
        }
       
    }
}
