using Microsoft.AspNetCore.Mvc;
using QLBH_API.Services;

namespace QLBH_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly IGithubStorageService _storageService;

        public ImageController(IGithubStorageService storageService)
        {
            _storageService = storageService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file uploaded." });
            }

            try
            {
                var result = await _storageService.UploadImageAsync(file);
                // Trả về cả filename để lưu DB và URL đầy đủ (nếu cần)
                return Ok(new { fileName = result.FileName, url = result.DownloadUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("get/{filename}")]
        public async Task<IActionResult> GetImage(string filename)
        {
            var imageData = await _storageService.GetImageAsync(filename);
            if (imageData == null)
            {
                // Có thể trả về 1 ảnh placeholder mặc định
                return NotFound();
            }

            // Trả về nội dung ảnh
            return File(imageData, GetMimeType(filename)); // Cần hàm GetMimeType
        }

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