using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using JsonCrudApp.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace JsonCrudApp.Services
{
    public class UserPdfService
    {
        private readonly IWebHostEnvironment _env;

        public UserPdfService(IWebHostEnvironment env)
        {
            _env = env;
        }

        private string JsonFileName
        {
            get { return Path.Combine(_env.WebRootPath, "data", "user_pdfs.json"); }
        }

        public IEnumerable<UserPdf> GetUserPdfs(int userId)
        {
            if (!File.Exists(JsonFileName))
            {
                return new List<UserPdf>();
            }

            using (var jsonFileReader = File.OpenText(JsonFileName))
            {
                var allPdfs = JsonSerializer.Deserialize<List<UserPdf>>(jsonFileReader.ReadToEnd(),
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<UserPdf>();

                return allPdfs.Where(p => p.UserId == userId).OrderByDescending(p => p.CreatedAt);
            }
        }

        public void AddPdf(UserPdf pdf)
        {
            List<UserPdf> allPdfs;
            if (File.Exists(JsonFileName))
            {
                using (var jsonFileReader = File.OpenText(JsonFileName))
                {
                    allPdfs = JsonSerializer.Deserialize<List<UserPdf>>(jsonFileReader.ReadToEnd(),
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }) ?? new List<UserPdf>();
                }
            }
            else
            {
                allPdfs = new List<UserPdf>();
            }

            pdf.Id = allPdfs.Any() ? allPdfs.Max(x => x.Id) + 1 : 1;
            allPdfs.Add(pdf);
            SavePdfs(allPdfs);
        }

        public void DeletePdf(int id, int userId)
        {
            List<UserPdf> allPdfs;
            if (!File.Exists(JsonFileName)) return;

            using (var jsonFileReader = File.OpenText(JsonFileName))
            {
                allPdfs = JsonSerializer.Deserialize<List<UserPdf>>(jsonFileReader.ReadToEnd(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<UserPdf>();
            }

            var pdf = allPdfs.FirstOrDefault(p => p.Id == id && p.UserId == userId);
            if (pdf != null && !string.IsNullOrEmpty(pdf.FilePath))
            {
                // Delete actual file
                var fullPath = Path.Combine(_env.WebRootPath, pdf.FilePath.TrimStart('/').Replace('/', '\\'));
                if (File.Exists(fullPath))
                {
                    try { File.Delete(fullPath); } catch { }
                }

                allPdfs.Remove(pdf);
                SavePdfs(allPdfs);
            }
        }

        public void UpdatePdfName(int id, int userId, string? newName)
        {
            List<UserPdf> allPdfs;
            if (!File.Exists(JsonFileName)) return;

            using (var jsonFileReader = File.OpenText(JsonFileName))
            {
                allPdfs = JsonSerializer.Deserialize<List<UserPdf>>(jsonFileReader.ReadToEnd(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<UserPdf>();
            }

            var pdf = allPdfs.FirstOrDefault(p => p.Id == id && p.UserId == userId);
            if (pdf != null)
            {
                pdf.FileName = newName;
                SavePdfs(allPdfs);
            }
        }

        private void SavePdfs(List<UserPdf> pdfs)
        {
            var folder = Path.GetDirectoryName(JsonFileName);
            if (folder != null && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            using (var outputStream = File.OpenWrite(JsonFileName))
            {
                outputStream.SetLength(0);
                JsonSerializer.Serialize(new Utf8JsonWriter(outputStream, new JsonWriterOptions { Indented = true }), pdfs);
            }
        }
    }
}
