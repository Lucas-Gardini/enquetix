using enquetix.Modules.Auth.Middlewares;
using enquetix.Modules.Poll.DTOs;
using enquetix.Modules.Poll.Services;
using Microsoft.AspNetCore.Mvc;

namespace enquetix.Modules.Poll.Controllers
{
    [ApiController]
    [AuthorizeSession]
    [Route("polls")]
    public class PollVoteController(IPollVoteService service) : ControllerBase
    {
        [HttpGet("{pollId:guid}/vote")]
        public async Task<IActionResult> Get(Guid pollId)
        {
            var poll = await service.GetPollVoteAsync(pollId);
            return Ok(poll);
        }

        [HttpGet("{pollId:guid}/votes")]
        public async Task<IActionResult> GetVotes(Guid pollId)
        {
            var poll = await service.GetPollVotesAsync(pollId);
            return Ok(poll);
        }

        [HttpPost("{pollId:guid}/vote")]
        public async Task<IActionResult> Post(Guid pollId, [FromBody] CreateUpdatePollVoteInputDto dto)
        {
            var result = await service.SendVote(pollId, dto);
            return Ok(result);
        }

    }
}
