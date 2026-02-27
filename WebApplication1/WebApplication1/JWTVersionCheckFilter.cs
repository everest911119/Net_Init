using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace WebApplication1
{
    public class JWTVersionCheckFilter : IAsyncActionFilter
    {
        private readonly UserManager<MyUser> _userManager;

        public JWTVersionCheckFilter(UserManager<MyUser> userManager)
        {
            _userManager = userManager;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var controllerActionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;

            if (controllerActionDescriptor == null) return;
            if (controllerActionDescriptor.MethodInfo?.GetCustomAttributes(inherit: true).Any(
                a => a.GetType().Equals(typeof(NoJWTVersionCheckAttribute))) ?? false)
            {
                await next();
            }
            if (controllerActionDescriptor.ControllerTypeInfo?.
                GetCustomAttributes(typeof(NoJWTVersionCheckAttribute), true)?.Any() ?? false)
            {
                await next();
            }
            var claimJWTVersion = context.HttpContext.User.FindFirst("JWTVersion");
            if (claimJWTVersion == null)
            {
                context.Result = new ObjectResult(
                    new
                    {
                        error = "JWTVersion claim is missing"
                    })
                { StatusCode = 400 };
                return;

            }else
            {
                var claimUserId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                int userId = Convert.ToInt32(claimUserId.Value);

                long jwtVersion = Convert.ToInt64(claimJWTVersion.Value);

                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user ==null)
                {
                    context.Result = new ObjectResult("user not found") { StatusCode = 400 };
                    return;
                }
                if (user.JWTVersion > jwtVersion)
                {
                    context.Result = new ObjectResult("JWTVersion is outdated") { StatusCode = 400 };
                    return;
                }
                await next();
            }
        }

      
    }
}
