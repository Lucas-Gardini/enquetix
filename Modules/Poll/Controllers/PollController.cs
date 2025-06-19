using enquetix.Modules.Auth.Middlewares;
using enquetix.Modules.Poll.DTOs;
using enquetix.Modules.Poll.Services;
using Microsoft.AspNetCore.Mvc;

namespace enquetix.Modules.Poll.Controllers
{
    [ApiController]
    [AuthorizeSession]
    [Route("polls")]
    public class PollController(IPollService service) : ControllerBase
    {
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var poll = await service.GetPollAsync(id);
            return Ok(poll);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 0, [FromQuery] string? search = null)
        {
            var polls = await service.GetPollsAsync(page, search);
            return Ok(polls);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePollDto dto)
        {
            var created = await service.CreatePollAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [HttpPost("many")]
        public async Task<IActionResult> CreateBulk([FromBody] List<CreatePollDto> dtoList)
        {
            var created = await service.CreatePollsAsync(dtoList);
            return Ok(created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePollDto dto)
        {
            var updated = await service.UpdatePollAsync(id, dto);
            return Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await service.DeletePollAsync(id);
            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteBulk([FromBody] List<Guid> ids)
        {
            await service.DeletePollsAsync(ids);
            return NoContent();
        }
    }
}
