using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RefreshToken.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SecuredController : ControllerBase
    {

        [HttpGet]

        public IActionResult GetData()
        {
            return Ok("Hello from Secured Controller ...");
        }
    }
}
