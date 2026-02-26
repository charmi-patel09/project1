using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using JsonCrudApp.Models;

namespace JsonCrudApp.Services
{
    public class RoleWidgetService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        // Centralized Widget Definition for scalability
        public static readonly Dictionary<string, string> AvailableWidgets = new()
        {
            { "global-search-hub", "Global Search" },
            { "weather-hub", "Weather" },
            { "currency-hub", "Currency Exchange" },
            { "chrono-hub", "Timezone/Clock" },
            { "news-hub", "Global News" },
            { "emergency-hub", "Emergency Contacts" },
            { "habit-hub", "Habit Tracker" },
            { "pdf-hub", "PDF Converter" },
            { "notes-hub", "Notes" },
            { "translator-hub", "Translator" },
            { "goal-hub", "Smart Goals" }
        };

        public RoleWidgetService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            InitializeDefaultRoles();
        }

        private string JsonFileName => Path.Combine(_webHostEnvironment.WebRootPath, "data", "role_widgets.json");

        public IEnumerable<RoleWidget> GetRoleWidgets()
        {
            if (!File.Exists(JsonFileName)) return new List<RoleWidget>();

            using (var reader = File.OpenText(JsonFileName))
            {
                return JsonSerializer.Deserialize<RoleWidget[]>(reader.ReadToEnd(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? Enumerable.Empty<RoleWidget>();
            }
        }

        public RoleWidget? GetWidgetsByRole(string role)
        {
            return GetRoleWidgets().FirstOrDefault(rw => rw.Role.Equals(role, System.StringComparison.OrdinalIgnoreCase));
        }

        public void SaveRoleWidgets(IEnumerable<RoleWidget> roleWidgets)
        {
            var folder = Path.GetDirectoryName(JsonFileName);
            if (folder != null && !Directory.Exists(folder)) Directory.CreateDirectory(folder);

            using (var stream = File.OpenWrite(JsonFileName))
            {
                stream.SetLength(0);
                JsonSerializer.Serialize(new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }), roleWidgets);
            }
        }

        public void UpdateRoleWidgets(string role, string widgets)
        {
            var all = GetRoleWidgets().ToList();
            var existing = all.FirstOrDefault(rw => rw.Role.Equals(role, System.StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                existing.AllowedWidgets = widgets;
            }
            else
            {
                all.Add(new RoleWidget { Role = role, AllowedWidgets = widgets });
            }
            SaveRoleWidgets(all);
        }

        public void CreateRole(string roleName, string[]? widgets = null)
        {
            var all = GetRoleWidgets().ToList();
            if (all.Any(rw => rw.Role.Equals(roleName, System.StringComparison.OrdinalIgnoreCase))) return;

            string allowedWidgets;
            if (widgets != null && widgets.Length > 0)
            {
                allowedWidgets = string.Join(",", widgets);
            }
            else
            {
                // AUTO-SYNC: Assign all widgets from Admin role if specific list not provided
                var adminRole = all.FirstOrDefault(rw => rw.Role.Equals(UserRole.Admin, System.StringComparison.OrdinalIgnoreCase));
                allowedWidgets = adminRole?.AllowedWidgets ?? string.Join(",", AvailableWidgets.Keys);
            }

            all.Add(new RoleWidget { Role = roleName, AllowedWidgets = allowedWidgets });
            SaveRoleWidgets(all);
        }

        public void DeleteRole(string roleName)
        {
            var all = GetRoleWidgets().ToList();
            var target = all.FirstOrDefault(rw => rw.Role.Equals(roleName, System.StringComparison.OrdinalIgnoreCase));
            if (target != null)
            {
                all.Remove(target);
                SaveRoleWidgets(all);
            }
        }

        private void InitializeDefaultRoles()
        {
            if (!File.Exists(JsonFileName))
            {
                var defaults = new List<RoleWidget>
                {
                    new RoleWidget { Role = UserRole.Admin, AllowedWidgets = string.Join(",", AvailableWidgets.Keys) },
                    new RoleWidget { Role = UserRole.Visitor, AllowedWidgets = "weather-hub,currency-hub,news-hub" },
                    new RoleWidget { Role = UserRole.@private, AllowedWidgets = "notes-hub,translator-hub,emergency-hub,news-hub,chrono-hub,pdf-hub,habit-hub,goal-hub" }
                };
                SaveRoleWidgets(defaults);
            }
        }
    }
}
