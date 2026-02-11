using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.Linq;
using JsonCrudApp.Models;
using System.Security.Cryptography;
using System.Text;

namespace JsonCrudApp.Services
{
    /// <summary>
    /// Service to handle authentication logic.
    /// </summary>
    public class AuthService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly JsonFileStudentService _studentService;

        public AuthService(IWebHostEnvironment webHostEnvironment, JsonFileStudentService studentService)
        {
            _webHostEnvironment = webHostEnvironment;
            _studentService = studentService;
        }

        public string HashPassword(string password)
        {
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }

        public bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }

        public bool RegisterStudent(string email, string password, string name = "New Student", int age = 18, string course = "General", string role = "Private")
        {
            if (UserExists(email)) return false;

            var student = new Student
            {
                Email = email,
                Password = HashPassword(password),
                Name = name,
                Age = age,
                Course = course,
                Role = role
            };
            _studentService.AddStudent(student);
            return true;
        }

        public bool RegisterStudent(Student student)
        {
            if (UserExists(student.Email!)) return false;

            // Ensure password is hashed if it isn't already (simple check, or just re-hash)
            // Assuming input student has raw password
            student.Password = HashPassword(student.Password!);

            _studentService.AddStudent(student);
            return true;
        }

        public bool UserExists(string email)
        {
            var students = _studentService.GetStudents();
            return students.Any(s => s.Email == email);
        }

        public bool AnyAdminExists()
        {
            var students = _studentService.GetStudents();
            return students.Any(s => s.Role == "Admin");
        }

        // Removed Admin-specific methods: GetAdminCredentials, RegisterUser (Admin), GetUserByEmail (Admin), GenerateResetToken, ValidateResetToken, ResetPassword, IncrementAccessFailedCount, ResetAccessFailedCount, ValidateAdmin

        public bool ValidateStudent(string email, string password, out string? errorMessage, out Student? student)
        {
            errorMessage = null;
            student = null;
            var students = _studentService.GetStudents().ToList();
            var foundStudent = students.FirstOrDefault(s => s.Email == email);

            if (foundStudent == null)
            {
                errorMessage = "Account not found.";
                return false;
            }

            if (VerifyPassword(password, foundStudent.Password!))
            {
                student = foundStudent;
                return true;
            }

            errorMessage = "Incorrect password.";
            return false;
        }

        public bool ValidateUser(string email, string password, out string? errorMessage)
        {
            return ValidateStudent(email, password, out errorMessage, out _);
        }

        public bool ValidateUser(string email, string password, out string? errorMessage, out Student? student)
        {
            return ValidateStudent(email, password, out errorMessage, out student);
        }

        // Retain Reset Logic placeholders if needed by Controller, or Controller will implement differently.
        // For now, removing them means AccountController calls to them must be removed.
        public string GenerateResetToken(string email) => string.Empty;
        public bool ValidateResetToken(string email, string token) => false;
        public bool ResetPassword(string email, string newPassword, string token) => false;

        public bool SetPin(string email, string pin)
        {
            var student = _studentService.GetStudents().FirstOrDefault(s => s.Email == email);
            if (student == null) return false;

            student.SecurityPinHash = HashPassword(pin);
            student.IsSecurityEnabled = true;
            _studentService.UpdateStudent(student);
            return true;
        }

        public bool VerifyPin(string email, string pin)
        {
            var student = _studentService.GetStudents().FirstOrDefault(s => s.Email == email);
            if (student == null || !student.IsSecurityEnabled || string.IsNullOrEmpty(student.SecurityPinHash)) return false;

            return VerifyPassword(pin, student.SecurityPinHash);
        }

        public bool HasPin(string email)
        {
            var student = _studentService.GetStudents().FirstOrDefault(s => s.Email == email);
            return student != null && student.IsSecurityEnabled;
        }
    }
}
