using Microsoft.AspNetCore.Mvc;
using JsonCrudApp.Services;
using JsonCrudApp.Models;
using JsonCrudApp.ViewModels;

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
        private readonly UserActivityService _userActivityService;

        public AccountController(AuthService authService, EmailService emailService, OtpService otpService, UserActivityService userActivityService)
        {
            _authService = authService;
            _emailService = emailService;
            _otpService = otpService;
            _userActivityService = userActivityService;
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
                    HttpContext.Session.SetString("PendingUserPin", model.SecurityPin); // Store PIN
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
                        string? pin = HttpContext.Session.GetString("PendingUserPin");
                        // Finalize Registration
                        var newStudent = new Student
                        {
                            Email = email,
                            Password = password,
                            Name = "New Student",
                            Age = 18,
                            Course = "General",
                            Role = "Private"
                        };

                        if (!string.IsNullOrEmpty(pin))
                        {
                            newStudent.SecurityPinHash = _authService.HashPassword(pin);
                            newStudent.IsSecurityEnabled = true;
                        }

                        _authService.RegisterStudent(newStudent);
                        HttpContext.Session.Remove("PendingUserPin");

                        // Auto-login & Log Activity
                        HttpContext.Session.SetString("StudentUser", email);
                        HttpContext.Session.SetString("Role", "Private");
                        HttpContext.Session.SetString("PinVerified", "false"); // Initial state
                        _userActivityService.LogVisit(email, "/Account/VerifyOtp"); // Log Registration Visit

                        TempData["SuccessMessage"] = "Account created successfully";
                        return RedirectToAction("Dashboard", "Home");
                    }

                    if (purpose == "StudentCreation")
                    {
                        string name = HttpContext.Session.GetString("PendingUserName") ?? "New Student";
                        string ageStr = HttpContext.Session.GetString("PendingUserAge") ?? "18";
                        string course = HttpContext.Session.GetString("PendingUserCourse") ?? "General";
                        string role = HttpContext.Session.GetString("PendingUserRole") ?? "Private";
                        string widgets = HttpContext.Session.GetString("PendingUserWidgets") ?? "";
                        string? pin = HttpContext.Session.GetString("PendingUserPin");
                        int age = int.TryParse(ageStr, out int a) ? a : 18;

                        // Finalize Creation
                        // If Admin, ensure Course is Administration
                        if (role == "Admin") course = "Administration";

                        var newStudent = new Student
                        {
                            Email = email,
                            Password = password,
                            Name = name,
                            Age = age,
                            Course = course,
                            Role = role,
                            WidgetPermissions = widgets
                        };

                        if (!string.IsNullOrEmpty(pin))
                        {
                            newStudent.SecurityPinHash = _authService.HashPassword(pin);
                            newStudent.IsSecurityEnabled = true;
                        }

                        _authService.RegisterStudent(newStudent);

                        // Clear creation-specific data
                        HttpContext.Session.Remove("PendingUserName");
                        HttpContext.Session.Remove("PendingUserAge");
                        HttpContext.Session.Remove("PendingUserCourse");
                        HttpContext.Session.Remove("PendingUserRole");
                        HttpContext.Session.Remove("PendingUserWidgets");
                        HttpContext.Session.Remove("PendingUserPin");

                        // Check if currently logged in as Admin
                        string? currentRole = HttpContext.Session.GetString("Role");
                        if (currentRole == "Admin")
                        {
                            TempData["SuccessMessage"] = "User created successfully";
                            return RedirectToAction("Index", "Students");
                        }

                        // Auto-login & Log Activity for new user (if not created by Admin)
                        HttpContext.Session.SetString("StudentUser", email);
                        HttpContext.Session.SetString("Role", role);
                        HttpContext.Session.SetString("PinVerified", "false");

                        _userActivityService.LogVisit(email, "/Account/VerifyOtp"); // Log Creation Visit

                        TempData["SuccessMessage"] = "Account created successfully";
                        return RedirectToAction("Dashboard", "Home");
                    }

                    // For Login flow (if OTP is used for login)
                    HttpContext.Session.SetString("StudentUser", email);
                    HttpContext.Session.SetString("Role", "Private");
                    HttpContext.Session.SetString("PinVerified", "false");
                    _userActivityService.LogVisit(email, "/Account/Login (OTP)");

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
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("StudentUser")))
            {
                return RedirectToAction("Dashboard", "Home");
            }
            ModelState.Clear();
            return View(new LoginViewModel());
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (_authService.ValidateUser(model.Email!, model.Password!, out string? error, out Student? student))
                {
                    // Login successful
                    HttpContext.Session.SetString("StudentUser", model.Email!);
                    HttpContext.Session.SetString("Role", student?.Role ?? "Private");
                    HttpContext.Session.SetString("PinVerified", "false"); // PIN required after login
                    _userActivityService.LogVisit(model.Email!, "/Account/Login");
                    return RedirectToAction("Dashboard", "Home");
                }

                // If check fails
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
            return View(model); // Expecting LoginViewModel
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Clear all session data
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
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
                ViewBag.ErrorMessage = "Password reset is currently unavailable. Please contact support.";
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            return RedirectToAction("Login");
        }

        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            return RedirectToAction("Login");
        }
    }
}
