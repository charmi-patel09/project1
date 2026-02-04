using JsonCrudApp.Models;
using JsonCrudApp.Services;
using Microsoft.AspNetCore.Mvc;
using JsonCrudApp.Attributes;

namespace JsonCrudApp.Controllers
{
    [AuthorizeRole("Admin")]
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
            if (filter == "Custom" && customDate == null) customDate = DateTime.Today;

            var report = _userActivityService.GetReport(filter, customDate);
            ViewBag.CurrentFilter = filter;
            return View(report);
        }

        public IActionResult Index()
        {
            return View(_studentService.GetStudents());
        }

        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult Create()
        {
            ModelState.Clear();
            return View(new Student());
        }

        [HttpPost]
        public IActionResult Create(Student student)
        {
            if (ModelState.IsValid)
            {
                if (!_authService.UserExists(student.Email!))
                {
                    // Initiate OTP Flow for Student Creation
                    string otp = _otpService.GenerateOtp();
                    DateTime expiry = DateTime.Now.AddMinutes(2);

                    _emailService.SendOtpEmail(student.Email!, otp);

                    // Store temp session
                    HttpContext.Session.SetString("PendingUserEmail", student.Email ?? "");
                    HttpContext.Session.SetString("PendingUserPassword", student.Password ?? "");
                    HttpContext.Session.SetString("PendingUserName", student.Name ?? "");
                    HttpContext.Session.SetString("PendingUserAge", student.Age.ToString());
                    HttpContext.Session.SetString("PendingUserCourse", student.Course ?? "");
                    HttpContext.Session.SetString("PendingUserType", "Student");
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
            if (ModelState.IsValid)
            {
                _studentService.UpdateStudent(student);
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        public IActionResult Details(int id)
        {
            var student = _studentService.GetStudentById(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        public IActionResult Delete(int id)
        {
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
            _studentService.DeleteStudent(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
