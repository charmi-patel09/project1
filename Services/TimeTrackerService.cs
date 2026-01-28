using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using JsonCrudApp.Models;
using Microsoft.AspNetCore.Hosting;

namespace JsonCrudApp.Services
{
    public class TimeTrackerService
    {
        public TimeTrackerService(IWebHostEnvironment webHostEnvironment)
        {
            WebHostEnvironment = webHostEnvironment;
        }

        public IWebHostEnvironment WebHostEnvironment { get; }

        private string JsonFileName
        {
            get { return Path.Combine(WebHostEnvironment.WebRootPath, "data", "timeentries.json"); }
        }

        public IEnumerable<TimeEntry> GetAllEntries()
        {
            if (!File.Exists(JsonFileName))
            {
                return new List<TimeEntry>();
            }

            using (var jsonFileReader = File.OpenText(JsonFileName))
            {
                var content = jsonFileReader.ReadToEnd();
                if (string.IsNullOrWhiteSpace(content)) return new List<TimeEntry>();

                try
                {
                    return JsonSerializer.Deserialize<TimeEntry[]>(content,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }) ?? Enumerable.Empty<TimeEntry>();
                }
                catch
                {
                    return Enumerable.Empty<TimeEntry>();
                }
            }
        }

        public IEnumerable<TimeEntry> GetEntriesByUser(string userEmail)
        {
            // Order by most recent first
            return GetAllEntries().Where(n => n.UserEmail == userEmail).OrderByDescending(x => x.StartTime);
        }

        public void AddEntry(TimeEntry entry)
        {
            var entries = GetAllEntries().ToList();
            entry.Id = entries.Any() ? entries.Max(x => x.Id) + 1 : 1;

            // Ensure we don't duplicate if called erroneously, though ID gen handles it.
            entries.Add(entry);
            SaveEntries(entries);
        }

        public void UpdateEntry(TimeEntry entry)
        {
            var entries = GetAllEntries().ToList();
            var existing = entries.FirstOrDefault(e => e.Id == entry.Id && e.UserEmail == entry.UserEmail);
            if (existing != null)
            {
                existing.TaskName = entry.TaskName;
                existing.StartTime = entry.StartTime;
                existing.EndTime = entry.EndTime;
                existing.DurationSeconds = entry.DurationSeconds;
                SaveEntries(entries);
            }
        }

        public void DeleteEntry(int id, string userEmail)
        {
            var entries = GetAllEntries().ToList();
            var entry = entries.FirstOrDefault(e => e.Id == id && e.UserEmail == userEmail);
            if (entry != null)
            {
                entries.Remove(entry);
                SaveEntries(entries);
            }
        }

        private void SaveEntries(IEnumerable<TimeEntry> entries)
        {
            var folder = Path.GetDirectoryName(JsonFileName);
            if (folder != null && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            using (var outputStream = File.OpenWrite(JsonFileName))
            {
                outputStream.SetLength(0); // Clear existing content
                JsonSerializer.Serialize<IEnumerable<TimeEntry>>(
                    new Utf8JsonWriter(outputStream, new JsonWriterOptions
                    {
                        Indented = true
                    }),
                    entries
                );
            }
        }
    }
}
