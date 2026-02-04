using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JsonCrudApp.Attributes
{
    /// <summary>
    /// Custom Authorization Filter to enforce Role-based access using Session.
    /// Redirects to Login if not authenticated, or AccessDenied if unauthorized.
    /// </summary>
    public class AuthorizeRoleAttribute : ActionFilterAttribute
    {
        private readonly string _requiredRole;

        public AuthorizeRoleAttribute(string role)
        {
            _requiredRole = role;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var userRole = session.GetString("Role");
            var adminUser = session.GetString("AdminUser"); // Fallback check
            var studentUser = session.GetString("StudentUser"); // Fallback check

            // 1. Check Authentication state
            bool isAuthenticated = !string.IsNullOrEmpty(userRole) ||
                                   !string.IsNullOrEmpty(adminUser) ||
                                   !string.IsNullOrEmpty(studentUser);

            if (!isAuthenticated)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            // 2. Validate Role
            // If Role session var is missing but AdminUser is set, treat as Admin (legacy/fallback)
            if (string.IsNullOrEmpty(userRole) && !string.IsNullOrEmpty(adminUser))
            {
                userRole = "Admin";
            }
            // If Role session var is missing but StudentUser is set, treat as User
            if (string.IsNullOrEmpty(userRole) && !string.IsNullOrEmpty(studentUser))
            {
                userRole = "User";
            }

            // Strict Check
            if (userRole != _requiredRole)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
