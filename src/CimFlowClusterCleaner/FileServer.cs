using System.Net.Http.Json;

namespace CimFlowClusterCleaner;

internal static class FileServer
{
    public static async Task<IEnumerable<FileSystemEntry>> GetFilesAsync(HttpClient client,  Uri uri)
    {
        var requestUri = new Uri(uri, "?json");
        var response = await client.GetAsync(requestUri).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new FileServerResponseException(
                $"Received bad status code: '{response.StatusCode}' from calling endpoint: '{requestUri}'.");
        }

        var fileSystemEntries = await response.Content
            .ReadFromJsonAsync<IEnumerable<FileSystemEntry>>()
            .ConfigureAwait(false);

        return fileSystemEntries?.Where(x => !x.IsDirectory) ??
            throw new FileServerResponseException("Could not deserialize response from server.");
    }

    public static async Task DeleteFileAsync(HttpClient httpClient, Uri uri)
    {
        var response = await httpClient.DeleteAsync(uri).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            throw new DeleteFileException(
                $"Could not delete file in resource path: '{uri}'. Error message: '{errorMessage}'.");
        }
    }
}

internal sealed class FileServerResponseException : Exception
{
    public FileServerResponseException() { }
    public FileServerResponseException(string message) : base(message) { }
    public FileServerResponseException(string message, Exception innerException) : base(message, innerException) { }
}

internal sealed class DeleteFileException : Exception
{
    public DeleteFileException() {}
    public DeleteFileException(string? message) : base(message) {}
    public DeleteFileException(string? message, Exception? innerException) : base(message, innerException) {}
}
