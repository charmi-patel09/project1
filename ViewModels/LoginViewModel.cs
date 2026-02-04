using System.ComponentModel.DataAnnotations;

namespace JsonCrudApp.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Required")]
        [EmailAddress(ErrorMessage = "InvalidEmail")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Required")]
        public string? Password { get; set; }
    }
}
