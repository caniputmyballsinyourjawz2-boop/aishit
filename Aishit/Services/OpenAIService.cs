using Aishit.Models;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Text.Json;

namespace Aishit.Services
{
    public class OpenAIService : IAIService
    {
        private readonly ChatClient _chatClient;
        private readonly string _model;

        public OpenAIService(IConfiguration configuration)
        {
            var apiKey = configuration["OneRouter:ApiKey"];

            // FIX: Wrap the API key in ApiKeyCredential
            var client = new OpenAIClient(new ApiKeyCredential(apiKey), new OpenAIClientOptions
            {
                Endpoint = new Uri("https://llm.onerouter.pro/v1")
            });

            _model = "gpt-4o";
            _chatClient = client.GetChatClient(_model);
        }

        public async Task<Summary> SummarizeAsync(string text)
        {
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(@"You are a helpful study assistant that creates clear, structured summaries. 
Always respond with JSON in this exact format:
{
  ""summaryText"": ""A concise paragraph summary"",
  ""keyPoints"": [""Point 1"", ""Point 2"", ""Point 3""],
  ""keyTakeaways"": ""Main lessons or conclusions""
}"),
                new UserChatMessage($@"Summarize this text:

{text}")
            };

            var options = new ChatCompletionOptions
            {
                Temperature = 0.7f,
                ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
            };

            var completion = await _chatClient.CompleteChatAsync(messages, options);
            var jsonContent = completion.Value.Content[0].Text;

            var summaryData = JsonSerializer.Deserialize<SummaryData>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return new Summary
            {
                OriginalText = text,
                SummaryText = summaryData?.SummaryText ?? "",
                KeyPoints = summaryData?.KeyPoints ?? new List<string>(),
                KeyTakeaways = summaryData?.KeyTakeaways ?? ""
            };
        }

        public async IAsyncEnumerable<string> SummarizeStreamingAsync(string text)
        {
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are a helpful study assistant that creates clear, concise summaries with key points and takeaways."),
                new UserChatMessage($@"Summarize this text in a structured way:

**Summary:**
[Write a concise paragraph summary]

**Key Points:**
- [Point 1]
- [Point 2]
- [Point 3]

**Key Takeaways:**
[Main lessons or conclusions]

Text to summarize:
{text}")
            };

            var options = new ChatCompletionOptions
            {
                Temperature = 0.7f
            };

            var streamingUpdates = _chatClient.CompleteChatStreamingAsync(messages, options);

            await foreach (var update in streamingUpdates)
            {
                foreach (var contentPart in update.ContentUpdate)
                {
                    if (!string.IsNullOrEmpty(contentPart.Text))
                    {
                        yield return contentPart.Text;
                    }
                }
            }
        }

        private class SummaryData
        {
            public string SummaryText { get; set; } = string.Empty;
            public List<string> KeyPoints { get; set; } = new();
            public string KeyTakeaways { get; set; } = string.Empty;
        }
    }
}