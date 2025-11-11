namespace Aishit.Models
{
    public class ScenarioExam
    {
        public string Scenario { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
        public string UserAnswer { get; set; } = string.Empty;
        public string SuggestedAnswer { get; set; } = string.Empty;
        public List<string> KeyConcepts { get; set; } = new();
        public bool IsGraded { get; set; } = false;
    }

    public class ScenarioExamResult
    {
        public List<ScenarioExam> Questions { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}