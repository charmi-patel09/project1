using JsonCrudApp.Models;
using JsonCrudApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace JsonCrudApp.Controllers
{
    // [AuthorizeRole("Admin")] - Removed as per requirement
    public class StudentsController : BaseController
    {
        private readonly JsonFileStudentService _studentService;
        private readonly AuthService _authService;
        private readonly OtpService _otpService;
        private readonly EmailService _emailService;
        private readonly UserActivityService _userActivityService;
        private readonly UserWidgetService _userWidgetService;
        private readonly RoleWidgetService _roleWidgetService;

        public StudentsController(JsonFileStudentService studentService, AuthService authService, OtpService otpService, EmailService emailService, UserActivityService userActivityService, UserWidgetService userWidgetService, RoleWidgetService roleWidgetService)
        {
            _studentService = studentService;
            _authService = authService;
            _otpService = otpService;
            _emailService = emailService;
            _userActivityService = userActivityService;
            _userWidgetService = userWidgetService;
            _roleWidgetService = roleWidgetService;
        }

        public IActionResult DailyActivity(string filter = "Today", DateTime? customDate = null)
        {
            // Fallback to today if customDate is not provided but filter is 'Custom'
            if (filter == "Custom" && customDate == null) customDate = DateTime.UtcNow.Date;

            var report = _userActivityService.GetReport(filter, customDate);
            ViewBag.CurrentFilter = filter;
            return View(report);
        }

        [HttpGet]
        public IActionResult GetUserActivityDetails(string email, string filter = "Today", DateTime? customDate = null)
        {
            // Fallback to today if customDate is not provided but filter is 'Custom'
            if (filter == "Custom" && customDate == null) customDate = DateTime.UtcNow.Date;

            var visits = _userActivityService.GetUserVisits(email, filter, customDate);
            return Json(visits);
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Role") != UserRole.Admin) return RedirectToAction("AccessDenied", "Account");
            // Restrictions removed as per requirement
            return View(_studentService.GetStudents());
        }

        [HttpGet]
        public IActionResult CreateAdmin()
        {
            if (HttpContext.Session.GetString("Role") != UserRole.Admin) return RedirectToAction("AccessDenied", "Account");
            // Restrictions removed as per requirement
            // Ensure no residual session data leaks into the form
            HttpContext.Session.Remove("PendingAdminEmail");


            ModelState.Clear();
            return View(new Student { Email = null, Password = null, Role = UserRole.Admin });
        }

        [HttpPost]
        public IActionResult CreateAdmin(Student student)
        {
            if (HttpContext.Session.GetString("Role") != UserRole.Admin) return RedirectToAction("AccessDenied", "Account");
            // Restrictions removed as per requirement
            // ... existing validation logic ...

            // Only validate Email and Password for simplicity or use a specific ViewModel
            if (!string.IsNullOrEmpty(student.Email) && !string.IsNullOrEmpty(student.Password))
            {
                if (!_authService.UserExists(student.Email))
                {
                    // OTP Flow
                    string otp = _otpService.GenerateOtp();
                    DateTime expiry = DateTime.Now.AddMinutes(2);

                    _emailService.SendOtpEmail(student.Email, otp);

                    HttpContext.Session.SetString("PendingAdminEmail", student.Email);
                    HttpContext.Session.SetString("PendingAdminPassword", student.Password);
                    HttpContext.Session.SetString("PendingAdminName", student.Name ?? "Admin User");


                    HttpContext.Session.SetString("OtpPurpose", "AdminCreation");
                    HttpContext.Session.SetString("OtpCode", otp);
                    HttpContext.Session.SetString("OtpExpiry", expiry.ToString("O"));

                    if (!string.IsNullOrEmpty(student.SecurityPin))
                    {
                        HttpContext.Session.SetString("PendingSecurityPin", student.SecurityPin);
                    }

                    return RedirectToAction("VerifyAdminOtp");
                }
                ModelState.AddModelError("Email", "User already exists");
            }
            else
            {
                ModelState.AddModelError("", "Email and Password are required");
            }
            return View(student);
        }

        [HttpGet]
        public IActionResult VerifyAdminOtp()
        {
            if (HttpContext.Session.GetString("Role") != UserRole.Admin) return RedirectToAction("AccessDenied", "Account");
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("PendingAdminEmail")))
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost]
        public IActionResult VerifyAdminOtp(string otp)
        {
            if (HttpContext.Session.GetString("Role") != UserRole.Admin) return RedirectToAction("AccessDenied", "Account");
            string storedOtp = HttpContext.Session.GetString("OtpCode") ?? "";
            string expiryStr = HttpContext.Session.GetString("OtpExpiry") ?? "";
            string email = HttpContext.Session.GetString("PendingAdminEmail") ?? "";
            string password = HttpContext.Session.GetString("PendingAdminPassword") ?? "";
            string name = HttpContext.Session.GetString("PendingAdminName") ?? "Admin";
            string purpose = HttpContext.Session.GetString("OtpPurpose") ?? "";

            if (DateTime.TryParse(expiryStr, out DateTime expiry) && purpose == "AdminCreation")
            {
                if (_otpService.IsValid(otp, storedOtp, expiry))
                {
                    // Create Admin
                    var newAdmin = new Student
                    {
                        Email = email,
                        Password = password,
                        Name = name,
                        Age = 25,
                        Course = "Administration",
                        Role = UserRole.Admin
                    };

                    string? pin = HttpContext.Session.GetString("PendingSecurityPin");
                    if (!string.IsNullOrEmpty(pin))
                    {
                        newAdmin.SecurityPinHash = _authService.HashPin(pin);
                        newAdmin.IsSecurityEnabled = true;
                        HttpContext.Session.Remove("PendingSecurityPin");
                    }

                    _authService.RegisterStudent(newAdmin);

                    // Cleanup
                    HttpContext.Session.Remove("OtpCode");
                    HttpContext.Session.Remove("OtpExpiry");
                    HttpContext.Session.Remove("PendingAdminEmail");
                    HttpContext.Session.Remove("PendingAdminPassword");
                    HttpContext.Session.Remove("PendingAdminName");
                    HttpContext.Session.Remove("OtpPurpose");

                    TempData["SuccessMessage"] = "Admin created successfully";
                    return RedirectToAction("Index");
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
                ViewBag.ErrorMessage = "Invalid Session";
            }
            return View();
        }

        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("Role") != UserRole.Admin) return RedirectToAction("AccessDenied", "Account");

            // Ensure no residual session data leaks into the form
            HttpContext.Session.Remove("PendingUserEmail");
            HttpContext.Session.Remove("PendingUserName");

            ViewData["Title"] = "Create New User";
            ModelState.Clear();
            return View(new Student { Email = null, Password = null, Role = UserRole.Visitor });
        }

        [HttpPost]
        public IActionResult Create(Student student)
        {
            if (HttpContext.Session.GetString("Role") != UserRole.Admin) return RedirectToAction("AccessDenied", "Account");

            // Default values for fields hidden in the form
            if (student.Age == null)
            {
                student.Age = 18;
                ModelState.Remove("Age"); // Remove validation error for Age
            }

            if (student.Role != null && student.Role.Equals("normal", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("Role", "The selected role is no longer available.");
                return View(student);
            }

            // Map Role for Course logic
            if (student.Role == UserRole.Admin) student.Course = "Administration";
            else if (string.IsNullOrEmpty(student.Course)) student.Course = "General";

            if (ModelState.IsValid)
            {
                if (!_authService.UserExists(student.Email!))
                {
                    // Enforce role-widget mapping in backend by role assignment
                    // (Permissions are dynamically fetched from RoleWidgetService at login/session)

                    // Initiate OTP Flow
                    string otp = _otpService.GenerateOtp();
                    DateTime expiry = DateTime.Now.AddMinutes(2); // 2 minutes expiry 

                    _emailService.SendOtpEmail(student.Email!, otp);

                    // Store temp session
                    HttpContext.Session.SetString("PendingUserEmail", student.Email ?? "");
                    HttpContext.Session.SetString("PendingUserPassword", student.Password ?? "");
                    HttpContext.Session.SetString("PendingUserName", student.Name ?? "");
                    HttpContext.Session.SetString("PendingUserAge", (student.Age ?? 18).ToString());
                    HttpContext.Session.SetString("PendingUserCourse", student.Course ?? "General");
                    HttpContext.Session.SetString("PendingUserRole", student.Role ?? UserRole.Visitor);

                    if (!string.IsNullOrEmpty(student.SecurityPin))
                    {
                        HttpContext.Session.SetString("PendingSecurityPin", student.SecurityPin);
                    }

                    HttpContext.Session.SetString("OtpPurpose", "StudentCreation");
                    HttpContext.Session.SetString("OtpCode", otp);
                    HttpContext.Session.SetString("OtpExpiry", expiry.ToString("O"));

                    return RedirectToAction("VerifyOtp", "Account");
                }
                ModelState.AddModelError("Email", "User with this email already exists");
            }
            return View(student);
        }

        public IActionResult Edit(int id)
        {
            if (HttpContext.Session.GetString("Role") != UserRole.Admin) return RedirectToAction("AccessDenied", "Account");

            var student = _studentService.GetStudentById(id);
            if (student == null)
            {
                return NotFound();
            }

            // Load current widgets for the form - Strict Role-Based (derived from current role)
            var roleWidgets = _roleWidgetService.GetWidgetsByRole(student.Role);
            ViewBag.WidgetPermissions = roleWidgets?.AllowedWidgets;

            return View(student);
        }

        [HttpPost]
        public IActionResult Edit(Student student)
        {
            if (HttpContext.Session.GetString("Role") != UserRole.Admin) return RedirectToAction("AccessDenied", "Account");

            if (ModelState.IsValid)
            {
                // Enforce role-widget mapping in backend by role assignment
                // (Permissions are dynamically fetched from RoleWidgetService at session start)

                var original = _studentService.GetStudentById(student.Id);
                if (original == null)
                {
                    TempData["ErrorMessage"] = "User not found";
                    return RedirectToAction(nameof(Index));
                }

                // Update properties
                original.Name = student.Name;
                original.Email = student.Email; // Identity stable via Id

                if (!string.IsNullOrEmpty(student.Password) && student.Password != original.Password)
                {
                    original.Password = _authService.HashPassword(student.Password);
                }

                original.Age = student.Age;
                original.Role = student.Role;

                if (!string.IsNullOrEmpty(student.SecurityPin))
                {
                    original.SecurityPinHash = _authService.HashPin(student.SecurityPin);
                    original.IsSecurityEnabled = true;
                }

                if (student.Role == UserRole.Admin) original.Course = "Administration";
                else if (string.IsNullOrEmpty(original.Course)) original.Course = "General";

                try
                {
                    _studentService.UpdateStudent(original);
                    TempData["SuccessMessage"] = "User updated successfully";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error updating user: " + ex.Message);
                }
            }

            return View(student);
        }

        public IActionResult Details(int id)
        {
            if (HttpContext.Session.GetString("Role") != UserRole.Admin) return RedirectToAction("AccessDenied", "Account");
            // Restrictions removed
            var student = _studentService.GetStudentById(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        public IActionResult Delete(int id)
        {
            if (HttpContext.Session.GetString("Role") != UserRole.Admin) return RedirectToAction("AccessDenied", "Account");
            // Restrictions removed
            var student = _studentService.GetStudentById(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            if (HttpContext.Session.GetString("Role") != UserRole.Admin) return RedirectToAction("AccessDenied", "Account");
            // Restrictions removed
            _studentService.DeleteStudent(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
