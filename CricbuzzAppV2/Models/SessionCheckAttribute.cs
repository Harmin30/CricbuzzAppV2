using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CricbuzzAppV2.Filters
{
    public class SessionCheckAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var controller = context.RouteData.Values["controller"]?.ToString();
            var action = context.RouteData.Values["action"]?.ToString();

            // Publicly accessible pages
            if (controller == "UserPortal" ||
                (controller == "Account" && (action == "Login" || action == "Register")))
            {
                base.OnActionExecuting(context);
                return;
            }

            var username = context.HttpContext.Session.GetString("Username");
            var role = context.HttpContext.Session.GetString("Role");

            // Redirect to login if no session
            if (string.IsNullOrEmpty(username))
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            // Only Admins can access Home (admin dashboard)
            if (controller == "Home" && role != "Admin" && role != "SuperAdmin")
            {
                context.Result = new RedirectToActionResult("Index", "UserPortal", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
