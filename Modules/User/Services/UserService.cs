using enquetix.Modules.Application;
using enquetix.Modules.Application.EntityFramework;
using enquetix.Modules.User.DTOs;
using Microsoft.EntityFrameworkCore;

namespace enquetix.Modules.User.Services
{
    static class PasswordHasher
    {
        private const int WorkFactor = 12;
        public static string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, workFactor: WorkFactor);
        public static bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
    }

    public class UserService(Context context) : IUserService
    {
        public async Task<GetUserDto> CreateAsync(CreateUserDto request)
        {
            var exists = await context.Users.AnyAsync(u => u.Email == request.Email || u.Username == request.Username);
            if (exists) throw new HttpResponseException
            {
                Status = 409,
                Value = new { Message = "User with this email or username already exists." }
            };

            var user = new Repository.UserModel
            {
                Email = request.Email,
                Password = PasswordHasher.Hash(request.Password),
                Username = request.Username
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return new GetUserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username
            };
        }

        public async Task<GetUserDto?> GetByIdAsync(Guid id)
        {
            var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            return user is null ? null : new GetUserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username
            };
        }
    }

    public interface IUserService
    {
        Task<GetUserDto> CreateAsync(CreateUserDto request);
        Task<GetUserDto?> GetByIdAsync(Guid id);
    }
}
