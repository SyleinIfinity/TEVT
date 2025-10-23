using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhanVanKhanh_API.Models
{
    public interface IThietBiRepository
    {
        Task<IEnumerable<ThietBi>> GetThietBiByNhomAsync(int maNhom);
        Task<ThietBi?> GetThietBiByIdAsync(int id);
    }
}