using JsonCrudApp.Models;
using JsonCrudApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace JsonCrudApp.Controllers
{
    public class StudentsController : BaseController
    {
        private readonly JsonFileStudentService _studentService;
        private readonly AuthService _authService;
        private readonly OtpService _otpService;
        private readonly EmailService _emailService;
        private readonly Microsoft.Extensions.Localization.IStringLocalizer<SharedResource> _localizer;

        public StudentsController(JsonFileStudentService studentService, AuthService authService, OtpService otpService, EmailService emailService, Microsoft.Extensions.Localization.IStringLocalizer<SharedResource> localizer)
        {
            _studentService = studentService;
            _authService = authService;
            _otpService = otpService;
            _emailService = emailService;
            _localizer = localizer;
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
                ModelState.AddModelError("Email", _localizer["UserWithEmailExists"]);
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
