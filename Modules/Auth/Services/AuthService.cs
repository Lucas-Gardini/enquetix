using enquetix.Modules.Application;
using enquetix.Modules.Application.EntityFramework;
using enquetix.Modules.User.Repository;
using Microsoft.EntityFrameworkCore;

namespace enquetix.Modules.Auth.Services
{
    public class AuthService(Context context, IHttpContextAccessor httpContextAccessor) : IAuthService
    {
        public async Task<UserModel> ValidateUserAsync(string email, string password)
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

        public async Task<UserModel> GetLoggedUserAsync()
        {
            var userId = (httpContextAccessor.HttpContext?.Session.GetString(SessionKeys.UserId)) ?? throw new HttpResponseException
            {
                Status = 401,
                Value = new { Message = "User not logged in." }
            };

            var user = await context.Users.FindAsync(Guid.Parse(userId)) ?? throw new HttpResponseException
            {
                Status = 404,
                Value = new { Message = "User not found." }
            };

            return user;
        }

        public Guid GetLoggedUserId()
        {
            var userId = (httpContextAccessor.HttpContext?.Session.GetString(SessionKeys.UserId)) ?? throw new HttpResponseException
            {
                Status = 401,
                Value = new { Message = "User not logged in." }
            };

            return Guid.Parse(userId);
        }
    }

    public interface IAuthService
    {
        Task<UserModel> ValidateUserAsync(string email, string password);
        Task<UserModel> GetLoggedUserAsync();
        Guid GetLoggedUserId();
    }

    public static class SessionKeys
    {
        public const string UserId = "UserId";
        public const string Username = "Username";
    }
}
