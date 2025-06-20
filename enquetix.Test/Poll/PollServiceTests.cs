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

namespace enquetix.Test.Poll
{
	public class PollServiceTests
	{
		[Fact]
		public async Task CreatePollAsync_CreatesPoll_WhenDataIsValid()
		{
			// Arrange
			var options = new DbContextOptionsBuilder<Context>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;
			using var context = new Context(options);

			var authService = new Mock<enquetix.Modules.Auth.Services.IAuthService>();
			var userId = Guid.NewGuid();
			authService.Setup(a => a.GetLoggedUserId()).Returns(userId);

			var cacheService = new Mock<enquetix.Modules.Application.Redis.ICacheService>();
			cacheService.Setup(c => c.RemoveByPartialNameAsync(It.IsAny<string>())).ReturnsAsync(true);

			var service = new PollService(context, authService.Object, cacheService.Object);
			var pollDto = new CreatePollDto
			{
				Title = "Enquete Teste",
				Description = "Descrição da enquete",
				StartDate = DateTimeOffset.UtcNow.AddHours(1),
				EndDate = DateTimeOffset.UtcNow.AddHours(2)
			};

			// Act
			var result = await service.CreatePollAsync(pollDto);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(pollDto.Title, result.Title);
			Assert.Equal(pollDto.Description, result.Description);
			Assert.Equal(userId, result.CreatedBy);
		}

		[Fact]
		public async Task CreatePollAsync_Throws_WhenDatesAreInvalid()
		{
			// Arrange
			var options = new DbContextOptionsBuilder<Context>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;
			using var context = new Context(options);
			var authService = new Mock<enquetix.Modules.Auth.Services.IAuthService>();
			authService.Setup(a => a.GetLoggedUserId()).Returns(Guid.NewGuid());
			var cacheService = new Mock<enquetix.Modules.Application.Redis.ICacheService>();
			var service = new PollService(context, authService.Object, cacheService.Object);
			var pollDto = new CreatePollDto
			{
				Title = "Enquete Teste",
				Description = "Descrição da enquete",
				StartDate = DateTimeOffset.UtcNow.AddHours(-1),
				EndDate = DateTimeOffset.UtcNow.AddHours(2)
			};

			// Act & Assert
			await Assert.ThrowsAsync<HttpResponseException>(() => service.CreatePollAsync(pollDto));
		}
	}
}
