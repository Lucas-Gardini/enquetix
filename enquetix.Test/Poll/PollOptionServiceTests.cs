using enquetix.Modules.Poll.Services;
using enquetix.Modules.Poll.DTOs;
using enquetix.Modules.Poll.Repository;
using enquetix.Modules.Application.EntityFramework;
using enquetix.Modules.Application;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace enquetix.Test.Poll
{
	public class PollOptionServiceTests
	{
		[Fact]
		public async Task CreatePollOptionAsync_CreatesOption_WhenDataIsValid()
		{
			// Arrange
			var options = new DbContextOptionsBuilder<Context>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;
			using var context = new Context(options);
			var poll = new PollModel { Id = Guid.NewGuid(), Title = "Enquete", Description = "Desc", StartDate = DateTimeOffset.UtcNow, EndDate = DateTimeOffset.UtcNow.AddDays(1), CreatedBy = Guid.NewGuid() };
			context.Polls.Add(poll);
			context.SaveChanges();

			var authService = new Mock<enquetix.Modules.Auth.Services.IAuthService>();
			authService.Setup(a => a.GetLoggedUserId()).Returns(poll.CreatedBy);
			var cacheService = new Mock<enquetix.Modules.Application.Redis.ICacheService>();
			cacheService.Setup(c => c.RemoveAsync(It.IsAny<string>())).ReturnsAsync(true);
			var service = new PollOptionService(context, authService.Object, cacheService.Object);
			var optionDto = new CreatePollOptionDto { OptionText = "Opção 1" };

			// Act
			var result = await service.CreatePollOptionAsync(poll.Id, optionDto);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(optionDto.OptionText, result.OptionText);
			Assert.Equal(poll.Id, result.PollId);
		}
	}
}
