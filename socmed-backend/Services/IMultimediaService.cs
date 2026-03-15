namespace socmed_backend.Services;

public record MediaUploadResult(string FileId, string MediaType);

public interface IMultimediaService
{
    /// <summary>
    /// Forwards a file to the File Manager API.
    /// </summary>
    /// <param name="fileStream">The file content stream.</param>
    /// <param name="fileName">The original filename.</param>
    /// <param name="contentType">The MIME type.</param>
    /// <returns>The result containing File ID and Type.</returns>
    Task<MediaUploadResult?> UploadFileAsync(Stream fileStream, string fileName, string contentType);

    /// <summary>
    /// Generates a public URL for a given File ID.
    /// </summary>
    /// <param name="fileId">The UUID of the file.</param>
    /// <returns>A full URL to the streaming endpoint.</returns>
    string GetPublicUrl(string fileId);
}
