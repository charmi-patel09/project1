// ... (imports remain)
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

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string enteredPassword, string storedPassword)
        {
            if (string.IsNullOrEmpty(storedPassword)) return false;

            string hashedEntered = HashPassword(enteredPassword);

            if (hashedEntered == storedPassword) return true;

            return enteredPassword == storedPassword;
        }

        public bool RegisterStudent(string email, string password, string name = "New Student", int age = 18, string course = "General", string role = "User")
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
    }
}
