using System.Net.Http.Headers;
using System.Text.Json;

namespace socmed_backend.Services;

public class MultimediaService : IMultimediaService
{
    private readonly HttpClient _httpClient;
    private readonly string _internalApiUrl;
    private readonly string _publicBaseUrl;
    private readonly string _apiKey;

    public MultimediaService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _internalApiUrl = configuration["Multimedia:InternalApiUrl"] ?? "http://file-manager-deploy-server-1:3000";
        _publicBaseUrl = configuration["Multimedia:PublicBaseUrl"] ?? "https://file.polobutporo.xyz";
        _apiKey = configuration["Multimedia:ApiKey"] ?? "wow-such-default-key-very-secure";
        
        // Add API key to all requests
        _httpClient.DefaultRequestHeaders.Add("X-API-Key", _apiKey);
    }

    public async Task<MediaUploadResult?> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        using var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Add(fileContent, "file", fileName);
        
        // Add accessPolicy field to make file public
        var accessPolicyContent = new StringContent("public");
        content.Add(accessPolicyContent, "accessPolicy");

        try
        {
            // Use the single-file upload endpoint (not chunked for simplicity)
            var response = await _httpClient.PostAsync($"{_internalApiUrl}/api/upload", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[MultimediaService] Upload failed: {response.StatusCode} - {error}");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            
            // Parse response based on your file manager's response format
            // The upload endpoint returns: { success: true, files: [{ fileId, jobId, filename }] }
            if (doc.RootElement.TryGetProperty("success", out var success) && success.GetBoolean())
            {
                var files = doc.RootElement.GetProperty("files");
                if (files.GetArrayLength() > 0)
                {
                    var firstFile = files[0];
                    var fileId = firstFile.GetProperty("fileId").GetString()!;
                    
                    // Determine media type from content type
                    string mediaType = contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase) 
                                     ? "video" 
                                     : contentType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase)
                                     ? "audio"
                                     : "image";

                    return new MediaUploadResult(fileId, mediaType);
                }
            }
            
            // Alternative: if response directly has fileId
            if (doc.RootElement.TryGetProperty("fileId", out var fileIdProp))
            {
                var fileId = fileIdProp.GetString()!;
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
        // Use the public stream endpoint with the fileId
        // You can also check file status first if needed
        return $"{_publicBaseUrl}/api/public/stream/{fileId}";
    }
}