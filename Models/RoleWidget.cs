using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JsonCrudApp.Models
{
    public class RoleWidget
    {
        [Key]
        public string Role { get; set; } = string.Empty;
        public string AllowedWidgets { get; set; } = string.Empty; // Comma-separated widget IDs
    }
}
