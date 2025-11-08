namespace Aishit.Models
{
    public class QuizQuestion
    {
        public QuestionType Type { get; set; }
        public string Question { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new();
        public int CorrectIndex { get; set; }
        public bool CorrectBoolean { get; set; }
        public string CorrectAnswer { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
    }

    public enum QuestionType
    {
        MultipleChoice,
        TrueFalse,
        Identification,
        ScenarioBased
    }
}