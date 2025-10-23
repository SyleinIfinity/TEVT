using Microsoft.EntityFrameworkCore;
using PhanVanKhanh_API.Data; // Namespace đúng
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhanVanKhanh_API.Models
{
    public class ThietBiRepository : IThietBiRepository
    {
        private readonly DataContext _context;

        public ThietBiRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ThietBi>> GetThietBiByNhomAsync(int maNhom)
        {
            // Sử dụng LINQ để lấy ThietBi theo MaNhom
            return await _context.ThietBi
                                 .Where(t => t.MaNhom == maNhom)
                                 .ToListAsync();
        }

        public async Task<ThietBi?> GetThietBiByIdAsync(int id)
        {
            // Sử dụng LINQ để tìm ThietBi theo Id
            return await _context.ThietBi.FindAsync(id);
        }
    }
}