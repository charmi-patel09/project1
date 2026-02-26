using System;
using Microsoft.AspNetCore.Mvc;
using JsonCrudApp.Services;
using JsonCrudApp.Models;
using JsonCrudApp.ViewModels;
using Microsoft.AspNetCore.Http;
using System.Linq;

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

        private readonly RoleWidgetService _roleWidgetService;
        private readonly UserWidgetService _userWidgetService;

        public AccountController(AuthService authService, EmailService emailService, OtpService otpService, UserActivityService userActivityService, RoleWidgetService roleWidgetService, UserWidgetService userWidgetService)
        {
            _authService = authService;
            _emailService = emailService;
            _otpService = otpService;
            _userActivityService = userActivityService;
            _roleWidgetService = roleWidgetService;
            _userWidgetService = userWidgetService;
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
                    if (!string.IsNullOrEmpty(model.SecurityPin))
                    {
                        HttpContext.Session.SetString("PendingSecurityPin", model.SecurityPin);
                    }

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

                    if (purpose == "Registration" || purpose == "StudentCreation")
                    {
                        Student newStudent;
                        string roleValue;

                        if (purpose == "Registration")
                        {
                            roleValue = UserRole.Visitor;
                            newStudent = new Student
                            {
                                Email = email,
                                Password = password,
                                Name = "New Student",
                                Age = 18,
                                Course = "General",
                                Role = roleValue
                            };
                        }
                        else // StudentCreation
                        {
                            string name = HttpContext.Session.GetString("PendingUserName") ?? "New Student";
                            string ageStr = HttpContext.Session.GetString("PendingUserAge") ?? "18";
                            string course = HttpContext.Session.GetString("PendingUserCourse") ?? "General";
                            roleValue = HttpContext.Session.GetString("PendingUserRole") ?? UserRole.@private;

                            newStudent = new Student
                            {
                                Email = email,
                                Password = password,
                                Name = name,
                                Age = int.Parse(ageStr),
                                Course = course,
                                Role = roleValue
                            };

                            // Clear creation-specific data
                            HttpContext.Session.Remove("PendingUserName");
                            HttpContext.Session.Remove("PendingUserAge");
                            HttpContext.Session.Remove("PendingUserCourse");
                            HttpContext.Session.Remove("PendingUserRole");
                        }

                        string? pin = HttpContext.Session.GetString("PendingSecurityPin");
                        if (!string.IsNullOrEmpty(pin))
                        {
                            newStudent.SecurityPinHash = _authService.HashPin(pin);
                            newStudent.IsSecurityEnabled = true;
                            HttpContext.Session.Remove("PendingSecurityPin");
                        }

                        _authService.RegisterStudent(newStudent);

                        // Strict Role-Based Widget Assignment
                        var roleWidgets = _roleWidgetService.GetWidgetsByRole(roleValue);
                        if (roleWidgets != null)
                        {
                            HttpContext.Session.SetString("WidgetPermissions", roleWidgets.AllowedWidgets);
                        }

                        // Check if currently logged in as Admin (only relevant for StudentCreation)
                        string? currentRole = HttpContext.Session.GetString("Role");
                        if (currentRole == UserRole.Admin && purpose == "StudentCreation")
                        {
                            TempData["SuccessMessage"] = "User created successfully";
                            return RedirectToAction("Index", "Students");
                        }

                        // Auto-login & Log Activity
                        HttpContext.Session.SetInt32("UserId", newStudent.Id);
                        HttpContext.Session.SetString("StudentUser", email);
                        HttpContext.Session.SetString("Role", roleValue);
                        _userActivityService.LogVisit(email, "/Account/VerifyOtp");

                        TempData["SuccessMessage"] = "Account created successfully";
                        return RedirectToAction("Dashboard", "Home");
                    }

                    // For Login flow (if OTP is used for login)
                    var students = _authService.GetStudents();
                    var user = students.FirstOrDefault(s => s.Email == email);
                    if (user != null)
                    {
                        HttpContext.Session.SetInt32("UserId", user.Id);
                        HttpContext.Session.SetString("StudentUser", email);
                        HttpContext.Session.SetString("Role", user.Role);

                        var roleWidgets = _roleWidgetService.GetWidgetsByRole(user.Role);
                        if (roleWidgets != null)
                        {
                            HttpContext.Session.SetString("WidgetPermissions", roleWidgets.AllowedWidgets);
                        }

                        _userActivityService.LogVisit(email, "/Account/Login (OTP)");
                        return RedirectToAction("Dashboard", "Home");
                    }
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
                    if (student != null)
                    {
                        HttpContext.Session.SetInt32("UserId", student.Id);
                        HttpContext.Session.SetString("StudentUser", model.Email!);
                        HttpContext.Session.SetString("Role", student.Role);
                        HttpContext.Session.SetString("IsSecurityEnabled", student.IsSecurityEnabled.ToString().ToLower());

                        // Reset secure states on new login
                        HttpContext.Session.Remove("PinVerified");
                        HttpContext.Session.Remove("IsUnlocked");
                        // Also clear any previous widget unlock states
                        HttpContext.Session.Remove("Unlocked_habit-hub");
                        HttpContext.Session.Remove("Unlocked_pdf-hub");
                        HttpContext.Session.Remove("Unlocked_notes-hub");
                        HttpContext.Session.SetInt32("PinAttempts", 0);

                        // Strict Role-Based Widget Logic
                        var roleWidgets = _roleWidgetService.GetWidgetsByRole(student.Role);
                        if (roleWidgets != null)
                        {
                            HttpContext.Session.SetString("WidgetPermissions", roleWidgets.AllowedWidgets);
                        }
                    }

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

        [HttpPost]
        public IActionResult VerifyPin(string pin, string? targetWidget = null)
        {
            var userEmail = HttpContext.Session.GetString("StudentUser");
            if (string.IsNullOrEmpty(userEmail)) return Json(new { success = false, message = "Session expired" });

            var students = _authService.GetStudents();
            var user = students.FirstOrDefault(s => s.Email == userEmail);

            if (user == null) return Json(new { success = false, message = "User not found" });

            if (!user.IsSecurityEnabled)
            {
                if (!string.IsNullOrEmpty(targetWidget))
                {
                    HttpContext.Session.SetString($"Unlocked_{targetWidget}", "true");
                }
                return Json(new { success = true });
            }

            // Check attempts
            int attempts = HttpContext.Session.GetInt32("PinAttempts") ?? 0;
            if (attempts >= 5) return Json(new { success = false, message = "Too many attempts. Please try again later." });

            bool isCorrect = _authService.VerifyPin(pin, user.SecurityPinHash!);
            if (isCorrect)
            {
                if (!string.IsNullOrEmpty(targetWidget))
                {
                    HttpContext.Session.SetString($"Unlocked_{targetWidget}", "true");
                    // Also store in sessionStorage via JS after this returns
                }

                // Explicitly ensure global flags are NOT set
                HttpContext.Session.Remove("IsUnlocked");
                HttpContext.Session.Remove("PinVerified");

                HttpContext.Session.Remove("PinAttempts");
                return Json(new { success = true });
            }

            attempts++;
            HttpContext.Session.SetInt32("PinAttempts", attempts);
            return Json(new { success = false, message = $"Incorrect PIN. {5 - attempts} attempts remaining." });
        }

        [HttpPost]
        public IActionResult SetupSecurityPin([FromForm] string pin)
        {
            var userEmail = HttpContext.Session.GetString("StudentUser");
            if (string.IsNullOrEmpty(userEmail)) return Json(new { success = false, message = "Session expired" });

            if (_authService.SetSecurityPin(userEmail, pin))
            {
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Failed to update security settings." });
        }
    }
}
