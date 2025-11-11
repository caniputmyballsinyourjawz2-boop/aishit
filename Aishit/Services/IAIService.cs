using Aishit.Models;

namespace Aishit.Services
{
    public enum SummaryLength
    {
        Brief,
        Detailed,
        Comprehensive
    }

    // ADD THIS ENUM HERE
    public enum ScenarioDifficulty 
    { 
        Easy, 
        Medium, 
        Hard 
    }

    public interface IAIService
    {
        Task<Summary> SummarizeAsync(string text, SummaryLength length = SummaryLength.Detailed);
        IAsyncEnumerable<string> SummarizeStreamingAsync(string text);
        Task<List<Flashcard>> GenerateFlashcardsAsync(string text, int count = 10);
        Task<List<QuizQuestion>> GenerateQuizAsync(string text, int questionCount = 12);
        
        // UPDATE THIS LINE - add difficulty parameter
        Task<ScenarioExamResult> GenerateScenarioExamAsync(
            string text, 
            int questionCount = 10, 
            ScenarioDifficulty difficulty = ScenarioDifficulty.Medium);
    }
}