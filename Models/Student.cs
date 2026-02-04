using System.ComponentModel.DataAnnotations;

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

        public string Role { get; set; } = "User";
    }
}
