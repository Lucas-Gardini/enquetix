using enquetix.Modules.Auth.Services;
using enquetix.Modules.User.Repository;
using enquetix.Modules.Application.EntityFramework;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using Moq;
using Xunit;
using System.Threading.Tasks;
using System;
using enquetix.Modules.Application;

namespace enquetix.Test
{
	public class AuthServiceTests
	{
		[Fact]
		public async Task ValidateUserAsync_ReturnsUser_WhenCredentialsAreValid()
		{
			// Arrange
			var options = new DbContextOptionsBuilder<Context>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;
			using var context = new Context(options);
			var password = BCrypt.Net.BCrypt.HashPassword("123456");
			var user = new UserModel { Email = "test@email.com", Password = password, Username = "testuser" };
			context.Users.Add(user);
			context.SaveChanges();

			var httpContextAccessor = new Mock<IHttpContextAccessor>();
			var service = new AuthService(context, httpContextAccessor.Object);

			// Act
			var result = await service.ValidateUserAsync("test@email.com", "123456");

			// Assert
			Assert.NotNull(result);
			Assert.Equal(user.Email, result.Email);
		}

		[Fact]
		public async Task ValidateUserAsync_Throws_WhenInvalidCredentials()
		{
			// Arrange
			var options = new DbContextOptionsBuilder<Context>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;
			using var context = new Context(options);
			var password = BCrypt.Net.BCrypt.HashPassword("123456");
			var user = new UserModel { Email = "test@email.com", Password = password, Username = "testuser" };
			context.Users.Add(user);
			context.SaveChanges();

			var httpContextAccessor = new Mock<IHttpContextAccessor>();
			var service = new AuthService(context, httpContextAccessor.Object);

			// Act & Assert
			await Assert.ThrowsAsync<HttpResponseException>(() => service.ValidateUserAsync("test@email.com", "wrongpass"));
			await Assert.ThrowsAsync<HttpResponseException>(() => service.ValidateUserAsync("notfound@email.com", "123456"));
		}
	}
}
