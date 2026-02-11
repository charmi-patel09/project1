using System.ComponentModel.DataAnnotations;

namespace JsonCrudApp.Models
{
    public class SignUpViewModel
    {
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password and confirm password do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [StringLength(6, MinimumLength = 4, ErrorMessage = "PIN must be 4-6 digits")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "PIN must be numeric")]
        public string? SecurityPin { get; set; }

        [Compare("SecurityPin", ErrorMessage = "PINs do not match")]
        public string? ConfirmSecurityPin { get; set; }
    }
}
