using Microsoft.Extensions.Logging;

namespace AideMemoire.Infrastructure.Services;

public class OnnxModelDownloadService {
    private const int BufferSize = 1024 * 1024;

    private readonly ILogger<OnnxModelDownloadService> _logger;

    private readonly IHttpClientFactory _httpClientFactory;

    public OnnxModelDownloadService(ILogger<OnnxModelDownloadService> logger, IHttpClientFactory httpClientFactory) {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    private readonly DownloadableFile[] files = [
            new DownloadableFile("minilm-l12-v2.onnx", "https://huggingface.co/sentence-transformers/all-MiniLM-L12-v2/resolve/main/onnx/model.onnx"),
            new DownloadableFile("minilm-l12-v2_vocab.txt", "https://huggingface.co/sentence-transformers/all-MiniLM-L12-v2/raw/main/vocab.txt")
        ];

    public async Task DownloadModelsAsync() {
        // initialize directory
        var modelsDirectory = Path.Combine(AppContext.BaseDirectory, "Models");
        if (!Directory.Exists(modelsDirectory)) {
            _logger.LogInformation("Creating models directory at {ModelsDirectory}", modelsDirectory);
            Directory.CreateDirectory(modelsDirectory);
        }

        // download any missing files
        foreach (var modelFile in files) {
            var filePath = GetPath(modelFile.FileName);

            if (!File.Exists(filePath))
                await DownloadFileWithProgressAsync(modelFile.Url, filePath, modelFile.FileName);
        }
    }

    private async Task DownloadFileWithProgressAsync(string url, string filePath, string fileName) {
        try {
            using var _httpClient = _httpClientFactory.CreateClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(10);

            using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? 0;
            var totalMB = totalBytes / (1024.0 * 1024.0);

            using var contentStream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize, true);

            var buffer = new byte[BufferSize];
            var totalBytesRead = 0L;
            int bytesRead;

            _logger.LogInformation("Starting download of {fileName} ({totalMB:F1})", fileName, totalMB);

            var lastProgressUpdate = DateTime.Now;

            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0) {
                await fileStream.WriteAsync(buffer, 0, bytesRead);
                totalBytesRead += bytesRead;

                if (DateTime.Now - lastProgressUpdate > TimeSpan.FromSeconds(5)) {
                    lastProgressUpdate = DateTime.Now;

                    var progressPercent = (double)totalBytesRead / totalBytes * 100;
                    var downloadedMB = totalBytesRead / (1024.0 * 1024.0);
                    _logger.LogInformation("...{DownloadedMB:F1} MB ({ProgressPercent:F1}%)", downloadedMB, progressPercent);
                }
            }

            var finalMB = totalBytesRead / (1024.0 * 1024.0);
            _logger.LogInformation("...{finalMB:F1} MB OK", finalMB);
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Failed to download {FileName} from {Url}", fileName, url);
            throw;
        }
    }

    public static string GetPath(string fileName) =>
        Path.Combine(AppContext.BaseDirectory, "Models", fileName);

    private record DownloadableFile(string FileName, string Url);
}
