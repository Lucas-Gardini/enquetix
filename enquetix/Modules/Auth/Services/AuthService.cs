using enquetix.Modules.Application;
using enquetix.Modules.Application.EntityFramework;
using enquetix.Modules.AuditLog.Repository;
using enquetix.Modules.AuditLog.Services;
using enquetix.Modules.User.Repository;
using Microsoft.EntityFrameworkCore;

namespace enquetix.Modules.Auth.Services
{
    public class AuthService(Context context, IHttpContextAccessor httpContextAccessor, IAuditLogService auditLogService) : IAuthService
    {
        public async Task SaveAccessLogLogin()
        {
            var userId = httpContextAccessor.HttpContext?.Session.GetString(SessionKeys.UserId) ?? throw new HttpResponseException
            {
                Status = 401,
                Value = new { Message = "User not logged in." }
            };
            var log = new AccessLogModel
            {
                User = userId,
                Operation = AccessLogOperation.Login,
                Timestamp = DateTime.UtcNow
            };
            await auditLogService.SaveAccessLog(log);
        }

        public async Task SaveAccessLogLogout()
        {
            var userId = httpContextAccessor.HttpContext?.Session.GetString(SessionKeys.UserId);
            if (userId == null) return;

            var log = new AccessLogModel
            {
                User = userId,
                Operation = AccessLogOperation.Logout,
                Timestamp = DateTime.UtcNow
            };
            await auditLogService.SaveAccessLog(log);
        }

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

            var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId)) ?? throw new HttpResponseException
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

        public Guid? GetLoggedUserIdSafe()
        {
            var userId = httpContextAccessor.HttpContext?.Session.GetString(SessionKeys.UserId);
            return userId != null ? Guid.Parse(userId) : null;
        }
    }

    public interface IAuthService
    {
        Task SaveAccessLogLogin();
        Task SaveAccessLogLogout();

        Task<UserModel> ValidateUserAsync(string email, string password);
        Task<UserModel> GetLoggedUserAsync();
        Guid GetLoggedUserId();
        Guid? GetLoggedUserIdSafe();
    }

    public static class SessionKeys
    {
        public const string UserId = "UserId";
        public const string Username = "Username";
    }
}
