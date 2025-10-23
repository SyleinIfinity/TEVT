using Microsoft.EntityFrameworkCore;
using PhanVanKhanh_API.Data; // Namespace đúng
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhanVanKhanh_API.Models
{
    public class NhomRepository : INhomRepository
    {
        private readonly DataContext _context;

        public NhomRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Nhom>> GetAllNhomAsync()
        {
            // Sử dụng LINQ để lấy tất cả Nhom
            return await _context.Nhom.ToListAsync();
        }

        public async Task<Nhom?> GetNhomByIdAsync(int id)
        {
            // Sử dụng LINQ để tìm Nhom theo Id
            return await _context.Nhom.FindAsync(id);
        }
    }
}