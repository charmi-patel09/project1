using System.Text.Json;
using JsonCrudApp.Models;
using Microsoft.AspNetCore.Hosting;

namespace JsonCrudApp.Services
{
    public class GoalService
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, WriteIndented = true };

        public GoalService(IWebHostEnvironment webHostEnvironment)
        {
            _filePath = Path.Combine(webHostEnvironment.ContentRootPath, "Data", "goals.json");
            if (!Directory.Exists(Path.GetDirectoryName(_filePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            }
            if (!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, "[]");
            }
        }

        public List<Goal> GetUserGoals(int userId)
        {
            var json = File.ReadAllText(_filePath);
            var goals = JsonSerializer.Deserialize<List<Goal>>(json, _options) ?? new List<Goal>();
            var userGoals = goals.Where(g => g.UserId == userId).ToList();

            bool modified = false;
            foreach (var goal in userGoals)
            {
                // Auto-update status to Overdue
                if (goal.Status != "Completed" && goal.EndDate < DateTime.Today && goal.Status != "Overdue")
                {
                    goal.Status = "Overdue";
                    modified = true;
                }

                // Update Streak & Schedule Status
                if (CalculateCurrentState(goal)) modified = true;
            }

            if (modified)
            {
                SaveAllGoals(goals);
            }

            return userGoals.OrderByDescending(g => g.CreatedDate).ToList();
        }

        private bool CalculateCurrentState(Goal goal)
        {
            if (goal.DailyTasks == null || !goal.DailyTasks.Any()) return false;

            bool modified = false;
            var oldStatus = goal.ScheduleStatus;
            var oldStreak = goal.Streak;

            // Group tasks by date
            var groupedByDate = goal.DailyTasks
                .GroupBy(t => t.Date.Date)
                .OrderBy(g => g.Key)
                .ToList();

            // Calculate Streak
            int currentStreak = 0;
            var yesterday = DateTime.Today.AddDays(-1);
            var checkDate = yesterday;

            // Look backwards from yesterday
            while (true)
            {
                var dayTasks = groupedByDate.FirstOrDefault(d => d.Key == checkDate);
                if (dayTasks != null && dayTasks.All(t => t.IsCompleted))
                {
                    currentStreak++;
                    checkDate = checkDate.AddDays(-1);
                }
                else
                {
                    break;
                }
            }

            // Check if today is also fully completed to add to streak display (optional, usually streak counts yesterday's success)
            var todayTasks = groupedByDate.FirstOrDefault(d => d.Key == DateTime.Today);
            if (todayTasks != null && todayTasks.All(t => t.IsCompleted))
            {
                currentStreak++;
            }

            if (goal.Streak != currentStreak)
            {
                goal.Streak = currentStreak;
                modified = true;
            }

            // Update Schedule Status
            var missedDays = groupedByDate
                .Where(d => d.Key < DateTime.Today && d.Any(t => !t.IsCompleted))
                .Count();

            if (goal.Status == "Completed") goal.ScheduleStatus = "Completed";
            else goal.ScheduleStatus = "On Track";

            if (oldStatus != goal.ScheduleStatus) modified = true;

            return modified;
        }

        public void AddGoal(Goal goal)
        {
            // Always generate a full month structure
            goal.DailyTasks = GenerateMonthlyTaskStructure(goal.StartDate, goal.Title, goal.Id, goal.UserId);

            if (goal.Milestones == null || !goal.Milestones.Any())
            {
                goal.Milestones = GenerateSmartMilestones(goal);
            }

            var json = File.ReadAllText(_filePath);
            var goals = JsonSerializer.Deserialize<List<Goal>>(json, _options) ?? new List<Goal>();
            goals.Add(goal);
            SaveAllGoals(goals);
        }

        public void UpdateGoal(Goal updatedGoal)
        {
            var json = File.ReadAllText(_filePath);
            var goals = JsonSerializer.Deserialize<List<Goal>>(json, _options) ?? new List<Goal>();
            var index = goals.FindIndex(g => g.Id == updatedGoal.Id && g.UserId == updatedGoal.UserId);
            if (index != -1)
            {
                // Progress is now strictly based on Daily Tasks
                double dProgress = updatedGoal.DailyTasks.Any() ? (double)updatedGoal.DailyTasks.Count(t => t.IsCompleted) / updatedGoal.DailyTasks.Count : 1;

                updatedGoal.Progress = dProgress * 100;

                if (updatedGoal.Progress >= 100)
                {
                    updatedGoal.Progress = 100;
                    updatedGoal.Status = "Completed";
                    updatedGoal.ScheduleStatus = "Completed";
                }
                else if (updatedGoal.Progress > 0) updatedGoal.Status = "In Progress";
                else updatedGoal.Status = "Not Started";

                CalculateCurrentState(updatedGoal);

                if (updatedGoal.Status != "Completed" && updatedGoal.EndDate < DateTime.Today)
                {
                    updatedGoal.Status = "Overdue";
                }

                goals[index] = updatedGoal;
                SaveAllGoals(goals);
            }
        }

        public Goal? UpdateDailyTaskDetail(string goalId, string taskId, string title, string description, int userId)
        {
            var json = File.ReadAllText(_filePath);
            var goals = JsonSerializer.Deserialize<List<Goal>>(json, _options) ?? new List<Goal>();

            var goal = goals.FirstOrDefault(g => g.Id.Equals(goalId, StringComparison.OrdinalIgnoreCase) && g.UserId == userId);
            if (goal == null) return null;

            var task = goal.DailyTasks.FirstOrDefault(t => t.Id.Equals(taskId, StringComparison.OrdinalIgnoreCase) && t.UserId == userId);
            if (task == null) return null;

            task.Title = title ?? "";
            task.Description = description ?? "";

            // Recalculate progress before saving
            double dProgress = goal.DailyTasks.Any() ? (double)goal.DailyTasks.Count(t => t.IsCompleted) / goal.DailyTasks.Count : 1;
            goal.Progress = dProgress * 100;

            if (goal.Progress >= 100) { goal.Status = "Completed"; goal.ScheduleStatus = "Completed"; }
            else if (goal.Progress > 0) goal.Status = "In Progress";
            else goal.Status = "Not Started";

            CalculateCurrentState(goal);

            if (goal.Status != "Completed" && goal.EndDate < DateTime.Today) goal.Status = "Overdue";

            SaveAllGoals(goals);
            return goal;
        }

        public Goal? TaskAction(string goalId, string taskId, string action, string? content, int userId)
        {
            var json = File.ReadAllText(_filePath);
            var goals = JsonSerializer.Deserialize<List<Goal>>(json, _options) ?? new List<Goal>();
            var goal = goals.FirstOrDefault(g => g.Id.Equals(goalId, StringComparison.OrdinalIgnoreCase) && g.UserId == userId);
            if (goal == null) return null;

            if (action == "add_to_date" && content != null)
            {
                var parts = content.Split('|');
                if (parts.Length >= 2 && DateTime.TryParse(parts[0], out var date))
                {
                    // Constraint: One task per day
                    if (goal.DailyTasks.Any(t => t.Date.Date == date.Date))
                    {
                        return goal; // Or throw error, but UI should handle this
                    }

                    var taskContent = parts[1];
                    var taskDesc = parts.Length > 2 ? parts[2] : "";
                    goal.DailyTasks.Add(new DailyTask
                    {
                        Title = taskContent,
                        Description = taskDesc,
                        Date = date,
                        UserId = goal.UserId,
                        GoalId = goal.Id
                    });
                }
            }
            else
            {
                var task = goal.DailyTasks.FirstOrDefault(t => t.Id.Equals(taskId, StringComparison.OrdinalIgnoreCase) && t.UserId == userId);
                if (task != null)
                {
                    if (action == "edit" && content != null)
                    {
                        var parts = content.Split('|');
                        task.Title = parts[0];
                        if (parts.Length > 1) task.Description = parts[1];
                    }
                    else if (action == "delete") goal.DailyTasks.Remove(task);
                }
            }

            // Consolidate progress and saving
            double dProgress = goal.DailyTasks.Any() ? (double)goal.DailyTasks.Count(t => t.IsCompleted) / goal.DailyTasks.Count : 1;
            goal.Progress = dProgress * 100;

            if (goal.Progress >= 100) { goal.Status = "Completed"; goal.ScheduleStatus = "Completed"; }
            else if (goal.Progress > 0) goal.Status = "In Progress";
            else goal.Status = "Not Started";

            CalculateCurrentState(goal);
            if (goal.Status != "Completed" && goal.EndDate < DateTime.Today) goal.Status = "Overdue";

            SaveAllGoals(goals);
            return goal;
        }

        public Goal? RedistributeRemainingTasks(string goalId, int userId)
        {
            var json = File.ReadAllText(_filePath);
            var goals = JsonSerializer.Deserialize<List<Goal>>(json, _options) ?? new List<Goal>();
            var goal = goals.FirstOrDefault(g => g.Id == goalId && g.UserId == userId);
            if (goal == null) return null;

            var uncompletedPast = goal.DailyTasks.Where(t => t.Date.Date < DateTime.Today.Date && !t.IsCompleted).ToList();
            if (!uncompletedPast.Any()) return goal;

            var futureDays = goal.DailyTasks
                .Where(t => t.Date.Date >= DateTime.Today.Date)
                .GroupBy(t => t.Date.Date)
                .OrderBy(g => g.Key)
                .ToList();

            if (!futureDays.Any())
            {
                foreach (var t in uncompletedPast) t.Date = DateTime.Today;
            }
            else
            {
                int dayIndex = 0;
                foreach (var task in uncompletedPast)
                {
                    task.Date = futureDays[dayIndex % futureDays.Count].Key;
                    dayIndex++;
                }
            }

            goal.DailyTasks = goal.DailyTasks.OrderBy(t => t.Date).ToList();
            UpdateGoal(goal);
            return goal;
        }

        public void DeleteGoal(string id, int userId)
        {
            var json = File.ReadAllText(_filePath);
            var goals = JsonSerializer.Deserialize<List<Goal>>(json, _options) ?? new List<Goal>();

            // Security: Only delete if both Goal ID and User ID match
            goals.RemoveAll(g => g.Id == id && g.UserId == userId);

            SaveAllGoals(goals);
        }

        public List<DailyTask> GetDailyTasks(string goalId, int userId)
        {
            var json = File.ReadAllText(_filePath);
            var goals = JsonSerializer.Deserialize<List<Goal>>(json, _options) ?? new List<Goal>();
            var goal = goals.FirstOrDefault(g => g.Id == goalId && g.UserId == userId);
            return goal?.DailyTasks ?? new List<DailyTask>();
        }

        private void SaveAllGoals(List<Goal> goals)
        {
            File.WriteAllText(_filePath, JsonSerializer.Serialize(goals, _options));
        }

        private List<Milestone> GenerateSmartMilestones(Goal goal)
        {
            var milestones = new List<Milestone>();
            var duration = (goal.EndDate - goal.StartDate).Days;
            if (duration < 1) duration = 1;

            int numMilestones = 3;
            if (duration <= 3) numMilestones = duration;
            else if (duration <= 7) numMilestones = 4;
            else if (duration <= 30) numMilestones = 6;
            else numMilestones = 8;

            int interval = duration / numMilestones;
            if (interval < 1) interval = 1;

            for (int i = 1; i <= numMilestones; i++)
            {
                milestones.Add(new Milestone
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = $"Phase {i}: Achieve key target for {goal.Title}",
                    DueDate = goal.StartDate.AddDays(i * interval > duration ? duration : i * interval),
                    IsCompleted = false
                });
            }
            if (milestones.Last().DueDate < goal.EndDate) milestones.Last().DueDate = goal.EndDate;
            return milestones;
        }

        private List<DailyTask> GenerateMonthlyTaskStructure(DateTime startDate, string goalTitle, string goalId, int userId)
        {
            var tasks = new List<DailyTask>();
            var daysInMonth = DateTime.DaysInMonth(startDate.Year, startDate.Month);

            for (int i = 1; i <= daysInMonth; i++)
            {
                var date = new DateTime(startDate.Year, startDate.Month, i);
                tasks.Add(new DailyTask
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    GoalId = goalId,
                    Title = $"Focus Session: {goalTitle}",
                    Date = date,
                    IsCompleted = false
                });
            }
            return tasks;
        }

        public dynamic GetAnalytics(int userId)
        {
            var goals = GetUserGoals(userId);
            if (!goals.Any()) return new { Score = 0, Completed = 0, Active = 0, Overdue = 0 };

            var completed = goals.Count(g => g.Status == "Completed");
            var active = goals.Count(g => g.Status == "In Progress" || g.Status == "Not Started");
            var overdue = goals.Count(g => g.Status == "Overdue");

            double avgProgress = goals.Average(g => g.Progress);
            double score = ((double)completed / goals.Count * 60) + ((avgProgress / 100) * 40);

            score -= (overdue * 5);
            if (score < 0) score = 0;
            if (score > 100) score = 100;

            // Weekly Stats
            var last7Days = Enumerable.Range(0, 7).Select(i => DateTime.Today.AddDays(-i)).ToList();
            int totalTasksLast7 = 0;
            int completedTasksLast7 = 0;

            foreach (var goal in goals)
            {
                var recentTasks = goal.DailyTasks.Where(t => t.Date.Date >= last7Days.Last().Date && t.Date.Date <= last7Days.First().Date).ToList();
                totalTasksLast7 += recentTasks.Count;
                completedTasksLast7 += recentTasks.Count(t => t.IsCompleted);
            }

            double weeklyCompletion = totalTasksLast7 > 0 ? (double)completedTasksLast7 / totalTasksLast7 * 100 : 0;

            return new
            {
                Score = Math.Round(score, 1),
                Completed = completed,
                Active = active,
                Overdue = overdue,
                Total = goals.Count,
                SuccessRatio = Math.Round((double)completed / goals.Count * 100, 1),
                WeeklyPerformance = Math.Round(weeklyCompletion, 1)
            };
        }
    }
}
