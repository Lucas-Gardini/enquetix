using enquetix.Modules.Application;
using enquetix.Modules.Application.EntityFramework;
using enquetix.Modules.User.Repository;
using Microsoft.EntityFrameworkCore;

namespace enquetix.Modules.Auth.Services
{
    public class AuthService(Context context) : IAuthService
    {
        public async Task<UserModel> ValidateUser(string email, string password)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
                throw new HttpResponseException
                {
                    Status = 401,
                    Value = new { Message = "Invalid email or password." }
                };

            return user;
        }
    }

    public interface IAuthService
    {
        Task<UserModel> ValidateUser(string email, string password);
    }

    public static class SessionKeys
    {
        public const string UserId = "UserId";
        public const string Username = "Username";
    }
}
