using System.ComponentModel.DataAnnotations;

namespace JsonCrudApp.Models
{
    public class AdminUser
    {
        [Required(ErrorMessage = "Required")]
        [EmailAddress(ErrorMessage = "InvalidEmail")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Required")]
        public string? Password { get; set; }

        public int AccessFailedCount { get; set; } = 0;

        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }
    }
}
