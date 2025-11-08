using Aishit.Models;

namespace Aishit.Services
{
    public interface IAIService
    {
        Task<Summary> SummarizeAsync(string text);
        IAsyncEnumerable<string> SummarizeStreamingAsync(string text);
        Task<List<Flashcard>> GenerateFlashcardsAsync(string text, int count = 10);
        Task<List<QuizQuestion>> GenerateQuizAsync(string text, int questionCount = 12);
    }
}