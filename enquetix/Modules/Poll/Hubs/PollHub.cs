using Microsoft.AspNetCore.SignalR;

namespace enquetix.Modules.Poll.Hubs
{
    public class PollHub : Hub
    {
        public async Task SubscribeToPoll(string pollId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, pollId);
        }

        public async Task UnsubscribeFromPoll(string pollId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, pollId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }

    public class PollHubService(IHubContext<PollHub> hubContext) : IPollHubService
    {
        public async Task NotifyPollUpdated(string pollId, object updatedData)
        {
            await hubContext.Clients.Group(pollId).SendAsync("PollUpdated", pollId, updatedData);
        }

        public async Task NotifyPollDeleted(string pollId)
        {
            await hubContext.Clients.Group(pollId).SendAsync("PollDeleted", pollId);
        }

        public async Task NotifyPollOptionCreated(string pollId, object option)
        {
            await hubContext.Clients.Group(pollId).SendAsync("PollOptionCreated", pollId, option);
        }

        public async Task NotifyPollOptionUpdated(string pollId, object option)
        {
            await hubContext.Clients.Group(pollId).SendAsync("PollOptionUpdated", pollId, option);
        }

        public async Task NotifyPollOptionDeleted(string pollId, object option)
        {
            await hubContext.Clients.Group(pollId).SendAsync("PollOptionDeleted", pollId, option);
        }

        public async Task NotifyPollVotesChanged(string pollId, string optionId, int totalVotes)
        {
            await hubContext.Clients.Group(pollId).SendAsync("PollVotesChanged", pollId, optionId, totalVotes);
        }
    }

    public interface IPollHubService
    {
        Task NotifyPollUpdated(string pollId, object updatedData);
        Task NotifyPollDeleted(string pollId);
        Task NotifyPollOptionCreated(string pollId, object option);
        Task NotifyPollOptionUpdated(string pollId, object option);
        Task NotifyPollOptionDeleted(string pollId, object option);
        Task NotifyPollVotesChanged(string pollId, string optionId, int totalVotes);
    }
}
