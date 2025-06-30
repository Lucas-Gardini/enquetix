using enquetix.Modules.User.DTOs;
using enquetix.Modules.User.Services;
using Microsoft.AspNetCore.Mvc;

namespace enquetix.Modules.User.Controllers
{
    [ApiController]
    [Route("users")]
    public class UserController(IUserService service) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserDto request)
        {
            var user = await service.CreateAsync(request);
            return CreatedAtAction(nameof(Create), new { id = user.Id }, user);
        }
    }
}
