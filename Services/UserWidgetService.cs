using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using JsonCrudApp.Models;

namespace JsonCrudApp.Services
{
    public class UserWidgetService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UserWidgetService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        private string JsonFileName => Path.Combine(_webHostEnvironment.WebRootPath, "data", "user_widgets.json");

        public IEnumerable<UserWidget> GetUserWidgets()
        {
            if (!File.Exists(JsonFileName)) return new List<UserWidget>();

            using (var reader = File.OpenText(JsonFileName))
            {
                return JsonSerializer.Deserialize<UserWidget[]>(reader.ReadToEnd(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? Enumerable.Empty<UserWidget>();
            }
        }

        public UserWidget? GetWidgetsByUser(string email)
        {
            return GetUserWidgets().FirstOrDefault(uw => uw.Email.Equals(email, System.StringComparison.OrdinalIgnoreCase));
        }

        public void SaveUserWidgets(IEnumerable<UserWidget> userWidgets)
        {
            var folder = Path.GetDirectoryName(JsonFileName);
            if (folder != null && !Directory.Exists(folder)) Directory.CreateDirectory(folder);

            using (var stream = File.OpenWrite(JsonFileName))
            {
                stream.SetLength(0);
                JsonSerializer.Serialize(new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }), userWidgets);
            }
        }

        public void UpdateUserWidgets(string email, string widgets)
        {
            var all = GetUserWidgets().ToList();
            var existing = all.FirstOrDefault(uw => uw.Email.Equals(email, System.StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                existing.AllowedWidgets = widgets;
            }
            else
            {
                all.Add(new UserWidget { Email = email, AllowedWidgets = widgets });
            }
            SaveUserWidgets(all);
        }

        public void DeleteUserWidgets(string email)
        {
            var all = GetUserWidgets().ToList();
            var existing = all.FirstOrDefault(uw => uw.Email.Equals(email, System.StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                all.Remove(existing);
                SaveUserWidgets(all);
            }
        }
    }
}
