using System.Net.Http.Headers;
using System.Text.Json;

namespace socmed_backend.Services;

public class MultimediaService : IMultimediaService
{
    private readonly HttpClient _httpClient;
    private readonly string _internalApiUrl;
    private readonly string _publicBaseUrl;

    public MultimediaService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _internalApiUrl = configuration["Multimedia:InternalApiUrl"] ?? "http://localhost:3000";
        _publicBaseUrl = configuration["Multimedia:PublicBaseUrl"] ?? "https://api.file.polobutporo.xyz";
    }

    public async Task<MediaUploadResult?> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        using var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Add(fileContent, "file", fileName);

        try
        {
            var response = await _httpClient.PostAsync($"{_internalApiUrl}/upload", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            
            if (doc.RootElement.TryGetProperty("success", out var success) && success.GetBoolean())
            {
                var data = doc.RootElement.GetProperty("data");
                var fileId = data.GetProperty("fileId").GetString()!;
                
                // Use the contentType passed to this method for reliable detection
                string mediaType = contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase) 
                                 ? "video" : "image";

                return new MediaUploadResult(fileId, mediaType);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MultimediaService] Upload failed: {ex.Message}");
        }

        return null;
    }

    public string GetPublicUrl(string fileId)
    {
        return $"{_publicBaseUrl}/files/{fileId}/stream";
    }
}
