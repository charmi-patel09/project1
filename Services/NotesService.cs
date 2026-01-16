using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using JsonCrudApp.Models;
using Microsoft.AspNetCore.Hosting;

namespace JsonCrudApp.Services
{
    public class NotesService
    {
        public NotesService(IWebHostEnvironment webHostEnvironment)
        {
            WebHostEnvironment = webHostEnvironment;
        }

        public IWebHostEnvironment WebHostEnvironment { get; }

        private string JsonFileName
        {
            get { return Path.Combine(WebHostEnvironment.WebRootPath, "data", "notes.json"); }
        }

        public IEnumerable<Note> GetAllNotes()
        {
            if (!File.Exists(JsonFileName))
            {
                return new List<Note>();
            }

            using (var jsonFileReader = File.OpenText(JsonFileName))
            {
                var content = jsonFileReader.ReadToEnd();
                if (string.IsNullOrWhiteSpace(content)) return new List<Note>();

                return JsonSerializer.Deserialize<Note[]>(content,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? Enumerable.Empty<Note>();
            }
        }

        public IEnumerable<Note> GetNotesByUser(string userEmail)
        {
            return GetAllNotes().Where(n => n.UserEmail == userEmail);
        }

        public void AddNote(Note note)
        {
            var notes = GetAllNotes().ToList();
            note.Id = notes.Any() ? notes.Max(x => x.Id) + 1 : 1;
            note.CreatedDate = DateTime.Now;
            notes.Add(note);
            SaveNotes(notes);
        }

        public void UpdateNote(Note note)
        {
            var notes = GetAllNotes().ToList();
            var query = notes.FirstOrDefault(x => x.Id == note.Id && x.UserEmail == note.UserEmail);
            if (query != null)
            {
                query.Title = note.Title;
                query.Description = note.Description;
                // CreatedDate usually doesn't change on edit, or updated date is added. keeping simple.
                SaveNotes(notes);
            }
        }

        public void DeleteNote(int id, string userEmail)
        {
            var notes = GetAllNotes().ToList();
            var note = notes.FirstOrDefault(x => x.Id == id && x.UserEmail == userEmail);
            if (note != null)
            {
                notes.Remove(note);
                SaveNotes(notes);
            }
        }

        private void SaveNotes(IEnumerable<Note> notes)
        {
            var folder = Path.GetDirectoryName(JsonFileName);
            if (folder != null && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            using (var outputStream = File.OpenWrite(JsonFileName))
            {
                outputStream.SetLength(0); // Clear existing content
                JsonSerializer.Serialize<IEnumerable<Note>>(
                    new Utf8JsonWriter(outputStream, new JsonWriterOptions
                    {
                        Indented = true
                    }),
                    notes
                );
            }
        }
    }
}
