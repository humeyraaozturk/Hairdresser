using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

public class RapidAPIService
{
    private readonly HttpClient _httpClient;

    public RapidAPIService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Add("x-rapidapi-key", "3aac5d3b0emsh2668d32fc363770p1230d9jsn076730848416");
        _httpClient.DefaultRequestHeaders.Add("x-rapidapi-host", "hairstyle-changer-pro.p.rapidapi.com");
    }

    // Fotoğraf analizi yapan metod
    public async Task<string> AnalyzeImageAsync(Stream imageStream)
    {
        var content = new MultipartFormDataContent();
        //var fileContent = new ByteArrayContent(imageBytes);
        //fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
        content.Add(new StreamContent(imageStream), "file", "image.jpg");

        // API'ye fotoğrafı gönder
        var response = await _httpClient.PostAsync("https://hairstyle-changer-pro.p.rapidapi.com/api/rapidapi/query-async-task-result", content);
        // Yanıtı loglayın
        Console.WriteLine($"API Response: {response.StatusCode}");
        
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Error: {response.ReasonPhrase}");
            return null;
        }

        string responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"API Response Content: {responseContent}");
        return responseContent;
    }
}
