using enquetix.Modules.Auth.Middlewares;
using enquetix.Modules.Auth.Services;
using Microsoft.AspNetCore.Mvc;

namespace enquetix.Modules.Auth.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] string email, [FromForm] string password)
        {
            var user = await authService.ValidateUserAsync(email, password);

            HttpContext.Session.SetString(SessionKeys.UserId, user.Id.ToString());
            HttpContext.Session.SetString(SessionKeys.Username, user.Username);

            await authService.SaveAccessLogLogin();

            return Ok(new { message = "Logged in", user = new { user.Id, user.Username, user.Email } });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await authService.SaveAccessLogLogout();

            HttpContext.Session.Clear();
            return Ok(new { message = "Logged out" });
        }

        [HttpGet("me")]
        [AuthorizeSession]
        public IActionResult Me()
        {
            var id = HttpContext.Session.GetString(SessionKeys.UserId);
            var username = HttpContext.Session.GetString(SessionKeys.Username);

            return Ok(new { id, username });
        }
    }
}
