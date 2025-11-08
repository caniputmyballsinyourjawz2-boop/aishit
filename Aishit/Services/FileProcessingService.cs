using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text;

namespace Aishit.Services
{
    public class FileProcessingService : IFileProcessingService
    {
        public async Task<string> ExtractTextFromFileAsync(Stream fileStream, string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLower();

            return extension switch
            {
                ".txt" => await ReadTextFileAsync(fileStream),
                ".pdf" => await ReadPdfFileAsync(fileStream),
                ".docx" => await ReadDocxFileAsync(fileStream),
                _ => throw new NotSupportedException($"File type {extension} is not supported. Please upload .txt, .pdf, or .docx files.")
            };
        }

        private async Task<string> ReadTextFileAsync(Stream stream)
        {
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }

       private async Task<string> ReadPdfFileAsync(Stream stream)
{
    // Copy to memory stream first (supports synchronous reads)
    using var memoryStream = new MemoryStream();
    await stream.CopyToAsync(memoryStream);
    memoryStream.Position = 0;
    
    return await Task.Run(() =>
    {
        using var pdfReader = new PdfReader(memoryStream);
        using var pdfDocument = new PdfDocument(pdfReader);
        
        var text = new StringBuilder();
        
        for (int page = 1; page <= pdfDocument.GetNumberOfPages(); page++)
        {
            var strategy = new SimpleTextExtractionStrategy();
            var pageText = PdfTextExtractor.GetTextFromPage(pdfDocument.GetPage(page), strategy);
            text.AppendLine(pageText);
        }
        
        return text.ToString();
    });
}

       private async Task<string> ReadDocxFileAsync(Stream stream)
{
    // Copy to memory stream first
    using var memoryStream = new MemoryStream();
    await stream.CopyToAsync(memoryStream);
    memoryStream.Position = 0;
    
    return await Task.Run(() =>
    {
        using var wordDocument = WordprocessingDocument.Open(memoryStream, false);
        var body = wordDocument.MainDocumentPart?.Document.Body;
        
        if (body == null)
            return string.Empty;
        
        var text = new StringBuilder();
        
        foreach (var paragraph in body.Elements<Paragraph>())
        {
            text.AppendLine(paragraph.InnerText);
        }
        
        return text.ToString();
    });
}
    }
}