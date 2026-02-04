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

        public StudentsController(JsonFileStudentService studentService, AuthService authService, OtpService otpService, EmailService emailService, UserActivityService userActivityService)
        {
            _studentService = studentService;
            _authService = authService;
            _otpService = otpService;
            _emailService = emailService;
            _userActivityService = userActivityService;
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
            if (HttpContext.Session.GetString("Role") != "Admin") return RedirectToAction("AccessDenied", "Account");
            // Restrictions removed as per requirement
            return View(_studentService.GetStudents());
        }

        [HttpGet]
        public IActionResult CreateAdmin()
        {
            if (HttpContext.Session.GetString("Role") != "Admin") return RedirectToAction("AccessDenied", "Account");
            // Restrictions removed as per requirement
            ModelState.Clear();
            return View(new Student());
        }

        [HttpPost]
        public IActionResult CreateAdmin(Student student)
        {
            if (HttpContext.Session.GetString("Role") != "Admin") return RedirectToAction("AccessDenied", "Account");
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
            if (HttpContext.Session.GetString("Role") != "Admin") return RedirectToAction("AccessDenied", "Account");
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("PendingAdminEmail")))
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost]
        public IActionResult VerifyAdminOtp(string otp)
        {
            if (HttpContext.Session.GetString("Role") != "Admin") return RedirectToAction("AccessDenied", "Account");
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
                    _authService.RegisterStudent(email, password, name, 25, "Administration", "Admin");

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
        public IActionResult Create(string role = "User")
        {
            if (HttpContext.Session.GetString("Role") != "Admin") return RedirectToAction("AccessDenied", "Account");

            ViewBag.TargetRole = role;
            ViewData["Title"] = role == "Admin" ? "Create Admin User" : "Create New User";

            ModelState.Clear();
            return View(new Student { Role = role });
        }

        [HttpPost]
        public IActionResult Create(Student student)
        {
            if (HttpContext.Session.GetString("Role") != "Admin") return RedirectToAction("AccessDenied", "Account");

            if (string.IsNullOrEmpty(student.Role)) student.Role = "User";

            if (ModelState.IsValid)
            {
                if (!_authService.UserExists(student.Email!))
                {
                    // Initiate OTP Flow
                    string otp = _otpService.GenerateOtp();
                    DateTime expiry = DateTime.Now.AddMinutes(2);

                    _emailService.SendOtpEmail(student.Email!, otp);

                    // Store temp session
                    HttpContext.Session.SetString("PendingUserEmail", student.Email ?? "");
                    HttpContext.Session.SetString("PendingUserPassword", student.Password ?? "");
                    HttpContext.Session.SetString("PendingUserName", student.Name ?? "");
                    HttpContext.Session.SetString("PendingUserAge", student.Age.ToString() ?? "");
                    HttpContext.Session.SetString("PendingUserCourse", student.Course ?? string.Empty);

                    // Set Role for Verification
                    HttpContext.Session.SetString("PendingUserRole", student.Role);

                    HttpContext.Session.SetString("OtpPurpose", "StudentCreation");
                    HttpContext.Session.SetString("OtpCode", otp);
                    HttpContext.Session.SetString("OtpExpiry", expiry.ToString("O"));

                    return RedirectToAction("VerifyOtp", "Account");
                }
                ModelState.AddModelError("Email", "User with this email already exists");
            }

            ViewBag.TargetRole = student.Role;
            return View(student);
        }

        public IActionResult Edit(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin") return RedirectToAction("AccessDenied", "Account");
            // Restrictions removed
            var student = _studentService.GetStudentById(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        [HttpPost]
        public IActionResult Edit(Student student)
        {
            if (HttpContext.Session.GetString("Role") != "Admin") return RedirectToAction("AccessDenied", "Account");

            if (ModelState.IsValid)
            {
                var original = _studentService.GetStudentById(student.Id);
                if (original == null) return NotFound();

                // Update properties
                original.Name = student.Name;
                original.Email = student.Email;
                original.Password = student.Password;
                original.Age = student.Age;

                // Preserve Role (Crucial to prevent demoting Admins)
                // original.Role remains unchanged

                // Note: Course is preserved from original as it's removed from form

                _studentService.UpdateStudent(original);
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        public IActionResult Details(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin") return RedirectToAction("AccessDenied", "Account");
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
            if (HttpContext.Session.GetString("Role") != "Admin") return RedirectToAction("AccessDenied", "Account");
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
            if (HttpContext.Session.GetString("Role") != "Admin") return RedirectToAction("AccessDenied", "Account");
            // Restrictions removed
            _studentService.DeleteStudent(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
