using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;

public class RelayService
{
    private readonly HttpClient _httpClient;
    private const string OpenAiEndpoint = "https://api.openai.com/v1/chat/completions";
    private const string ApiKey = ""; // Use secure config in real use

    public RelayService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> SendPromptToOpenAI(string prompt)
    {
        var requestBody = new
        {
            model = "gpt-4o",
            messages = new[] { new { role = "user", content = prompt } }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, OpenAiEndpoint);
        request.Headers.Add("Authorization", $"Bearer {ApiKey}");
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        using var content = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(content);
        return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
    }
}