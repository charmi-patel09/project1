using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

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

        protected bool IsPinVerified()
        {
            return HttpContext.Session.GetString("PinVerified") == "true";
        }
    }
}
