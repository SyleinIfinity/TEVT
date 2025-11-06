using QLBH_API.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace QLBH_API.Services
{
    public class GithubStorageService : IGithubStorageService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _apiClient; // Dùng để gọi api.github.com (có auth)
        private readonly HttpClient _rawClient; // Dùng để gọi raw.githubusercontent.com (không auth)

        private readonly string _owner;
        private readonly string _repo;
        private readonly string _basePath;
        private readonly string _branch;

        public GithubStorageService(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _config = config.GetSection("GitHubStorage");
            _owner = _config["RepositoryOwner"];
            _repo = _config["RepositoryName"];
            _basePath = _config["BasePath"];
            _branch = _config["Branch"];

            _apiClient = httpClientFactory.CreateClient("githubApi");
            _rawClient = httpClientFactory.CreateClient("githubRaw");
        }

        public async Task<GithubUploadResult> UploadImageAsync(IFormFile file)
        {
            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var githubApiUrl = $"https://api.github.com/repos/{_owner}/{_repo}/contents/{_basePath}/{uniqueFileName}";

            string base64Content;
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                base64Content = Convert.ToBase64String(memoryStream.ToArray());
            }

            var payload = new
            {
                message = $"Upload image: {uniqueFileName}",
                content = base64Content,
                branch = _branch
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await _apiClient.PutAsync(githubApiUrl, httpContent);

            if (!response.IsSuccessStatusCode)
            {
                // Log lỗi
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to upload to GitHub: {errorContent}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            using (var doc = JsonDocument.Parse(jsonResponse))
            {
                return new GithubUploadResult
                {
                    FileName = uniqueFileName,
                    DownloadUrl = doc.RootElement.GetProperty("content").GetProperty("download_url").GetString()
                };
            }
        }

        public async Task<byte[]> GetImageAsync(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                return null;
            }

            var rawUrl = $"https://raw.githubusercontent.com/{_owner}/{_repo}/{_branch}/{_basePath}/{filename}";

            try
            {
                var response = await _rawClient.GetAsync(rawUrl);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }
                return null;
            }
            catch (Exception)
            {
                // Log lỗi
                return null;
            }
        }

        // Helper để lấy MimeType, có thể cải tiến thêm
        private string GetMimeType(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            switch (ext)
            {
                case ".png": return "image/png";
                case ".jpg":
                case ".jpeg": return "image/jpeg";
                case ".gif": return "image/gif";
                default: return "application/octet-stream";
            }
        }
    }
}