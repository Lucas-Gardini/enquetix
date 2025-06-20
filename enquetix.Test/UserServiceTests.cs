using enquetix.Modules.User.Services;
using enquetix.Modules.User.DTOs;
using enquetix.Modules.Application.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;
using System.Threading.Tasks;

namespace enquetix.Test
{
	public class UserServiceTests
	{
		[Fact]
		public async Task CreateAsync_CreatesUser_WhenDataIsValid()
		{
			// Arrange
			var options = new DbContextOptionsBuilder<Context>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;
			using var context = new Context(options);
			var service = new UserService(context);
			var request = new CreateUserDto
			{
				Email = "user@email.com",
				Password = "senha123",
				Username = "usuario"
			};

			// Act
			var result = await service.CreateAsync(request);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(request.Email, result.Email);
			Assert.Equal(request.Username, result.Username);
		}

		[Fact]
		public async Task CreateAsync_Throws_WhenUserAlreadyExists()
		{
			// Arrange
			var options = new DbContextOptionsBuilder<Context>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;
			using var context = new Context(options);
			var service = new UserService(context);
			var request = new CreateUserDto
			{
				Email = "user@email.com",
				Password = "senha123",
				Username = "usuario"
			};
			await service.CreateAsync(request);

			// Act & Assert
			await Assert.ThrowsAsync<enquetix.Modules.Application.HttpResponseException>(() => service.CreateAsync(request));
		}

		[Fact]
		public async Task GetByIdAsync_ReturnsUser_WhenUserExists()
		{
			// Arrange
			var options = new DbContextOptionsBuilder<Context>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;
			using var context = new Context(options);
			var service = new UserService(context);
			var request = new CreateUserDto
			{
				Email = "user@email.com",
				Password = "senha123",
				Username = "usuario"
			};
			var created = await service.CreateAsync(request);

			// Act
			var result = await service.GetByIdAsync(created.Id!.Value);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(request.Email, result.Email);
			Assert.Equal(request.Username, result.Username);
		}

		[Fact]
		public async Task GetByIdAsync_ReturnsNull_WhenUserDoesNotExist()
		{
			// Arrange
			var options = new DbContextOptionsBuilder<Context>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;
			using var context = new Context(options);
			var service = new UserService(context);

			// Act
			var result = await service.GetByIdAsync(Guid.NewGuid());

			// Assert
			Assert.Null(result);
		}
	}
}
