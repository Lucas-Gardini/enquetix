using enquetix.Modules.Application;
using enquetix.Modules.Application.EntityFramework;
using enquetix.Modules.Application.Redis;
using enquetix.Modules.Auth.Services;
using enquetix.Modules.Poll.DTOs;
using enquetix.Modules.Poll.Repository;
using Microsoft.EntityFrameworkCore;

namespace enquetix.Modules.Poll.Services
{
    public class PollService(Context context, IAuthService authService, ICacheService cacheService) : IPollService
    {
        public async Task<PollModel> GetPollAsync(Guid id)
        {
            var result = await cacheService.CacheAsync($"poll:{id}", async () =>
            {
                return await context.Polls.FindAsync(id) ?? throw new HttpResponseException { Status = 404, Value = new { Message = "Poll not found." } };
            }, TimeSpan.FromMinutes(1));

            return result!;
        }

        public async Task<List<PollModel>> GetPollsAsync(int startPage = 0, string? search = null)
        {
            const int pageSize = 20;
            var userId = authService.GetLoggedUserId();

            var result = await cacheService.CacheAsync($"polls:{userId}:page:{startPage}:search:{search}", async () =>
            {
                var query = context.Polls.AsNoTracking().Where(p => p.CreatedBy == userId);
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(p => p.Title.Contains(search) || p.Description.Contains(search));
                }
                return await query
                    .OrderByDescending(p => p.StartDate)
                    .Skip(startPage * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            });

            return result!;
        }

        public async Task<PollModel> CreatePollAsync(CreatePollDto poll)
        {
            if (poll.StartDate < DateTime.UtcNow || poll.EndDate < DateTime.UtcNow)
                throw new HttpResponseException { Status = 400, Value = new { Message = "Invalid Date." } };
            else if (poll.StartDate >= poll.EndDate)
                throw new HttpResponseException { Status = 400, Value = new { Message = "Start date must be before end date." } };

            var newPoll = new PollModel
            {
                Title = poll.Title,
                Description = poll.Description,
                StartDate = poll.StartDate,
                EndDate = poll.EndDate,
                CreatedBy = authService.GetLoggedUserId()
            };

            context.Polls.Add(newPoll);
            await context.SaveChangesAsync();
            await cacheService.RemoveByPartialNameAsync($"polls:{newPoll.CreatedBy}");
            return newPoll;
        }

        public async Task<List<PollModel>> CreatePollsAsync(List<CreatePollDto> polls)
        {
            var userId = authService.GetLoggedUserId();
            var now = DateTime.UtcNow;

            var newPolls = polls.Select(poll =>
            {
                if (poll.StartDate < now || poll.EndDate < now)
                    throw new HttpResponseException { Status = 400, Value = new { Message = "Invalid Date in batch." } };
                else if (poll.StartDate >= poll.EndDate)
                    throw new HttpResponseException { Status = 400, Value = new { Message = "Start date must be before end date in batch." } };

                return new PollModel
                {
                    Title = poll.Title,
                    Description = poll.Description,
                    StartDate = poll.StartDate,
                    EndDate = poll.EndDate,
                    CreatedBy = userId
                };
            }).ToList();

            context.Polls.AddRange(newPolls);
            await context.SaveChangesAsync();
            await cacheService.RemoveByPartialNameAsync($"polls:{userId}");
            return newPolls;
        }

        public async Task<PollModel> UpdatePollAsync(Guid id, UpdatePollDto poll)
        {
            var existingPoll = await GetPollAsync(id);

            if (existingPoll.CreatedBy != authService.GetLoggedUserId())
            {
                throw new HttpResponseException { Status = 403, Value = new { Message = "You do not have permission to update this poll." } };
            }

            existingPoll.Title = poll.Title ?? existingPoll.Title;
            existingPoll.Description = poll.Description ?? existingPoll.Description;
            existingPoll.StartDate = poll.StartDate ?? existingPoll.StartDate;
            existingPoll.EndDate = poll.EndDate ?? existingPoll.EndDate;

            context.Polls.Update(existingPoll);
            await context.SaveChangesAsync();
            await cacheService.RemoveAsync($"poll:{id}");
            await cacheService.RemoveByPartialNameAsync($"polls:{existingPoll.CreatedBy}");
            return existingPoll;
        }

        public async Task DeletePollAsync(Guid id)
        {
            var existingPoll = await GetPollAsync(id);

            if (existingPoll.CreatedBy != authService.GetLoggedUserId())
            {
                throw new HttpResponseException { Status = 403, Value = new { Message = "You do not have permission to delete this poll." } };
            }

            context.Polls.Remove(existingPoll);
            await context.SaveChangesAsync();
            await cacheService.RemoveAsync($"poll:{id}");
            await cacheService.RemoveByPartialNameAsync($"polls:{existingPoll.CreatedBy}");
        }

        public async Task DeletePollsAsync(List<Guid> pollIds)
        {
            var userId = authService.GetLoggedUserId();

            var polls = await context.Polls
                .Where(p => pollIds.Contains(p.Id) && p.CreatedBy == userId)
                .ToListAsync();

            if (polls.Count == 0)
            {
                throw new HttpResponseException { Status = 404, Value = new { Message = "No polls found to delete." } };
            }

            context.Polls.RemoveRange(polls);
            await context.SaveChangesAsync();
            await cacheService.RemoveByPartialNameAsync($"polls:{userId}");
            await cacheService.RemoveAsync([.. polls.Select(p => $"poll:{p.Id}")]);
        }
    }

    public interface IPollService
    {
        Task<PollModel> GetPollAsync(Guid id);
        Task<List<PollModel>> GetPollsAsync(int startPage = 0, string? search = null);

        Task<PollModel> CreatePollAsync(CreatePollDto poll);
        Task<List<PollModel>> CreatePollsAsync(List<CreatePollDto> polls);

        Task<PollModel> UpdatePollAsync(Guid id, UpdatePollDto poll);

        Task DeletePollAsync(Guid id);
        Task DeletePollsAsync(List<Guid> pollIds);
    }
}
