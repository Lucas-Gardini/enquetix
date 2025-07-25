﻿using enquetix.Modules.Application;
using enquetix.Modules.Application.EntityFramework;
using enquetix.Modules.Application.Redis;
using enquetix.Modules.Auth.Services;
using enquetix.Modules.Poll.DTOs;
using enquetix.Modules.Poll.Hubs;
using enquetix.Modules.Poll.Repository;
using Microsoft.EntityFrameworkCore;

namespace enquetix.Modules.Poll.Services
{
    public class PollOptionService(Context context, IAuthService authService, ICacheService cacheService, IPollHubService pollHubService) : IPollOptionService
    {
        public async Task<PollOptionModel> GetPollOptionAsync(Guid id)
        {
            var cacheKey = $"pollOption:{id}";
            var option = await cacheService.CacheAsync(cacheKey, async () =>
            {
                return await context.PollOptions.FirstOrDefaultAsync(o => o.Id == id)
                    ?? throw new HttpResponseException { Status = 404, Value = new { Message = "Option not found" } };
            }, TimeSpan.FromMinutes(1));
            return option!;
        }

        public async Task<List<PollOptionModel>> GetPollOptionsAsync(Guid pollId)
        {
            var pollOptionsKey = $"pollOptions:poll:{pollId}";

            if (!await context.Polls.AsNoTracking().AnyAsync(p => p.Id == pollId))
                throw new HttpResponseException { Status = 404, Value = new { Message = "Poll not found" } };

            var options = await cacheService.CacheAsync(pollOptionsKey, async () =>
            {
                return await context.PollOptions.AsNoTracking().Where(p => p.PollId == pollId).ToListAsync();
            }, TimeSpan.FromMinutes(1));
            return options!;
        }

        public async Task<PollOptionModel> CreatePollOptionAsync(Guid pollId, CreatePollOptionDto poll)
        {
            if (!await context.Polls.AnyAsync(p => p.Id == pollId))
                throw new HttpResponseException { Status = 404, Value = new { Message = "Poll not found" } };

            var exists = await context.PollOptions
                .AnyAsync(o => o.PollId == pollId && o.OptionText.ToLower() == poll.OptionText.ToLower());
            if (exists)
                throw new HttpResponseException { Status = 409, Value = new { Message = "Option with same text already exists in this poll." } };

            var option = new PollOptionModel
            {
                PollId = pollId,
                OptionText = poll.OptionText
            };

            context.PollOptions.Add(option);
            await context.SaveChangesAsync();
            await cacheService.RemoveAsync($"pollOptions:poll:{pollId}");
            await cacheService.RemoveAsync($"poll:{pollId}");

            await pollHubService.NotifyPollOptionCreated(pollId.ToString(), option);

            return option;
        }

        public async Task<List<PollOptionModel>> CreatePollOptionsAsync(Guid pollId, List<CreatePollOptionDto> polls)
        {
            if (!await context.Polls.AnyAsync(p => p.Id == pollId))
                throw new HttpResponseException { Status = 404, Value = new { Message = "Poll not found" } };

            var existingTexts = await context.PollOptions
                .Where(o => o.PollId == pollId)
                .Select(o => o.OptionText.ToLower())
                .ToListAsync();

            var duplicates = polls
                .Select(p => p.OptionText.ToLower())
                .Intersect(existingTexts)
                .ToList();

            if (duplicates.Count != 0)
                throw new HttpResponseException
                {
                    Status = 409,
                    Value = new { Message = $"Duplicate option(s) detected: {string.Join(", ", duplicates)}" }
                };

            var options = polls.Select(dto => new PollOptionModel
            {
                PollId = pollId,
                OptionText = dto.OptionText
            }).ToList();

            context.PollOptions.AddRange(options);
            await context.SaveChangesAsync();
            await cacheService.RemoveAsync($"poll:{pollId}");
            await cacheService.RemoveAsync($"pollOptions:poll:{pollId}");

            foreach (var option in options)
            {
                await cacheService.RemoveAsync($"pollOption:{option.Id}");
                await pollHubService.NotifyPollOptionCreated(pollId.ToString(), option);
            }

            return options;
        }

        public async Task<PollOptionModel> UpdatePollOptionAsync(Guid id, UpdatePollOptionDto poll)
        {
            var existingOption = await GetPollOptionAsync(id);
            existingOption.OptionText = poll.OptionText ?? existingOption.OptionText;

            context.PollOptions.Update(existingOption);
            await context.SaveChangesAsync();
            await cacheService.RemoveAsync($"poll:{existingOption.PollId}");
            await cacheService.RemoveAsync($"pollOption:{id}");
            await cacheService.RemoveAsync($"pollOptions:poll:{existingOption.PollId}");

            await pollHubService.NotifyPollOptionUpdated(existingOption.PollId.ToString(), existingOption);

            return existingOption;
        }

        public async Task DeletePollOptionAsync(Guid id)
        {
            var existingOption = await GetPollOptionAsync(id);

            if (!await context.Polls.AsNoTracking().AnyAsync(p => p.Id == existingOption.PollId && p.CreatedBy == authService.GetLoggedUserId()))
                throw new HttpResponseException
                {
                    Status = 401,
                    Value = new { Message = $"You do not have permission to delete this option." }
                };

            context.PollOptions.Remove(existingOption);
            await context.SaveChangesAsync();
            await cacheService.RemoveAsync($"poll:{existingOption.PollId}");
            await cacheService.RemoveAsync($"pollOption:{id}");
            await cacheService.RemoveAsync($"pollOptions:poll:{existingOption.PollId}");

            await pollHubService.NotifyPollOptionDeleted(existingOption.PollId.ToString(), existingOption);
        }

        public async Task DeletePollOptionsAsync(List<Guid> pollOptionsIds)
        {
            var options = await context.PollOptions
                .Where(o => pollOptionsIds.Contains(o.Id))
                .ToListAsync();

            foreach (var pollOption in options)
            {
                if (!await context.Polls.AsNoTracking().AnyAsync(p => p.Id == pollOption.PollId && p.CreatedBy == authService.GetLoggedUserId()))
                    throw new HttpResponseException
                    {
                        Status = 401,
                        Value = new { Message = $"You do not have permission to delete option with ID {pollOption.Id}." }
                    };
            }

            context.PollOptions.RemoveRange(options);
            await context.SaveChangesAsync();

            foreach (var option in options)
            {
                await cacheService.RemoveAsync($"poll:{option.PollId}");
                await cacheService.RemoveAsync($"pollOption:{option.Id}");
                await cacheService.RemoveAsync($"pollOptions:poll:{option.PollId}");

                await pollHubService.NotifyPollOptionDeleted(option.PollId.ToString(), option);
            }
        }

        public async Task DeletePollOptionsAsync(Guid pollId)
        {
            if (!await context.Polls.AnyAsync(p => p.Id == pollId))
                throw new HttpResponseException { Status = 404, Value = new { Message = "Poll not found" } };

            if (!await context.Polls.AsNoTracking().AnyAsync(p => p.Id == pollId && p.CreatedBy == authService.GetLoggedUserId()))
                throw new HttpResponseException
                {
                    Status = 401,
                    Value = new { Message = $"You do not have permission to delete options for this poll." }
                };

            var options = await context.PollOptions
                .Where(o => o.PollId == pollId)
                .ToListAsync();

            context.PollOptions.RemoveRange(options);
            await context.SaveChangesAsync();
            await cacheService.RemoveAsync($"poll:{pollId}");
            await cacheService.RemoveAsync($"pollOptions:poll:{pollId}");
            foreach (var option in options)
            {
                await cacheService.RemoveAsync($"pollOption:{option.Id}");
                await pollHubService.NotifyPollOptionDeleted(pollId.ToString(), option);
            }
        }
    }

    public interface IPollOptionService
    {
        Task<PollOptionModel> GetPollOptionAsync(Guid id);
        Task<List<PollOptionModel>> GetPollOptionsAsync(Guid pollId);

        Task<PollOptionModel> CreatePollOptionAsync(Guid pollId, CreatePollOptionDto poll);
        Task<List<PollOptionModel>> CreatePollOptionsAsync(Guid pollId, List<CreatePollOptionDto> polls);

        Task<PollOptionModel> UpdatePollOptionAsync(Guid id, UpdatePollOptionDto poll);

        Task DeletePollOptionAsync(Guid id);
        Task DeletePollOptionsAsync(List<Guid> pollOptionsIds);
        Task DeletePollOptionsAsync(Guid pollId);
    }
}