using JsonCrudApp.Models;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;

namespace JsonCrudApp.Services
{
    /// <summary>
    /// Service to handle authentication logic using file-based credentials with password hashing.
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

        private string JsonFileName
        {
            get { return Path.Combine(_webHostEnvironment.ContentRootPath, "Data", "admin_credentials.json"); }
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
            // If stored password is empty or null
            if (string.IsNullOrEmpty(storedPassword)) return false;

            // Check if it's a hash (SHA256 Base64 usually ends with = and is certain length)
            // or just try hashing the entered one and comparing.
            // If the user wants "show #", we assume they want it hashed.

            string hashedEntered = HashPassword(enteredPassword);

            // Equality check (Case sensitive)
            if (hashedEntered == storedPassword) return true;

            // Fallback: Check if stored is plain text (for backward compatibility)
            return enteredPassword == storedPassword;
        }

        public List<AdminUser> GetAdminCredentials()
        {
            if (!File.Exists(JsonFileName))
            {
                return new List<AdminUser>();
            }

            try
            {
                var json = File.ReadAllText(JsonFileName);
                if (json.Trim().StartsWith("["))
                {
                    return JsonSerializer.Deserialize<List<AdminUser>>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<AdminUser>();
                }
                else
                {
                    var singleUser = JsonSerializer.Deserialize<AdminUser>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return singleUser != null ? new List<AdminUser> { singleUser } : new List<AdminUser>();
                }
            }
            catch
            {
                return new List<AdminUser>();
            }
        }

        public bool RegisterUser(string email, string password)
        {
            var users = GetAdminCredentials();
            if (users.Any(u => u.Email == email))
            {
                return false;
            }

            // Strict Role Assignment
            string role = (email == "charmimarakana@gmail.com") ? "Admin" : "User";

            // Store hashed password
            users.Add(new AdminUser { Email = email, Password = HashPassword(password), Role = role });

            var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });

            var directory = Path.GetDirectoryName(JsonFileName);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory!);
            }

            File.WriteAllText(JsonFileName, json);
            return true;
        }

        public bool RegisterStudent(string email, string password, string name = "New Student", int age = 18, string course = "General")
        {
            if (UserExists(email)) return false;

            // Strict Role Assignment
            string role = (email == "charmimarakana@gmail.com") ? "Admin" : "User";

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
            var users = GetAdminCredentials();
            if (users.Any(u => u.Email == email)) return true;

            var students = _studentService.GetStudents();
            return students.Any(s => s.Email == email);
        }

        public AdminUser? GetUserByEmail(string email)
        {
            var users = GetAdminCredentials();
            return users.FirstOrDefault(u => u.Email == email);
        }

        public string GenerateResetToken(string email)
        {
            var users = GetAdminCredentials();
            var user = users.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                var token = Guid.NewGuid().ToString("N");
                user.ResetToken = token;
                user.ResetTokenExpiry = DateTime.Now.AddMinutes(10);

                var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(JsonFileName, json);
                return token;
            }
            return string.Empty;
        }

        public bool ValidateResetToken(string email, string token)
        {
            var users = GetAdminCredentials();
            var user = users.FirstOrDefault(u => u.Email == email);

            if (user != null && user.ResetToken == token && user.ResetTokenExpiry > DateTime.Now)
            {
                return true;
            }
            return false;
        }

        public bool ResetPassword(string email, string newPassword, string token)
        {
            var users = GetAdminCredentials();
            var user = users.FirstOrDefault(u => u.Email == email);
            if (user != null && user.ResetToken == token && user.ResetTokenExpiry > DateTime.Now)
            {
                user.Password = HashPassword(newPassword);
                user.AccessFailedCount = 0; // Unlock on reset
                user.ResetToken = null; // Clear token after use
                user.ResetTokenExpiry = null;

                var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(JsonFileName, json);
                return true;
            }
            return false;
        }

        public void IncrementAccessFailedCount(string email)
        {
            var users = GetAdminCredentials();
            var user = users.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                user.AccessFailedCount++;
                var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(JsonFileName, json);
            }
        }

        public void ResetAccessFailedCount(string email)
        {
            var users = GetAdminCredentials();
            var user = users.FirstOrDefault(u => u.Email == email);
            if (user != null && user.AccessFailedCount > 0)
            {
                user.AccessFailedCount = 0;
                var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(JsonFileName, json);
            }
        }

        public bool ValidateAdmin(string email, string password, out string? errorMessage)
        {
            errorMessage = null;
            var users = GetAdminCredentials();
            var user = users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                errorMessage = "Admin account not found.";
                return false;
            }

            if (user.AccessFailedCount >= 3)
            {
                errorMessage = "Account locked due to 3 failed attempts. Please use Forgot Password to reset.";
                return false;
            }

            if (VerifyPassword(password, user.Password!))
            {
                bool needSave = false;
                if (user.AccessFailedCount > 0)
                {
                    user.AccessFailedCount = 0;
                    needSave = true;
                }

                // Strict Role Enforcement on Login
                string correctRole = (email == "charmimarakana@gmail.com") ? "Admin" : "User";
                if (user.Role != correctRole)
                {
                    user.Role = correctRole;
                    needSave = true;
                }

                if (needSave)
                {
                    var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(JsonFileName, json);
                }
                return true;
            }
            else
            {
                user.AccessFailedCount++;
                var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(JsonFileName, json);
                errorMessage = $"Invalid credentials. Attempt {user.AccessFailedCount} of 3.";
                if (user.AccessFailedCount >= 3)
                {
                    errorMessage = "Account locked. Please reset your password.";
                }
                return false;
            }
        }

        public bool ValidateStudent(string email, string password, out string? errorMessage)
        {
            errorMessage = null;
            var students = _studentService.GetStudents().ToList();
            var student = students.FirstOrDefault(s => s.Email == email);

            if (student == null)
            {
                errorMessage = "Student account not found (unregistered email).";
                return false;
            }

            if (VerifyPassword(password, student.Password!))
            {
                // Strict Role Enforcement on Login
                string correctRole = (email == "charmimarakana@gmail.com") ? "Admin" : "User";
                if (student.Role != correctRole)
                {
                    student.Role = correctRole;
                    _studentService.UpdateStudent(student);
                }
                return true;
            }

            errorMessage = "Incorrect password.";
            return false;
        }

        [Obsolete("Use ValidateAdmin or ValidateStudent instead")]
        public bool ValidateUser(string email, string password, out string? errorMessage)
        {
            if (ValidateAdmin(email, password, out errorMessage)) return true;
            return ValidateStudent(email, password, out errorMessage);
        }
    }
}
