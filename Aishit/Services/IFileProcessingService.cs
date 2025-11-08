namespace Aishit.Services
{
    public interface IFileProcessingService
    {
        Task<string> ExtractTextFromFileAsync(Stream fileStream, string fileName);
    }
}