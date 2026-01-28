using JsonCrudApp.Models;
using JsonCrudApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace JsonCrudApp.Controllers
{
    [Route("TimeTracker")]
    public class TimeTrackerController : BaseController
    {
        private readonly TimeTrackerService _timeTrackerService;

        public TimeTrackerController(TimeTrackerService timeTrackerService)
        {
            _timeTrackerService = timeTrackerService;
        }

        [Route("")]
        [Route("Index")]
        public IActionResult Index(string filter = "today", DateTime? startDate = null, DateTime? endDate = null)
        {
            var userEmail = GetCurrentUserEmail();
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            var allEntries = _timeTrackerService.GetEntriesByUser(userEmail); // Already ordered desc

            // Apply filtering
            IEnumerable<TimeEntry> filteredEntries = allEntries;
            DateTime now = DateTime.Now;

            // Normalize 'now' to today at 00:00:00 for comparison if needed
            DateTime today = now.Date;

            switch (filter.ToLower())
            {
                case "today":
                    filteredEntries = allEntries.Where(e => e.StartTime.Date == today);
                    break;
                case "yesterday":
                    filteredEntries = allEntries.Where(e => e.StartTime.Date == today.AddDays(-1));
                    break;
                case "week":
                    filteredEntries = allEntries.Where(e => e.StartTime.Date >= today.AddDays(-7));
                    break;
                case "custom":
                    if (startDate.HasValue)
                        filteredEntries = filteredEntries.Where(e => e.StartTime.Date >= startDate.Value.Date);
                    if (endDate.HasValue)
                        filteredEntries = filteredEntries.Where(e => e.StartTime.Date <= endDate.Value.Date);
                    break;
                case "all":
                default:
                    // No filter
                    break;
            }

            var entriesList = filteredEntries.ToList();

            // Calculate Summaries for "Today" specifically for the top cards
            var todayEntries = allEntries.Where(e => e.StartTime.Date == today).ToList();
            var totalTimeToday = TimeSpan.FromSeconds(todayEntries.Sum(e => e.DurationSeconds));

            var breakdown = todayEntries
                .GroupBy(e => e.TaskName)
                .ToDictionary(
                    g => g.Key ?? "Unknown",
                    g => TimeSpan.FromSeconds(g.Sum(e => e.DurationSeconds))
                );

            var viewModel = new JsonCrudApp.ViewModels.TimeTrackerViewModel
            {
                Entries = entriesList,
                Filter = filter,
                CustomStartDate = startDate,
                CustomEndDate = endDate,
                TotalTimeTracked = totalTimeToday,
                TaskBreakdown = breakdown
            };

            return View(viewModel);
        }

        [HttpPost("SaveEntry")]
        public IActionResult SaveEntry([FromBody] TimeEntry entry)
        {
            var userEmail = GetCurrentUserEmail();
            if (string.IsNullOrEmpty(userEmail)) return Unauthorized();

            entry.UserEmail = userEmail;
            _timeTrackerService.AddEntry(entry);
            return Ok(new { success = true });
        }

        [HttpPost("AddManualEntry")]
        public IActionResult AddManualEntry(TimeEntry entry)
        {
            var userEmail = GetCurrentUserEmail();
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Account");

            entry.UserEmail = userEmail;

            // Calculate duration if not provided but start/end are
            if (entry.EndTime.HasValue && entry.DurationSeconds == 0)
            {
                entry.DurationSeconds = (long)(entry.EndTime.Value - entry.StartTime).TotalSeconds;
            }
            // If Duration provided but no EndTime, calculate EndTime
            else if (entry.EndTime == null && entry.DurationSeconds > 0)
            {
                entry.EndTime = entry.StartTime.AddSeconds(entry.DurationSeconds);
            }

            if (entry.DurationSeconds < 0) entry.DurationSeconds = 0;

            _timeTrackerService.AddEntry(entry);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("UpdateEntry")]
        public IActionResult UpdateEntry(TimeEntry entry)
        {
            var userEmail = GetCurrentUserEmail();
            if (string.IsNullOrEmpty(userEmail)) return Unauthorized();

            entry.UserEmail = userEmail;

            // Recalculate duration
            if (entry.EndTime.HasValue)
            {
                entry.DurationSeconds = (long)(entry.EndTime.Value - entry.StartTime).TotalSeconds;
            }
            if (entry.DurationSeconds < 0) entry.DurationSeconds = 0;

            _timeTrackerService.UpdateEntry(entry);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("DeleteEntry")]
        public IActionResult DeleteEntry(int id)
        {
            var userEmail = GetCurrentUserEmail();
            if (string.IsNullOrEmpty(userEmail)) return Unauthorized();

            _timeTrackerService.DeleteEntry(id, userEmail);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("GetRecentTasks")]
        public IActionResult GetRecentTasks()
        {
            var userEmail = GetCurrentUserEmail();
            if (string.IsNullOrEmpty(userEmail)) return Unauthorized();

            var entries = _timeTrackerService.GetEntriesByUser(userEmail);
            var tasks = entries.Select(e => e.TaskName).Distinct().Take(5).ToList();
            return Ok(tasks);
        }

        private string? GetCurrentUserEmail()
        {
            var adminUser = HttpContext.Session.GetString("AdminUser");
            var studentUser = HttpContext.Session.GetString("StudentUser");

            if (!string.IsNullOrEmpty(adminUser)) return adminUser;
            if (!string.IsNullOrEmpty(studentUser)) return studentUser;
            return null;
        }
    }
}
