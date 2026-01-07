using Microsoft.AspNetCore.Mvc;
using JsonCrudApp.Services;
using JsonCrudApp.Models;

namespace JsonCrudApp.Controllers
{
    /// <summary>
    /// Controller to manage user login and logout sessions.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly AuthService _authService;
        private readonly EmailService _emailService;
        private readonly OtpService _otpService;

        public AccountController(AuthService authService, EmailService emailService, OtpService otpService)
        {
            _authService = authService;
            _emailService = emailService;
            _otpService = otpService;
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            ModelState.Clear();
            return View(new SignUpViewModel());
        }

        [HttpPost]
        public IActionResult SignUp(SignUpViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (!_authService.UserExists(model.Email))
                {
                    // Generate OTP for registration verification
                    string otp = _otpService.GenerateOtp();
                    DateTime expiry = DateTime.Now.AddMinutes(2);

                    // Send email
                    _emailService.SendOtpEmail(model.Email, otp);

                    // Store in session for verification
                    HttpContext.Session.SetString("PendingUserEmail", model.Email);
                    HttpContext.Session.SetString("PendingUserPassword", model.Password); // Store password temporarily
                    HttpContext.Session.SetString("OtpPurpose", "Registration");
                    HttpContext.Session.SetString("OtpCode", otp);
                    HttpContext.Session.SetString("OtpExpiry", expiry.ToString("O"));

                    return RedirectToAction("VerifyOtp");
                }

                ModelState.AddModelError("Email", "User with this email already exists.");
            }

            return View(model);
        }

        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult VerifyOtp()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("PendingUserEmail")))
            {
                return RedirectToAction("Login");
            }

            ModelState.Clear();
            return View();
        }

        [HttpPost]
        public IActionResult VerifyOtp(string otp)
        {
            string storedOtp = HttpContext.Session.GetString("OtpCode") ?? "";
            string expiryStr = HttpContext.Session.GetString("OtpExpiry") ?? "";
            string email = HttpContext.Session.GetString("PendingUserEmail") ?? "";
            string password = HttpContext.Session.GetString("PendingUserPassword") ?? "";
            string purpose = HttpContext.Session.GetString("OtpPurpose") ?? "";
            string userType = HttpContext.Session.GetString("PendingUserType") ?? "Admin";

            if (DateTime.TryParse(expiryStr, out DateTime expiry))
            {
                if (_otpService.IsValid(otp, storedOtp, expiry))
                {
                    // OTP is valid
                    HttpContext.Session.Remove("OtpCode");
                    HttpContext.Session.Remove("OtpExpiry");
                    HttpContext.Session.Remove("PendingUserEmail");
                    HttpContext.Session.Remove("PendingUserPassword");
                    HttpContext.Session.Remove("OtpPurpose");
                    HttpContext.Session.Remove("PendingUserType");

                    if (purpose == "Registration")
                    {
                        // Finalize Registration
                        _authService.RegisterStudent(email, password);

                        // Auto-login after verification
                        HttpContext.Session.SetString("StudentUser", email);
                        TempData["SuccessMessage"] = "Student account created successfully!";
                        return RedirectToAction("Dashboard", "Home");
                    }



                    // For Login flow
                    if (userType == "Student")
                    {
                        HttpContext.Session.SetString("StudentUser", email);
                    }
                    else
                    {
                        HttpContext.Session.SetString("AdminUser", email);
                    }
                    return RedirectToAction("Dashboard", "Home");
                }
            }

            ViewBag.ErrorMessage = "invalid otp";
            ModelState.Clear();
            return View();
        }

        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult Login()
        {
            ModelState.Clear();
            return View(new AdminUser());
        }

        [HttpPost]
        public IActionResult Login(AdminUser model)
        {
            if (ModelState.IsValid)
            {
                if (_authService.ValidateUser(model.Email!, model.Password!, out string? error))
                {
                    HttpContext.Session.SetString("AdminUser", model.Email!);
                    return RedirectToAction("Dashboard", "Home");
                }

                ViewBag.ErrorMessage = error;
            }

            ModelState.Clear();
            return View(new AdminUser());
        }

        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult StudentLogin()
        {
            ModelState.Clear();
            return View(new AdminUser());
        }

        [HttpPost]
        public IActionResult StudentLogin(AdminUser model)
        {
            if (ModelState.IsValid)
            {
                if (_authService.ValidateUser(model.Email!, model.Password!, out string? error))
                {
                    HttpContext.Session.SetString("StudentUser", model.Email!);
                    return RedirectToAction("Dashboard", "Home");
                }

                ViewBag.ErrorMessage = error;
            }

            ModelState.Clear();
            return View(new AdminUser());
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Clear all session data
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (_authService.UserExists(model.Email))
                {
                    string token = _authService.GenerateResetToken(model.Email);

                    var baseUrl = $"{Request.Scheme}://{Request.Host}";
                    var resetLink = $"{baseUrl}/Account/ResetPassword?email={Uri.EscapeDataString(model.Email)}&token={token}";

                    _emailService.SendResetLinkEmail(model.Email, resetLink);

                    ViewBag.SuccessMessage = "A password reset link has been sent to your email.";
                    return View();
                }
                ViewBag.ErrorMessage = "Email not found.";
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login");
            }

            if (!_authService.ValidateResetToken(email, token))
            {
                TempData["ErrorMessage"] = "Invalid or expired reset token.";
                return RedirectToAction("Login");
            }

            return View(new ResetPasswordViewModel { Email = email, Token = token });
        }

        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (_authService.ResetPassword(model.Email, model.NewPassword, model.Token))
                {
                    TempData["SuccessMessage"] = "Password reset successful. You can now login.";
                    return RedirectToAction("Login");
                }
                ViewBag.ErrorMessage = "Invalid or expired reset token.";
            }
            return View(model);
        }
    }
}
