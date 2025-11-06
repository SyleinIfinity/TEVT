using QLBH_API.Models;

namespace QLBH_API.Services
{
    public interface IGithubStorageService
    {
        Task<GithubUploadResult> UploadImageAsync(IFormFile file);
        Task<byte[]> GetImageAsync(string filename);
    }
}