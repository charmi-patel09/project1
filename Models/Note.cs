using System.ComponentModel.DataAnnotations;

namespace JsonCrudApp.Models
{
    public class Note
    {
        public int Id { get; set; }

        [Required]
        public string? UserEmail { get; set; }

        [Required(ErrorMessage = "Required")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Required")]
        public string? Description { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
