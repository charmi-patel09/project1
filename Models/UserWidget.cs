using System.ComponentModel.DataAnnotations;

namespace JsonCrudApp.Models
{
    public class UserWidget
    {
        [Key]
        public string Email { get; set; } = string.Empty;
        public string AllowedWidgets { get; set; } = string.Empty; // Overrides role-based defaults
    }
}
