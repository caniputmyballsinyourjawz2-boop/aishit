namespace Aishit.Models
{
    public class Summary
    {
        public string OriginalText { get; set; } = string.Empty;
        public string SummaryText { get; set; } = string.Empty;
        public List<string> KeyPoints { get; set; } = new();
        public string KeyTakeaways { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}