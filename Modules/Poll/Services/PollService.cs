using enquetix.Modules.Application;
using enquetix.Modules.Application.EntityFramework;
using enquetix.Modules.Auth.Services;
using enquetix.Modules.Poll.DTOs;
using enquetix.Modules.Poll.Repository;
using Microsoft.EntityFrameworkCore;

namespace enquetix.Modules.Poll.Services
{
    public class PollService(Context context, IAuthService authService) : IPollService
    {
        public async Task<PollModel> GetPollAsync(Guid id)
        {
            return await context.Polls.FindAsync(id)
                   ?? throw new HttpResponseException { Status = 404, Value = new { Message = "Poll not found." } };
        }

        public async Task<List<PollModel>> GetPollsAsync(int startPage = 0, string? search = null)
        {
            const int pageSize = 20;

            var query = context.Polls.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.Title.Contains(search) || p.Description.Contains(search));
            }

            return await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip(startPage * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<PollModel> CreatePollAsync(CreatePollDto poll)
        {
            if (poll.StartDate < DateTime.UtcNow || poll.EndDate < DateTime.UtcNow)
                throw new HttpResponseException { Status = 400, Value = new { Message = "Invalid Date." } };

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
