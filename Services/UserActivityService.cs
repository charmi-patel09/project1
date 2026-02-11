using System.Text.Json;
using JsonCrudApp.Models;
using JsonCrudApp.Services; // Assuming JsonFileStudentService is in this namespace

namespace JsonCrudApp.Services
{
    public class UserActivityService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly JsonFileStudentService _studentService;

        public UserActivityService(IWebHostEnvironment webHostEnvironment, JsonFileStudentService studentService)
        {
            _webHostEnvironment = webHostEnvironment;
            _studentService = studentService;
        }

        private string JsonFileName
        {
            get { return Path.Combine(_webHostEnvironment.WebRootPath, "data", "user_activity.json"); }
        }

        public void LogVisit(string email, string url)
        {
            var visit = new UserVisit { UserEmail = email, Timestamp = DateTime.UtcNow, PageUrl = url };
            var visits = GetAllVisits().ToList();
            visits.Add(visit);
            SaveVisits(visits);
        }

        public IEnumerable<UserVisit> GetAllVisits()
        {
            if (!File.Exists(JsonFileName)) return new List<UserVisit>();

            try
            {
                using var jsonFileReader = File.OpenText(JsonFileName);
                var content = jsonFileReader.ReadToEnd();
                if (string.IsNullOrWhiteSpace(content)) return new List<UserVisit>();
                return JsonSerializer.Deserialize<List<UserVisit>>(content) ?? new List<UserVisit>();
            }
            catch
            {
                return new List<UserVisit>();
            }
        }

        public DailyActivityReport GetReport(string rangeType, DateTime? date = null)
        {
            var today = DateTime.UtcNow.Date;
            var allVisits = GetAllVisits(); // In real app, optimize this with DB logging!
            var students = _studentService.GetStudents().ToDictionary(s => s.Email ?? "", s => s, StringComparer.OrdinalIgnoreCase);

            IEnumerable<UserVisit> filteredVisits;
            string displayDate;
            DateTime reportDate;

            if (rangeType == "Yesterday")
            {
                var start = today.AddDays(-1);
                var end = today;
                reportDate = start;
                filteredVisits = allVisits.Where(v => v.Timestamp >= start && v.Timestamp < end);
                displayDate = start.ToShortDateString();
            }
            else if (rangeType == "Last7Days")
            {
                var start = today.AddDays(-6);
                var end = today.AddDays(1);
                reportDate = today;
                filteredVisits = allVisits.Where(v => v.Timestamp >= start && v.Timestamp < end);
                displayDate = "Last 7 Days";
            }
            else // Today or Specific Date
            {
                var rawTarget = date?.Date ?? today;
                // Normalize to UTC to ensure consistent comparison if v.Timestamp is UTC
                var target = rawTarget.Kind == DateTimeKind.Utc ? rawTarget : DateTime.SpecifyKind(rawTarget, DateTimeKind.Utc);

                reportDate = target;
                var end = target.AddDays(1);
                filteredVisits = allVisits.Where(v => v.Timestamp >= target && v.Timestamp < end);
                displayDate = target.ToShortDateString();
            }

            var list = filteredVisits.ToList();

            var stats = list
                .GroupBy(v => v.UserEmail)
                .Select(g => new UserActivityStat
                {
                    UserEmail = g.Key,
                    VisitCount = g.Count(),
                    LastSeen = g.Max(v => v.Timestamp),
                    UserType = students.TryGetValue(g.Key, out var s) ?
                               (s.Role == "Admin" ? "Administrator" :
                                s.Role == "Private" ? "Private User" :
                                s.Role == "Guest" ? "Guest User" : "Normal User") : "Normal User"
                })
                .OrderByDescending(s => s.LastSeen)
                .ToList();

            return new DailyActivityReport
            {
                Date = reportDate,
                DisplayDate = displayDate,
                TotalUniqueUsers = stats.Count,
                TotalVisits = list.Count,
                UserStats = stats
            };
        }

        public IEnumerable<UserVisit> GetUserVisits(string email, string rangeType, DateTime? date = null)
        {
            var today = DateTime.UtcNow.Date;
            var allVisits = GetAllVisits();

            IEnumerable<UserVisit> filteredVisits;

            if (rangeType == "Yesterday")
            {
                var start = today.AddDays(-1);
                var end = today;
                filteredVisits = allVisits.Where(v => v.Timestamp >= start && v.Timestamp < end);
            }
            else if (rangeType == "Last7Days")
            {
                var start = today.AddDays(-6);
                var end = today.AddDays(1);
                filteredVisits = allVisits.Where(v => v.Timestamp >= start && v.Timestamp < end);
            }
            else // Today or Specific Date
            {
                var rawTarget = date?.Date ?? today;
                var target = rawTarget.Kind == DateTimeKind.Utc ? rawTarget : DateTime.SpecifyKind(rawTarget, DateTimeKind.Utc);
                var end = target.AddDays(1);
                filteredVisits = allVisits.Where(v => v.Timestamp >= target && v.Timestamp < end);
            }

            return filteredVisits
                .Where(v => v.UserEmail.Equals(email, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(v => v.Timestamp);
        }

        private void SaveVisits(List<UserVisit> visits)
        {
            var folder = Path.GetDirectoryName(JsonFileName);
            if (folder != null && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            using (var outputStream = File.OpenWrite(JsonFileName))
            {
                outputStream.SetLength(0); // Clear existing content
                JsonSerializer.Serialize(
                    new Utf8JsonWriter(outputStream, new JsonWriterOptions { Indented = true }),
                    visits
                );
            }
        }
    }

    public class DailyActivityReport
    {
        public DateTime Date { get; set; }
        public string DisplayDate { get; set; } = string.Empty;
        public int TotalUniqueUsers { get; set; }
        public int TotalVisits { get; set; }
        public List<UserActivityStat> UserStats { get; set; } = new List<UserActivityStat>();
    }

    public class UserActivityStat
    {
        public string UserEmail { get; set; } = string.Empty;
        public int VisitCount { get; set; }
        public DateTime LastSeen { get; set; }
        public string UserType { get; set; } = "Normal User";
    }
}
