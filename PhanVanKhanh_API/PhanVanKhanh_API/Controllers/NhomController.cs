using Microsoft.AspNetCore.Mvc;
using PhanVanKhanh_API.Models; // Namespace đúng
using System.Threading.Tasks;

namespace PhanVanKhanh_API.Controllers // Namespace đúng
{
    [ApiController]
    [Route("api/[controller]")]
    public class NhomController : ControllerBase
    {
        private readonly INhomRepository _nhomRepository; // Inject interface

        public NhomController(INhomRepository nhomRepository) // Nhận dependency
        {
            _nhomRepository = nhomRepository;
        }

        // GET: api/Nhom
        [HttpGet]
        public async Task<IActionResult> GetAllNhom()
        {
            var nhoms = await _nhomRepository.GetAllNhomAsync(); // Gọi phương thức của repo
            return Ok(nhoms);
        }
    }
}