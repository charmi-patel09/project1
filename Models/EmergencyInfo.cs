namespace JsonCrudApp.Models
{
    public class EmergencyInfo
    {
        public string CountryName { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public List<string> Police { get; set; } = new List<string>();
        public List<string> Ambulance { get; set; } = new List<string>();
        public List<string> Fire { get; set; } = new List<string>();
        public List<string> Dispatch { get; set; } = new List<string>(); // General/Unified (e.g. 112, 911)
    }
}
