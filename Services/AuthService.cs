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

            // Store hashed password
            users.Add(new AdminUser { Email = email, Password = HashPassword(password) });

            var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });

            var directory = Path.GetDirectoryName(JsonFileName);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory!);
            }

            File.WriteAllText(JsonFileName, json);
            return true;
        }

        public bool UserExists(string email)
        {
            var users = GetAdminCredentials();
            return users.Any(u => u.Email == email);
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

        public bool ValidateUser(string email, string password, out string? errorMessage)
        {
            errorMessage = null;
            bool isValid = false;
            bool needsUpgrade = false;

            // Check Registered Users
            var users = GetAdminCredentials();
            var user = users.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                if (user.AccessFailedCount >= 3)
                {
                    errorMessage = "Account locked due to 3 failed attempts. Please use Forgot Password to reset.";
                    return false;
                }

                if (VerifyPassword(password, user.Password!))
                {
                    isValid = true;
                    if (user.AccessFailedCount > 0) user.AccessFailedCount = 0;

                    // If it was plain text, upgrade it to hash
                    if (user.Password != HashPassword(password))
                    {
                        user.Password = HashPassword(password);
                        needsUpgrade = true;
                    }
                }
                else
                {
                    user.AccessFailedCount++;
                    needsUpgrade = true; // Still need to save the incremented count
                    errorMessage = $"Invalid credentials. Attempt {user.AccessFailedCount} of 3.";
                    if (user.AccessFailedCount >= 3)
                    {
                        errorMessage = "Account locked. Please reset your password.";
                    }
                }
            }

            if (needsUpgrade || isValid) // Always save if we incremented or reset
            {
                var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(JsonFileName, json);
            }

            if (isValid) return true;

            // Check Students (Assuming students also have passwords)
            var students = _studentService.GetStudents();
            var student = students.FirstOrDefault(s => s.Email == email);
            if (student != null)
            {
                if (VerifyPassword(password, student.Password!))
                {
                    return true;
                }
            }

            if (errorMessage == null) errorMessage = "Invalid email or password";
            return false;
        }
    }
}
