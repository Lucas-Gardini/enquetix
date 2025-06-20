using enquetix.Modules.Poll.Services;
using enquetix.Modules.Poll.DTOs;
using enquetix.Modules.Poll.Repository;
using enquetix.Modules.Application.EntityFramework;
using enquetix.Modules.Application;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using enquetix.Modules.User.Repository;

namespace enquetix.Test.Poll
{
	public class PollVoteServiceTests
	{
		[Fact]
		public async Task ProcessMessageAsync_CreatesVote_WhenDataIsValid()
		{
			// Arrange
			var options = new DbContextOptionsBuilder<Context>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;
			using var context = new Context(options);
			var poll = new PollModel { Id = Guid.NewGuid(), Title = "Enquete", Description = "Desc", StartDate = DateTimeOffset.UtcNow, EndDate = DateTimeOffset.UtcNow.AddDays(1), CreatedBy = Guid.NewGuid() };
			var user = new UserModel { Id = Guid.NewGuid(), Email = "user@email.com", Password = "senha", Username = "usuario" };
			var option = new PollOptionModel { Id = Guid.NewGuid(), PollId = poll.Id, OptionText = "Opção 1" };
			context.Polls.Add(poll);
			context.Users.Add(user);
			context.PollOptions.Add(option);
			context.SaveChanges();

			var cacheService = new Mock<enquetix.Modules.Application.Redis.ICacheService>();
			cacheService.Setup(c => c.RemoveAsync(It.IsAny<string>())).ReturnsAsync(true);
			var authService = new Mock<enquetix.Modules.Auth.Services.IAuthService>();
			var queueManager = new Mock<IPollVoteQueueManager>();
			var service = new PollVoteService(context, queueManager.Object, authService.Object, cacheService.Object);

			var voteDto = new CreateUpdatePollVoteDto { PollId = poll.Id, OptionId = option.Id, UserId = user.Id };
			var messageBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(voteDto));

			// Act
			await service.ProcessMessageAsync(messageBody);

			// Assert
			var vote = await context.PollVotes.FirstOrDefaultAsync(v => v.PollId == poll.Id && v.UserId == user.Id);
			Assert.NotNull(vote);
			Assert.Equal(option.Id, vote.OptionId);
		}
	}
}
