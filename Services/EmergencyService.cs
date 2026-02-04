using System.Text.Json;
using JsonCrudApp.Models;
using Microsoft.AspNetCore.Hosting;

namespace JsonCrudApp.Services
{
    public class EmergencyService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string _jsonFilePath;

        public EmergencyService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            _jsonFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "data", "emergency_numbers.json");
        }

        public async Task<List<EmergencyInfo>> GetAllAsync()
        {
            if (!File.Exists(_jsonFilePath)) return new List<EmergencyInfo>();

            try
            {
                using var stream = File.OpenRead(_jsonFilePath);
                return await JsonSerializer.DeserializeAsync<List<EmergencyInfo>>(stream) ?? new List<EmergencyInfo>();
            }
            catch
            {
                return new List<EmergencyInfo>();
            }
        }

        public async Task<EmergencyInfo?> GetByCountryCodeAsync(string code)
        {
            var all = await GetAllAsync();
            return all.FirstOrDefault(e => e.CountryCode.Equals(code, StringComparison.OrdinalIgnoreCase));
        }
    }
}
