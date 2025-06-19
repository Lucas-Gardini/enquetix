using enquetix.Modules.Auth.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace enquetix.Modules.Auth.Middlewares
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class AuthorizeSessionAttribute : Attribute, IAsyncAuthorizationFilter, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var session = context.HttpContext.Session;
            var userId = session.GetString(SessionKeys.UserId);

            if (string.IsNullOrWhiteSpace(userId))
            {
                context.Result = new UnauthorizedResult();
            }
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var session = context.HttpContext.Session;

            var userId = session.GetString(SessionKeys.UserId);

            if (string.IsNullOrWhiteSpace(userId))
            {
                await session.LoadAsync();
                userId = session.GetString(SessionKeys.UserId);
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
