using Microsoft.AspNetCore.Mvc;
using JsonCrudApp.Models;
using JsonCrudApp.Services;

namespace JsonCrudApp.Controllers
{
    public class HabitController : BaseController
    {
        private readonly HabitService _habitService;
        private readonly JsonFileStudentService _studentService;

        public HabitController(HabitService habitService, JsonFileStudentService studentService)
        {
            _habitService = habitService;
            _studentService = studentService;
        }

        [HttpGet]
        public IActionResult GetHabits()
        {

            var userEmail = HttpContext.Session.GetString("StudentUser");
            if (string.IsNullOrEmpty(userEmail)) return Unauthorized();

            var student = _studentService.GetStudents().FirstOrDefault(s => s.Email == userEmail);
            if (!IsAuthorized(student!, "habit-hub")) return Unauthorized();

            var habits = _habitService.GetHabitsByUser(userEmail).Select(h => new
            {
                h.Id,
                h.Name,
                h.Description,
                h.Category,
                h.FrequencyType,
                h.CustomDays,
                h.StartDate,
                h.CompletedDates,
                h.ReminderTime,
                h.Goal,
                Streak = _habitService.GetStreak(h),
                CompletionPercentage = _habitService.GetCompletionPercentage(h)
            });
            return Ok(habits);
        }

        [HttpGet]
        public IActionResult Edit(string id)
        {
            var userEmail = HttpContext.Session.GetString("StudentUser");
            if (string.IsNullOrEmpty(userEmail)) return Unauthorized();

            var student = _studentService.GetStudents().FirstOrDefault(s => s.Email == userEmail);
            if (!IsAuthorized(student!, "habit-hub")) return Unauthorized();

            var habit = _habitService.GetHabitsByUser(userEmail).FirstOrDefault(h => h.Id == id);
            if (habit == null) return NotFound();

            return Ok(habit);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Habit habit)
        {
            var userEmail = HttpContext.Session.GetString("StudentUser");
            if (string.IsNullOrEmpty(userEmail)) return Unauthorized();

            var student = _studentService.GetStudents().FirstOrDefault(s => s.Email == userEmail);
            if (!IsAuthorized(student!, "habit-hub")) return Unauthorized();

            // Prevent duplicate records (Name + UserEmail)
            var existing = _habitService.GetHabitsByUser(userEmail)
                .FirstOrDefault(h => h.Name.Equals(habit.Name, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                return BadRequest("A habit with this name already exists.");
            }

            habit.UserEmail = userEmail;

            if (habit.ReminderType == "AfterDuration")
            {
                habit.ReminderTime = DateTime.Now.AddMinutes(habit.ReminderDuration).ToString("HH:mm");
            }

            var created = _habitService.AddHabit(habit);
            return Ok(created);
        }

        [HttpPost]
        public IActionResult Edit([FromBody] Habit habit)
        {
            var userEmail = HttpContext.Session.GetString("StudentUser");
            if (string.IsNullOrEmpty(userEmail)) return Unauthorized();

            var student = _studentService.GetStudents().FirstOrDefault(s => s.Email == userEmail);
            if (!IsAuthorized(student!, "habit-hub")) return Unauthorized();

            // Prevent duplicate records, excluding current habit
            var existing = _habitService.GetHabitsByUser(userEmail)
                .FirstOrDefault(h => h.Name.Equals(habit.Name, StringComparison.OrdinalIgnoreCase) && h.Id != habit.Id);
            if (existing != null)
            {
                return BadRequest("Another habit with this name already exists.");
            }

            habit.UserEmail = userEmail;

            if (habit.ReminderType == "AfterDuration")
            {
                habit.ReminderTime = DateTime.Now.AddMinutes(habit.ReminderDuration).ToString("HH:mm");
            }

            var updated = _habitService.UpdateHabit(habit);
            if (updated == null) return NotFound();

            return Ok(updated);
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {
            var userEmail = HttpContext.Session.GetString("StudentUser");
            if (string.IsNullOrEmpty(userEmail)) return Unauthorized();

            var student = _studentService.GetStudents().FirstOrDefault(s => s.Email == userEmail);
            if (!IsAuthorized(student!, "habit-hub")) return Unauthorized();

            _habitService.DeleteHabit(id, userEmail);
            return Ok();
        }

        [HttpPost]
        public IActionResult Toggle([FromBody] ToggleRequest request)
        {
            var userEmail = HttpContext.Session.GetString("StudentUser");
            if (string.IsNullOrEmpty(userEmail)) return Unauthorized();

            var student = _studentService.GetStudents().FirstOrDefault(s => s.Email == userEmail);
            if (!IsAuthorized(student!, "habit-hub")) return Unauthorized();

            var result = _habitService.ToggleCompletion(request.HabitId, userEmail, request.Date);
            return Ok(new { completed = result });
        }

        [HttpGet]
        public IActionResult CheckReminders()
        {
            var userEmail = HttpContext.Session.GetString("StudentUser");
            if (string.IsNullOrEmpty(userEmail)) return Unauthorized();

            var student = _studentService.GetStudents().FirstOrDefault(s => s.Email == userEmail);
            if (student == null || !IsAuthorized(student, "habit-hub")) return Unauthorized();

            var habits = _habitService.GetHabitsByUser(userEmail);
            var now = DateTime.Now;
            var currentTimeStr = now.ToString("HH:mm");
            var today = now.Date;

            var dueHabits = new List<object>();

            foreach (var h in habits)
            {
                if (string.IsNullOrEmpty(h.ReminderTime)) continue;

                // Check time match (exact minute)
                if (h.ReminderTime != currentTimeStr) continue;

                // Check if completed today
                if (h.CompletedDates.Any(d => d.Date == today)) continue;

                // Check if already reminded today
                if (h.LastReminderDate.HasValue && h.LastReminderDate.Value.Date == today) continue;

                // Check frequency
                bool isDueToday = false;
                if (h.FrequencyType == "Daily") isDueToday = true;
                else if (h.FrequencyType == "Custom")
                {
                    var dayName = now.DayOfWeek.ToString();
                    if (h.CustomDays != null && h.CustomDays.Contains(dayName)) isDueToday = true;
                }

                if (isDueToday)
                {
                    dueHabits.Add(new { h.Id, h.Name, h.Description });
                    h.LastReminderDate = now;
                    _habitService.UpdateHabit(h);
                }
            }

            return Ok(dueHabits);
        }

        [HttpGet]
        public IActionResult GetAnalytics()
        {
            var userEmail = HttpContext.Session.GetString("StudentUser");
            if (string.IsNullOrEmpty(userEmail)) return Unauthorized();

            var student = _studentService.GetStudents().FirstOrDefault(s => s.Email == userEmail);
            if (!IsAuthorized(student!, "habit-hub")) return Unauthorized();

            var analytics = _habitService.GetAnalytics(userEmail);
            return Ok(analytics);
        }

        public class ToggleRequest
        {
            public string HabitId { get; set; } = string.Empty;
            public DateTime Date { get; set; }
        }
    }
}
