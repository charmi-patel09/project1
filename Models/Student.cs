using System.ComponentModel.DataAnnotations;

namespace JsonCrudApp.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "field is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "field is required")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "field is required")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "field is required")]
        public int? Age { get; set; }

        [Required(ErrorMessage = "field is required")]
        public string? Course { get; set; }
    }
}
