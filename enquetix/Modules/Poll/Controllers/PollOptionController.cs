using enquetix.Modules.Auth.Middlewares;
using enquetix.Modules.Poll.DTOs;
using enquetix.Modules.Poll.Services;
using Microsoft.AspNetCore.Mvc;

namespace enquetix.Modules.Poll.Controllers
{
    [ApiController]
    [AuthorizeSession]
    [Route("polls")]
    public class PollOptionController(IPollOptionService service) : ControllerBase
    {
        [HttpGet("{pollId:guid}/options")]
        public async Task<IActionResult> GetOptionsForPoll(Guid pollId)
        {
            var options = await service.GetPollOptionsAsync(pollId);
            return Ok(options);
        }

        [HttpGet("options/{id:guid}")]
        public async Task<IActionResult> GetOption(Guid id)
        {
            var option = await service.GetPollOptionAsync(id);
            return Ok(option);
        }

        [HttpPost("{pollId:guid}/options")]
        public async Task<IActionResult> CreateOption(Guid pollId, [FromBody] CreatePollOptionDto dto)
        {
            var option = await service.CreatePollOptionAsync(pollId, dto);
            return CreatedAtAction(nameof(GetOption), new { id = option.Id }, option);
        }

        [HttpPost("{pollId:guid}/options/many")]
        public async Task<IActionResult> CreateOptions(Guid pollId, [FromBody] List<CreatePollOptionDto> dtos)
        {
            var options = await service.CreatePollOptionsAsync(pollId, dtos);
            return Ok(options);
        }

        [HttpPut("options/{id:guid}")]
        public async Task<IActionResult> UpdateOption(Guid id, [FromBody] UpdatePollOptionDto dto)
        {
            var option = await service.UpdatePollOptionAsync(id, dto);
            return Ok(option);
        }

        [HttpDelete("options/{id:guid}")]
        public async Task<IActionResult> DeleteOption(Guid id)
        {
            await service.DeletePollOptionAsync(id);
            return NoContent();
        }

        [HttpDelete("options")]
        public async Task<IActionResult> DeleteOptions([FromBody] List<Guid> optionIds)
        {
            await service.DeletePollOptionsAsync(optionIds);
            return NoContent();
        }

        [HttpDelete("{pollId:guid}/options")]
        public async Task<IActionResult> DeleteOptionsForPoll(Guid pollId)
        {
            await service.DeletePollOptionsAsync(pollId);
            return NoContent();
        }
    }
}
