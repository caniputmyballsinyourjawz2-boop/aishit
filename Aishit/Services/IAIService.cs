using Aishit.Models;

namespace Aishit.Services
{
    public interface IAIService
    {
        Task<Summary> SummarizeAsync(string text);
        IAsyncEnumerable<string> SummarizeStreamingAsync(string text);
    }
}