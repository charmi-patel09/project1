using System.Text.Json;
using JsonCrudApp.Models;

namespace JsonCrudApp.Services
{
    public class UserActivityService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UserActivityService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        private string JsonFileName
        {
            get { return Path.Combine(_webHostEnvironment.WebRootPath, "data", "user_activity.json"); }
        }

        public void LogVisit(string email, string url)
        {
            var visit = new UserVisit { UserEmail = email, Timestamp = DateTime.Now, PageUrl = url };
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
            var today = DateTime.Today;
            var allVisits = GetAllVisits(); // In real app, optimize this with DB logging!

            IEnumerable<UserVisit> filteredVisits;
            string displayDate;

            if (rangeType == "Yesterday")
            {
                var target = today.AddDays(-1);
                filteredVisits = allVisits.Where(v => v.Timestamp.Date == target);
                displayDate = target.ToShortDateString();
            }
            else if (rangeType == "Last7Days")
            {
                var start = today.AddDays(-6);
                filteredVisits = allVisits.Where(v => v.Timestamp.Date >= start && v.Timestamp.Date <= today);
                displayDate = "Last 7 Days";
            }
            else // Today or Specific Date
            {
                var target = date?.Date ?? today;
                filteredVisits = allVisits.Where(v => v.Timestamp.Date == target);
                displayDate = target.ToShortDateString();
            }

            var list = filteredVisits.ToList();

            var stats = list
                .GroupBy(v => v.UserEmail)
                .Select(g => new UserActivityStat
                {
                    UserEmail = g.Key,
                    VisitCount = g.Count(),
                    LastSeen = g.Max(v => v.Timestamp)
                })
                .OrderByDescending(s => s.LastSeen)
                .ToList();

            return new DailyActivityReport
            {
                DisplayDate = displayDate,
                TotalUniqueUsers = stats.Count,
                TotalVisits = list.Count,
                UserStats = stats
            };
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
    }
}
