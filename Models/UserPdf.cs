namespace JsonCrudApp.Models
{
    public class UserPdf
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
