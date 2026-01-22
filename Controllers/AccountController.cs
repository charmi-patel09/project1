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

                ModelState.AddModelError("Email", "User with this email already exists");
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
                        TempData["SuccessMessage"] = "Student account created successfully";
                        return RedirectToAction("Dashboard", "Home");
                    }

                    if (purpose == "StudentCreation")
                    {
                        string name = HttpContext.Session.GetString("PendingUserName") ?? "New Student";
                        string ageStr = HttpContext.Session.GetString("PendingUserAge") ?? "18";
                        string course = HttpContext.Session.GetString("PendingUserCourse") ?? "General";
                        int age = int.TryParse(ageStr, out int a) ? a : 18;

                        // Finalize Creation
                        _authService.RegisterStudent(email, password, name, age, course);

                        // Clear creation-specific data
                        HttpContext.Session.Remove("PendingUserName");
                        HttpContext.Session.Remove("PendingUserAge");
                        HttpContext.Session.Remove("PendingUserCourse");

                        // Auto-login after verification
                        HttpContext.Session.SetString("StudentUser", email);
                        TempData["SuccessMessage"] = "Student account created successfully";
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
                else if (DateTime.Now > expiry)
                {
                    ViewBag.ErrorMessage = "OTP has expired";
                }
                else
                {
                    ViewBag.ErrorMessage = "Invalid OTP";
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Session expired or invalid";
            }

            ModelState.Clear();
            return View();
        }

        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult Login()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("AdminUser")) ||
                !string.IsNullOrEmpty(HttpContext.Session.GetString("StudentUser")))
            {
                return RedirectToAction("Dashboard", "Home");
            }
            ModelState.Clear();
            return View(new AdminUser());
        }

        [HttpPost]
        public IActionResult Login(AdminUser model)
        {
            if (ModelState.IsValid)
            {
                // 1. Try Admin Login first
                if (_authService.ValidateAdmin(model.Email!, model.Password!, out string? adminError))
                {
                    HttpContext.Session.SetString("AdminUser", model.Email!);
                    return RedirectToAction("Dashboard", "Home");
                }

                // 2. Try Student Login if Admin fails (or if email not found in Admin)
                if (_authService.ValidateStudent(model.Email!, model.Password!, out string? studentError))
                {
                    // Direct Login without OTP for Students as per user request
                    HttpContext.Session.SetString("StudentUser", model.Email!);
                    return RedirectToAction("Dashboard", "Home");
                }

                // If both fail, determine which error to show
                // If the email belongs to neither, show "unregistered email" message
                if (!_authService.UserExists(model.Email!))
                {
                    ViewBag.ErrorMessage = "Email not registered";
                }
                else
                {
                    ViewBag.ErrorMessage = "Incorrect password";
                }
            }

            ModelState.Clear();
            return View(new AdminUser());
        }

        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult StudentLogin()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("StudentUser")))
            {
                return RedirectToAction("Dashboard", "Home");
            }
            ModelState.Clear();
            return View(new AdminUser());
        }

        [HttpPost]
        public IActionResult StudentLogin(AdminUser model)
        {
            if (ModelState.IsValid)
            {
                if (_authService.ValidateStudent(model.Email!, model.Password!, out string? error))
                {
                    // Direct Login without OTP for Students as per user request
                    HttpContext.Session.SetString("StudentUser", model.Email!);
                    return RedirectToAction("Dashboard", "Home");
                }

                // Unified error handling
                if (!_authService.UserExists(model.Email!))
                {
                    ViewBag.ErrorMessage = "Email not registered";
                }
                else
                {
                    ViewBag.ErrorMessage = "Incorrect password";
                }
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

                    ViewBag.SuccessMessage = "Password reset link sent to your email";
                    return View();
                }
                ViewBag.ErrorMessage = "Email not found";
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
                TempData["ErrorMessage"] = "Invalid or expired reset token";
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
                ViewBag.ErrorMessage = "Invalid or expired reset token";
            }
            return View(model);
        }
    }
}
