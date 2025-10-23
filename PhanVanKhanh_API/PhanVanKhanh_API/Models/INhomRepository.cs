using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhanVanKhanh_API.Models
{
    public interface INhomRepository
    {
        Task<IEnumerable<Nhom>> GetAllNhomAsync();
        Task<Nhom?> GetNhomByIdAsync(int id); // Có thể thêm nếu cần
    }
}