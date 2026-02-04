
namespace JsonCrudApp.Models
{
    public class UserVisit
    {
        public string UserEmail { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string PageUrl { get; set; } = string.Empty;
    }
}
