using System.ComponentModel.DataAnnotations;

namespace JsonCrudApp.Models
{
    public class TimeEntry
    {
        public int Id { get; set; }

        public string? UserEmail { get; set; }

        [Required]
        public string? TaskName { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public long DurationSeconds { get; set; }
    }
}
