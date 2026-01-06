using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JsonCrudApp.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var adminUser = context.HttpContext.Session.GetString("AdminUser");
            var studentUser = context.HttpContext.Session.GetString("StudentUser");

            if (string.IsNullOrEmpty(adminUser) && string.IsNullOrEmpty(studentUser))
            {
                // Redirect to Login if neither session is present
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }
            base.OnActionExecuting(context);
        }
    }
}
