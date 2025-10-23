using Microsoft.AspNetCore.Mvc;
using PhanVanKhanh_API.Models; // Namespace đúng
using System.Threading.Tasks;

namespace PhanVanKhanh_API.Controllers // Namespace đúng
{
    [ApiController]
    [Route("api/[controller]")]
    public class ThietBiController : ControllerBase
    {
        private readonly IThietBiRepository _thietBiRepository; // Inject interface

        public ThietBiController(IThietBiRepository thietBiRepository) // Nhận dependency
        {
            _thietBiRepository = thietBiRepository;
        }

        // GET: api/ThietBi/TheoNhom/{maNhom}
        [HttpGet("TheoNhom/{maNhom}")]
        public async Task<IActionResult> GetThietBiByNhom(int maNhom)
        {
            var thietBis = await _thietBiRepository.GetThietBiByNhomAsync(maNhom); // Gọi repo
            return Ok(thietBis);
        }

        // GET: api/ThietBi/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetThietBiById(int id)
        {
            var thietBi = await _thietBiRepository.GetThietBiByIdAsync(id); // Gọi repo
            if (thietBi == null)
            {
                return NotFound();
            }
            return Ok(thietBi);
        }
    }
}