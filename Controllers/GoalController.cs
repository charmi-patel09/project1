using Microsoft.AspNetCore.Mvc;
using JsonCrudApp.Models;
using JsonCrudApp.Services;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace JsonCrudApp.Controllers
{
    public class GoalController : Controller
    {
        private readonly GoalService _goalService;

        public GoalController(GoalService goalService)
        {
            _goalService = goalService;
        }

        [HttpGet]
        public IActionResult GetGoals()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var goals = _goalService.GetUserGoals(userId.Value);
            return Json(goals);
        }

        [HttpPost]
        public IActionResult CreateGoal([FromBody] Goal goal)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            goal.UserId = userId.Value;
            goal.CreatedDate = DateTime.Now;

            // Smart Validation
            var duration = (goal.EndDate - goal.StartDate).Days;
            if (duration < 2 && goal.Title.Length > 20)
            {
                // Simple heuristic for "too short for goal size"
                return Json(new { success = false, message = "This goal may not be realistic within the selected time." });
            }

            _goalService.AddGoal(goal);
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult UpdateMilestone(string goalId, string milestoneId, bool isCompleted)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var goals = _goalService.GetUserGoals(userId.Value);
            var goal = goals.FirstOrDefault(g => g.Id.Equals(goalId, StringComparison.OrdinalIgnoreCase));
            if (goal == null) return NotFound();

            var milestone = goal.Milestones.FirstOrDefault(m => m.Id.Equals(milestoneId, StringComparison.OrdinalIgnoreCase));
            if (milestone != null)
            {
                milestone.IsCompleted = isCompleted;
                milestone.CompletedAt = isCompleted ? DateTime.Now : null;
                _goalService.UpdateGoal(goal);
            }

            return Json(new { success = true, goal = goal, progress = goal.Progress, status = goal.Status });
        }

        [HttpPost]
        public IActionResult UpdateDailyTask(string goalId, string taskId, bool isCompleted)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var goals = _goalService.GetUserGoals(userId.Value);
            var goal = goals.FirstOrDefault(g => g.Id.Equals(goalId, StringComparison.OrdinalIgnoreCase));
            if (goal == null) return NotFound();

            var task = goal.DailyTasks.FirstOrDefault(t => t.Id.Equals(taskId, StringComparison.OrdinalIgnoreCase) && t.UserId == userId.Value);
            if (task != null)
            {
                task.IsCompleted = isCompleted;
                task.CompletedAt = isCompleted ? DateTime.Now : null;
                _goalService.UpdateGoal(goal);
            }

            return Json(new { success = true, goal = goal, progress = goal.Progress, status = goal.Status, schedule = goal.ScheduleStatus });
        }

        [HttpDelete]
        public IActionResult DeleteGoal(string id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            _goalService.DeleteGoal(id, userId.Value);
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult TaskAction([FromForm] string goalId, [FromForm] string taskId, [FromForm] string action, [FromForm] string content)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var goal = _goalService.TaskAction(goalId, taskId, action, content, userId.Value);
            return Json(new { success = true, goal = goal });
        }

        [HttpPost]
        public IActionResult RedistributeTasks(string goalId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var goal = _goalService.RedistributeRemainingTasks(goalId, userId.Value);
            return Json(new { success = true, goal = goal });
        }

        [HttpGet]
        public IActionResult GetDailyTasks(string goalId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var tasks = _goalService.GetDailyTasks(goalId, userId.Value);
            var grouped = tasks
                .GroupBy(t => t.Date.Date.ToString("yyyy-MM-dd"))
                .ToDictionary(g => g.Key, g => g.ToList());

            return Json(grouped);
        }

        [HttpPost]
        public IActionResult UpdateDailyTaskDetail([FromForm] string goalId, [FromForm] string taskId, [FromForm] string title, [FromForm] string description)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var goal = _goalService.UpdateDailyTaskDetail(goalId, taskId, title, description, userId.Value);
            if (goal == null) return NotFound(new { success = false, message = "Task not found." });

            return Json(new { success = true, goal = goal });
        }

        [HttpGet]
        public IActionResult GetAnalytics()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var analytics = _goalService.GetAnalytics(userId.Value);
            return Json(analytics);
        }
    }
}
