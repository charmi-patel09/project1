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
                query.FrequencyType = habit.FrequencyType;
                query.CustomDays = habit.CustomDays;
                query.StartDate = habit.StartDate;
                query.Goal = habit.Goal;
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
    }
}
