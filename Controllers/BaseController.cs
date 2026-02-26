using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using JsonCrudApp.Models;
using System.Linq;

namespace JsonCrudApp.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var studentUser = context.HttpContext.Session.GetString("StudentUser");

            if (string.IsNullOrEmpty(studentUser))
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }
            base.OnActionExecuting(context);
        }

        protected bool IsPinVerified(Student user, string widgetId = "")
        {
            if (user == null) return false;

            // Requirement: Disable Security PIN for Visitor role
            if (user.Role == UserRole.Visitor) return true;

            // Requirement 5: If security not enabled, it's always verified for API purposes.
            if (!user.IsSecurityEnabled) return true;

            // If enabled, require session verification for the specific widget if provided
            if (!string.IsNullOrEmpty(widgetId))
            {
                return HttpContext.Session.GetString($"Unlocked_{widgetId}") == "true";
            }

            // Fallback to legacy/global check if no widget specified
            return HttpContext.Session.GetString("PinVerified") == "true";
        }

        protected bool IsAuthorized(Student user, string widgetId)
        {
            if (user == null) return false;

            // STRICT PERMISSION CHECK: Verify against the role's allowed widgets stored in session
            var permissions = HttpContext.Session.GetString("WidgetPermissions") ?? "";
            var allowedWidgets = permissions.Split(',', System.StringSplitOptions.RemoveEmptyEntries);

            if (!allowedWidgets.Contains(widgetId))
            {
                return false;
            }

            // If the role allows the widget, then check if it's locked by a PIN
            return IsPinVerified(user, widgetId);
        }
    }
}
