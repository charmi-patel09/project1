using System.ComponentModel.DataAnnotations;

namespace JsonCrudApp.Models
{
    public class Goal
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Goal title is required")]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Category { get; set; } = "General"; // Personal, Work, Health, Finance, etc.

        public string Priority { get; set; } = "Medium"; // Low, Medium, High

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(7);

        public List<Milestone> Milestones { get; set; } = new List<Milestone>();

        public List<DailyTask> DailyTasks { get; set; } = new List<DailyTask>();

        public double Progress { get; set; } = 0; // 0 to 100

        public string Status { get; set; } = "Not Started"; // Not Started, In Progress, Completed, Overdue

        public string ScheduleStatus { get; set; } = "On Track"; // On Track, Behind, Completed

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public int Streak { get; set; } = 0;

        public DateTime? LastModified { get; set; }
    }

    public class Milestone
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public bool IsCompleted { get; set; } = false;
        public DateTime? CompletedAt { get; set; }
    }

    public class DailyTask
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public int UserId { get; set; }
        public string GoalId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public bool IsCompleted { get; set; } = false;
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
