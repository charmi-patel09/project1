using Microsoft.AspNetCore.Mvc;
using JsonCrudApp.Services;
using System.Security.Cryptography;
using System.Text;

namespace JsonCrudApp.Controllers
{
    public class SecurityController : Controller
    {
        private readonly AuthService _authService;

        public SecurityController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult CheckPinStatus()
        {
            var email = HttpContext.Session.GetString("StudentUser");
            if (string.IsNullOrEmpty(email)) return Json(new { hasPin = false, isVerified = false });

            bool hasPin = _authService.HasPin(email);
            bool isVerified = HttpContext.Session.GetString("PinVerified") == "true";

            return Json(new { hasPin, isVerified });
        }


        [HttpPost]
        public IActionResult VerifyPin(string pin)
        {
            var email = HttpContext.Session.GetString("StudentUser");
            if (string.IsNullOrEmpty(email)) return Json(new { success = false, message = "Session expired" });

            if (_authService.VerifyPin(email, pin))
            {
                HttpContext.Session.SetString("PinVerified", "true");
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Incorrect PIN" });
        }
    }
}
