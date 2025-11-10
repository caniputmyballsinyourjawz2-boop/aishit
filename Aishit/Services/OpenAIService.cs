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
            
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("OneRouter API key is not configured in appsettings.json");
            }

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
                new UserChatMessage($@"Summarize this text:{text}")
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

        public async Task<List<Flashcard>> GenerateFlashcardsAsync(string text, int count = 10)
        {
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(@"You are a study assistant that creates effective flashcards. 
Always respond with ONLY valid JSON in this exact format:
{
  ""flashcards"": [
    {""front"": ""Question or term"", ""back"": ""Answer or definition""},
    {""front"": ""Question or term"", ""back"": ""Answer or definition""}
  ]
}"),
                new UserChatMessage($@"Create {count} flashcards from this content. Focus on key concepts, definitions, and important facts.

Text:
{text}")
            };

            var options = new ChatCompletionOptions
            {
                Temperature = 0.7f,
                ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
            };

            var completion = await _chatClient.CompleteChatAsync(messages, options);
            var jsonContent = completion.Value.Content[0].Text;

            var wrapper = JsonSerializer.Deserialize<FlashcardWrapper>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return wrapper?.Flashcards ?? new List<Flashcard>();
        }

        public async Task<List<QuizQuestion>> GenerateQuizAsync(string text, int questionCount = 40)
        {
            int questionsPerType = questionCount / 4;
            
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(@"You are a study assistant that creates diverse quizzes with different question types.
Always respond with ONLY valid JSON in this exact format:
{
  ""questions"": [
    {
      ""type"": ""MultipleChoice"",
      ""question"": ""Question text?"",
      ""options"": [""Option A"", ""Option B"", ""Option C"", ""Option D""],
      ""correctIndex"": 0,
      ""correctBoolean"": false,
      ""correctAnswer"": """",
      ""explanation"": ""Why this is correct""
    },
    {
      ""type"": ""TrueFalse"",
      ""question"": ""Statement to evaluate"",
      ""options"": [],
      ""correctIndex"": 0,
      ""correctBoolean"": true,
      ""correctAnswer"": """",
      ""explanation"": ""Why this is true/false""
    },
    {
      ""type"": ""Identification"",
      ""question"": ""What is this term/concept?"",
      ""options"": [],
      ""correctIndex"": 0,
      ""correctBoolean"": false,
      ""correctAnswer"": ""The correct term"",
      ""explanation"": ""Explanation of the term""
    },
    {
      ""type"": ""ScenarioBased"",
      ""question"": ""Given this scenario... what would happen?"",
      ""options"": [""Option A"", ""Option B"", ""Option C"", ""Option D""],
      ""correctIndex"": 0,
      ""correctBoolean"": false,
      ""correctAnswer"": """",
      ""explanation"": ""Why this is the best answer""
    }
  ]
}"),
                new UserChatMessage($@"Generate {questionCount} questions from this text with an equal mix:
- {questionsPerType} Multiple Choice questions
- {questionsPerType} True/False questions
- {questionsPerType} Identification questions
- {questionsPerType} Scenario-Based questions

Text:
{text}")
            };

            var options = new ChatCompletionOptions
            {
                Temperature = 0.7f,
                ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
            };

            var completion = await _chatClient.CompleteChatAsync(messages, options);
            var jsonContent = completion.Value.Content[0].Text;

            var wrapper = JsonSerializer.Deserialize<QuizWrapper>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return wrapper?.Questions ?? new List<QuizQuestion>();
        }

        // Helper classes for JSON deserialization
        private class SummaryData
        {
            public string SummaryText { get; set; } = string.Empty;
            public List<string> KeyPoints { get; set; } = new();
            public string KeyTakeaways { get; set; } = string.Empty;
        }

        private class FlashcardWrapper
        {
            public List<Flashcard> Flashcards { get; set; } = new();
        }

        private class QuizWrapper
        {
            public List<QuizQuestion> Questions { get; set; } = new();
        }
    }
}