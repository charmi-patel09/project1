using JsonCrudApp.Models;

namespace JsonCrudApp.ViewModels
{
    public class TimeTrackerViewModel
    {
        public IEnumerable<TimeEntry> Entries { get; set; } = new List<TimeEntry>();

        public string Filter { get; set; } = "today"; // today, yesterday, week, all, custom
        public DateTime? CustomStartDate { get; set; }
        public DateTime? CustomEndDate { get; set; }

        // Summaries
        public TimeSpan TotalTimeTracked { get; set; }
        public Dictionary<string, TimeSpan> TaskBreakdown { get; set; } = new Dictionary<string, TimeSpan>();
    }
}
