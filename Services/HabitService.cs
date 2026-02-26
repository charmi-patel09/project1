using System.Text.Json;
using JsonCrudApp.Models;

namespace JsonCrudApp.Services
{
    public class HabitService
    {
        public HabitService(IWebHostEnvironment webHostEnvironment)
        {
            WebHostEnvironment = webHostEnvironment;
        }

        public IWebHostEnvironment WebHostEnvironment { get; }

        private string JsonFileName
        {
            get { return Path.Combine(WebHostEnvironment.WebRootPath, "data", "habits.json"); }
        }

        public IEnumerable<Habit> GetAllHabits()
        {
            if (!File.Exists(JsonFileName))
            {
                return new List<Habit>();
            }

            using (var jsonFileReader = File.OpenText(JsonFileName))
            {
                var content = jsonFileReader.ReadToEnd();
                if (string.IsNullOrWhiteSpace(content)) return new List<Habit>();

                return JsonSerializer.Deserialize<Habit[]>(content,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? Enumerable.Empty<Habit>();
            }
        }

        public IEnumerable<Habit> GetHabitsByUser(string userEmail)
        {
            return GetAllHabits().Where(n => n.UserEmail == userEmail);
        }

        public Habit? AddHabit(Habit habit)
        {
            var habits = GetAllHabits().ToList();
            if (string.IsNullOrEmpty(habit.Id))
            {
                habit.Id = Guid.NewGuid().ToString();
            }
            if (habit.CreatedDate == default)
            {
                habit.CreatedDate = DateTime.Now;
            }

            habits.Add(habit);
            SaveHabits(habits);
            return habit;
        }

        public Habit? UpdateHabit(Habit habit)
        {
            var habits = GetAllHabits().ToList();
            var query = habits.FirstOrDefault(x => x.Id == habit.Id && x.UserEmail == habit.UserEmail);
            if (query != null)
            {
                query.Name = habit.Name;
                query.Description = habit.Description;
                query.Category = habit.Category;
                query.FrequencyType = habit.FrequencyType;
                query.CustomDays = habit.CustomDays;
                query.StartDate = habit.StartDate;
                query.Goal = habit.Goal;
                query.ReminderTime = habit.ReminderTime;
                // CompletedDates not updated here usually
                SaveHabits(habits);
                return query;
            }
            return null;
        }

        public bool ToggleCompletion(string habitId, string userEmail, DateTime date)
        {
            var habits = GetAllHabits().ToList();
            var habit = habits.FirstOrDefault(x => x.Id == habitId && x.UserEmail == userEmail);
            if (habit != null)
            {
                // Toggle logic: if date exists (ignoring time), remove it. Else add it.
                // We store strict dates.
                var dateOnly = date.Date;
                var existing = habit.CompletedDates.FirstOrDefault(d => d.Date == dateOnly);

                // DateTime is a struct, wait, if default it's 0001. 
                // We check if we found a matching date.
                // List.Contains uses default equality... which includes time if separate.
                // Let's rely on .Date comparison explicitly.

                var existingIndex = habit.CompletedDates.FindIndex(d => d.Date == dateOnly);

                if (existingIndex >= 0)
                {
                    habit.CompletedDates.RemoveAt(existingIndex);
                    SaveHabits(habits);
                    return false; // Not completed anymore
                }
                else
                {
                    habit.CompletedDates.Add(dateOnly);
                    SaveHabits(habits);
                    return true; // Completed
                }
            }
            return false;
        }

        public void DeleteHabit(string id, string userEmail)
        {
            var habits = GetAllHabits().ToList();
            var habit = habits.FirstOrDefault(x => x.Id == id && x.UserEmail == userEmail);
            if (habit != null)
            {
                habits.Remove(habit);
                SaveHabits(habits);
            }
        }

        private void SaveHabits(IEnumerable<Habit> habits)
        {
            var folder = Path.GetDirectoryName(JsonFileName);
            if (folder != null && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            using (var outputStream = File.OpenWrite(JsonFileName))
            {
                outputStream.SetLength(0); // Clear existing content
                JsonSerializer.Serialize<IEnumerable<Habit>>(
                    new Utf8JsonWriter(outputStream, new JsonWriterOptions
                    {
                        Indented = true
                    }),
                    habits
                );
            }
        }

        public HabitAnalytics GetAnalytics(string userEmail)
        {
            var habits = GetHabitsByUser(userEmail).ToList();
            var today = DateTime.Today;
            var last7Days = Enumerable.Range(0, 7).Select(i => today.AddDays(-i)).ToList();

            int totalCompleted = habits.Sum(h => h.CompletedDates.Count);
            
            // Missed calculation is tricky. For now, let's say missed are total expected minus completed.
            // But we only care about missed until today.
            int totalMissed = 0;
            int weeklyCompleted = 0;
            int weeklyMissed = 0;

            foreach (var h in habits)
            {
                // Calculate missed since start date
                var current = h.StartDate.Date;
                if (current > today) continue;

                while (current <= today)
                {
                    if (IsActiveDay(h, current))
                    {
                        bool completed = h.CompletedDates.Any(d => d.Date == current);
                        if (!completed)
                        {
                            totalMissed++;
                            if (last7Days.Contains(current)) weeklyMissed++;
                        }
                        else
                        {
                            if (last7Days.Contains(current)) weeklyCompleted++;
                        }
                    }
                    current = current.AddDays(1);
                }
            }

            return new HabitAnalytics
            {
                TotalCompleted = totalCompleted,
                TotalMissed = totalMissed,
                WeeklyCompleted = weeklyCompleted,
                WeeklyMissed = weeklyMissed
            };
        }

        private bool IsActiveDay(Habit h, DateTime date)
        {
            if (h.FrequencyType == "Daily") return true;
            if (h.FrequencyType == "Custom")
            {
                return h.CustomDays.Contains(date.DayOfWeek.ToString());
            }
            return false;
        }

        public int GetStreak(Habit h)
        {
            int streak = 0;
            var current = DateTime.Today;

            // If it was an active day and not completed, streak is broken.
            // Exception: if today is active but not yet completed, we check yesterday.
            
            if (IsActiveDay(h, current))
            {
                if (h.CompletedDates.Any(d => d.Date == current))
                {
                    streak++;
                }
                else
                {
                    // Check if it's the only active day missed. 
                    // If today is not done but yesterday was done (or not active), streak might still be alive from yesterday.
                    // Actually, if today IS active and NOT done, the streak of "consecutive completions" is technically 0 for now,
                    // OR it's whatever it was yesterday if we still have time to complete it today.
                    // Most apps show the streak up to yesterday if today isn't done yet.
                }
            }

            var checkDate = current.AddDays(-1);
            while (checkDate >= h.StartDate)
            {
                if (IsActiveDay(h, checkDate))
                {
                    if (h.CompletedDates.Any(d => d.Date == checkDate))
                    {
                        streak++;
                    }
                    else
                    {
                        break; // Streak broken
                    }
                }
                checkDate = checkDate.AddDays(-1);
            }

            return streak;
        }

        public double GetCompletionPercentage(Habit h)
        {
            var today = DateTime.Today;
            var start = h.StartDate.Date;
            if (start > today) return 0;

            int expected = 0;
            int actual = 0;

            var current = start;
            while (current <= today)
            {
                if (IsActiveDay(h, current))
                {
                    expected++;
                    if (h.CompletedDates.Any(d => d.Date == current))
                    {
                        actual++;
                    }
                }
                current = current.AddDays(1);
            }

            if (expected == 0) return 0;
            return Math.Round((double)actual / expected * 100, 1);
        }
    }

    public class HabitAnalytics
    {
        public int TotalCompleted { get; set; }
        public int TotalMissed { get; set; }
        public int WeeklyCompleted { get; set; }
        public int WeeklyMissed { get; set; }
    }
}
