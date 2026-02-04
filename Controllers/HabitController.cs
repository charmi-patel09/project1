using Microsoft.AspNetCore.Mvc;
using JsonCrudApp.Models;
using JsonCrudApp.Services;

namespace JsonCrudApp.Controllers
{
    public class HabitController : BaseController
    {
        private readonly HabitService _habitService;

        public HabitController(HabitService habitService)
        {
            _habitService = habitService;
        }

        [HttpGet]
        public IActionResult GetHabits()
        {
            var userEmail = HttpContext.Session.GetString("StudentUser");
            if (string.IsNullOrEmpty(userEmail)) return Unauthorized();

            var habits = _habitService.GetHabitsByUser(userEmail);
            return Ok(habits);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Habit habit)
        {
            var userEmail = HttpContext.Session.GetString("StudentUser");
            if (string.IsNullOrEmpty(userEmail)) return Unauthorized();

            habit.UserEmail = userEmail;

            // Basic validation
            if (string.IsNullOrWhiteSpace(habit.Name))
            {
                return BadRequest("Name is required");
            }

            var created = _habitService.AddHabit(habit);
            return Ok(created);
        }

        [HttpPost]
        public IActionResult Update([FromBody] Habit habit)
        {
            var userEmail = HttpContext.Session.GetString("StudentUser");
            if (string.IsNullOrEmpty(userEmail)) return Unauthorized();

            habit.UserEmail = userEmail;
            var updated = _habitService.UpdateHabit(habit);
            if (updated == null) return NotFound();

            return Ok(updated);
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {
            var userEmail = HttpContext.Session.GetString("StudentUser");
            if (string.IsNullOrEmpty(userEmail)) return Unauthorized();

            _habitService.DeleteHabit(id, userEmail);
            return Ok();
        }

        [HttpPost]
        public IActionResult Toggle([FromBody] ToggleRequest request)
        {
            var userEmail = HttpContext.Session.GetString("StudentUser");
            if (string.IsNullOrEmpty(userEmail)) return Unauthorized();

            var result = _habitService.ToggleCompletion(request.HabitId, userEmail, request.Date);
            return Ok(new { completed = result });
        }

        public class ToggleRequest
        {
            public string HabitId { get; set; } = string.Empty;
            public DateTime Date { get; set; }
        }
    }
}
