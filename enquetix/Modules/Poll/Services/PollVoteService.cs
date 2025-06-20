using enquetix.Modules.Application;
using enquetix.Modules.Application.EntityFramework;
using enquetix.Modules.Application.Redis;
using enquetix.Modules.Auth.Services;
using enquetix.Modules.Poll.DTOs;
using enquetix.Modules.Poll.Repository;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;

namespace enquetix.Modules.Poll.Services
{
    public class PollVoteQueueManager : IPollVoteQueueManager
    {
        private readonly IRabbitMQService _rabbitMQService;

        public PollVoteQueueManager(IRabbitMQService rabbitMQService, IServiceProvider serviceProvider)
        {
            _rabbitMQService = rabbitMQService;
            _rabbitMQService.Subscribe("votes", async (message) =>
            {
                using var scope = serviceProvider.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IPollVoteService>();
                await service.ProcessMessageAsync(message);
            });
        }

        public async Task SendMessageAsync(CreateUpdatePollVoteDto message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message), "Message cannot be null.");

            var messageBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            await _rabbitMQService.PublishAsync("votes", messageBody);
        }
    }

    public interface IPollVoteQueueManager
    {
        Task SendMessageAsync(CreateUpdatePollVoteDto message);
    }

    // ----------------------------------------------------------------------

    public class PollVoteService(Context context, IPollVoteQueueManager pollVoteQueueManager, IAuthService authService, ICacheService cacheService) : IPollVoteService
    {
        public async Task<bool> SendVote(Guid pollId, CreateUpdatePollVoteInputDto createUpdateVoteInputDto)
        {
            await pollVoteQueueManager.SendMessageAsync(new CreateUpdatePollVoteDto
            {
                PollId = pollId,
                OptionId = createUpdateVoteInputDto.OptionId,
                UserId = authService.GetLoggedUserId()
            });

            return true;
        }

        public async Task<GetPollVoteDto> GetPollVoteAsync(Guid pollId)
        {
            var userId = authService.GetLoggedUserId();
            var cacheKey = $"pollVote:{pollId}:{userId}";

            var pollVote = await cacheService.CacheAsync(cacheKey, async () =>
            {
                var poll = await context.PollVotes
                    .Where(v => v.PollId == pollId && v.UserId == userId)
                    .Include(v => v.Option)
                    .Select(v => new GetPollVoteDto
                    {
                        OptionId = v.OptionId,
                        OptionText = v.Option!.OptionText,
                    })
                    .FirstOrDefaultAsync();

                return poll ?? throw new HttpResponseException
                {
                    Status = 404,
                    Value = new { Message = "Poll not found." }
                };
            }, TimeSpan.FromMinutes(1));

            return pollVote!;
        }

        public async Task<GetPollVotesDto> GetPollVotesAsync(Guid pollId)
        {
            var cacheKey = $"pollVotes:{pollId}";

            if (!await context.Polls.AnyAsync(p => p.Id == pollId))
                throw new HttpResponseException
                {
                    Status = 404,
                    Value = new { Message = "Poll not found." }
                };

            var votes = await cacheService.CacheAsync(cacheKey, async () =>
            {
                var pollVotes = await context.PollVotes
                    .Where(v => v.PollId == pollId)
                    .Include(v => v.Option)
                    .GroupBy(v => new { v.OptionId, v.Option!.OptionText })
                    .Select(g => new GetPollVotesWithQuantityDto
                    {
                        OptionId = g.Key.OptionId,
                        OptionText = g.Key.OptionText,
                        TotalVotes = g.Count()
                    })
                    .ToListAsync();

                return new GetPollVotesDto { Votes = pollVotes };
            }, TimeSpan.FromMinutes(1));

            return votes!;
        }

        public async Task ProcessMessageAsync(byte[] messageBody)
        {
            CreateUpdatePollVoteDto? createVoteDto = JsonConvert.DeserializeObject<CreateUpdatePollVoteDto>(Encoding.UTF8.GetString(messageBody));

            if (createVoteDto == null)
                throw new ArgumentNullException(nameof(createVoteDto), "Message body cannot be deserialized to CreateVoteDto.");

            if (!await context.Polls.AnyAsync(p => p.Id == createVoteDto.PollId))
                throw new KeyNotFoundException($"Poll with ID {createVoteDto.PollId} does not exist.");

            if (createVoteDto.OptionId != null && !await context.PollOptions.AnyAsync(o => o.Id == createVoteDto.OptionId && o.PollId == createVoteDto.PollId))
                throw new KeyNotFoundException($"Option with ID {createVoteDto.OptionId} does not exist for Poll with ID {createVoteDto.PollId}.");

            if (createVoteDto.UserId == Guid.Empty)
                throw new ArgumentException("UserId cannot be empty.", nameof(createVoteDto.UserId));

            if (!await context.Users.AnyAsync(u => u.Id == createVoteDto.UserId))
                throw new KeyNotFoundException($"User with ID {createVoteDto.UserId} does not exist.");

            var existingVote = await context.PollVotes
                .FirstOrDefaultAsync(v => v.PollId == createVoteDto.PollId && v.UserId == createVoteDto.UserId);

            if (existingVote != null)
            {
                if (createVoteDto.OptionId == null)
                {
                    context.PollVotes.Remove(existingVote);
                }
                else
                {
                    existingVote.OptionId = (Guid)createVoteDto.OptionId;
                    context.PollVotes.Update(existingVote);
                }
            }
            else
            {
                if (createVoteDto.OptionId == null)
                    throw new ArgumentNullException(nameof(createVoteDto.OptionId), "OptionId cannot be null when creating a new vote.");

                var vote = new PollVoteModel
                {
                    PollId = createVoteDto.PollId,
                    OptionId = (Guid)createVoteDto.OptionId,
                    UserId = createVoteDto.UserId
                };
                context.PollVotes.Add(vote);
            }

            await context.SaveChangesAsync();
            await cacheService.RemoveAsync($"pollVote:{createVoteDto.PollId}:{createVoteDto.UserId}");
            await cacheService.RemoveAsync($"pollVotes:{createVoteDto.PollId}");
        }
    }

    public interface IPollVoteService
    {
        Task ProcessMessageAsync(byte[] messageBody);
        Task<GetPollVoteDto> GetPollVoteAsync(Guid pollId);
        Task<GetPollVotesDto> GetPollVotesAsync(Guid pollId);
        Task<bool> SendVote(Guid pollId, CreateUpdatePollVoteInputDto createUpdateVoteInputDto);
    }
}
