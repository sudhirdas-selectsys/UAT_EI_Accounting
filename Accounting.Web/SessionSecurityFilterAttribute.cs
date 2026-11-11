using Accounting.Entities.Internal;
using Accounting.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Accounting.Web
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SessionSecurityFilterAttribute : Attribute, IAuthorizationFilter
    {
        public bool RequireOtpVerification { get; set; } = true;

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;
            UserSession userSession = httpContext.Session.GetObjectFromJson<UserSession>("UserSession");

            // Session validation
            if (userSession == null)
            {
                HandleUnauthorized(context);
                return;
            }

            // OTP validation
            if (RequireOtpVerification && !userSession.Is_OtpVerified)
            {
                HandleUnauthorized(context);
                return;
            }

            // Future validations:
            // Role checks
            // Permission checks
            // Token checks
            // IP validation
            // Device validation
        }

        private void HandleUnauthorized(AuthorizationFilterContext context)
        {
            context.HttpContext.Session.Clear();
            //context.Result = new RedirectResult("/Login/Index");
            context.Result = new ContentResult
            {
                Content = "<script>window.top.location.href='/Login/Index';</script>",
                ContentType = "text/html"
            };
        }
    }
}
