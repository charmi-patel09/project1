using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using JsonCrudApp.Models;
using Microsoft.AspNetCore.Hosting;

namespace JsonCrudApp.Services
{
    public class JsonFileStudentService
    {
        public JsonFileStudentService(IWebHostEnvironment webHostEnvironment)
        {
            WebHostEnvironment = webHostEnvironment;
        }

        public IWebHostEnvironment WebHostEnvironment { get; }

        private string JsonFileName
        {
            get { return Path.Combine(WebHostEnvironment.WebRootPath, "data", "students.json"); }
        }

        public IEnumerable<Student> GetStudents()
        {
            if (!File.Exists(JsonFileName))
            {
                return new List<Student>();
            }

            using (var jsonFileReader = File.OpenText(JsonFileName))
            {
                return JsonSerializer.Deserialize<Student[]>(jsonFileReader.ReadToEnd(),
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? Enumerable.Empty<Student>();
            }
        }

        public void AddStudent(Student student)
        {
            var students = GetStudents().ToList();
            student.Id = students.Any() ? students.Max(x => x.Id) + 1 : 1;
            students.Add(student);
            SaveStudents(students);
        }

        public void UpdateStudent(Student student)
        {
            var students = GetStudents().ToList();
            var query = students.FirstOrDefault(x => x.Id == student.Id);
            if (query != null)
            {
                query.Name = student.Name;
                query.Email = student.Email;
                query.Password = student.Password;
                query.Age = student.Age;
                query.Course = student.Course;
                query.Role = student.Role;
                query.WidgetPermissions = student.WidgetPermissions;
                SaveStudents(students);
            }
        }

        public void DeleteStudent(int id)
        {
            var students = GetStudents().ToList();
            var student = students.FirstOrDefault(x => x.Id == id);
            if (student != null)
            {
                students.Remove(student);
                SaveStudents(students);
            }
        }

        public Student? GetStudentById(int id)
        {
            return GetStudents().FirstOrDefault(x => x.Id == id);
        }

        private void SaveStudents(IEnumerable<Student> students)
        {
            var folder = Path.GetDirectoryName(JsonFileName);
            if (folder != null && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            using (var outputStream = File.OpenWrite(JsonFileName))
            {
                outputStream.SetLength(0); // Clear existing content
                JsonSerializer.Serialize<IEnumerable<Student>>(
                    new Utf8JsonWriter(outputStream, new JsonWriterOptions
                    {
                        Indented = true
                    }),
                    students
                );
            }
        }
    }
}
