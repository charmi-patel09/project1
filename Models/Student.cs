using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace JsonCrudApp.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Required")]
        [EmailAddress(ErrorMessage = "InvalidEmail")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Required")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Required")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Required")]
        public int? Age { get; set; }

        public string? Course { get; set; }

        public string Role { get; set; } = "private";


        [RegularExpression(@"^\d{4,6}$", ErrorMessage = "PIN must be 4-6 digits")]
        public string? SecurityPin { get; set; }

        public string? SecurityPinHash { get; set; }
        public bool IsSecurityEnabled { get; set; } = false;
    }
}
