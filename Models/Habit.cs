using System.ComponentModel.DataAnnotations;

namespace JsonCrudApp.Models
{
    public class Habit
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string UserEmail { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string FrequencyType { get; set; } = "Daily"; // Daily, Custom

        public List<string> CustomDays { get; set; } = new List<string>(); // "Monday", "Tuesday", etc.

        public DateTime StartDate { get; set; } = DateTime.Today;

        public List<DateTime> CompletedDates { get; set; } = new List<DateTime>();

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string Goal { get; set; } = string.Empty;
    }
}
