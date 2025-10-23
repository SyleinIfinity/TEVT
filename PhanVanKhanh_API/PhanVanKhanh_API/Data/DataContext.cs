using Microsoft.EntityFrameworkCore;
using PhanVanKhanh_API.Models; // Sử dụng namespace Models của API

namespace PhanVanKhanh_API.Data // Namespace Data của API
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        // DbSet sử dụng tên lớp Model (Nhom, ThietBi)
        public DbSet<Nhom> Nhom { get; set; }
        public DbSet<ThietBi> ThietBi { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Cấu hình kiểu dữ liệu decimal cho DonGia trong lớp ThietBi
            modelBuilder.Entity<ThietBi>()
                .Property(p => p.DonGia)
                .HasColumnType("numeric(18, 0)");

            // Thiết lập mối quan hệ One-to-Many giữa Nhom và ThietBi
            modelBuilder.Entity<Nhom>()
                .HasMany(n => n.ThietBis) // Một Nhom có nhiều ThietBi
                .WithOne(t => t.Nhom)     // Một ThietBi thuộc về một Nhom
                .HasForeignKey(t => t.MaNhom); // Khóa ngoại là MaNhom trong ThietBi

            // Gọi base method
            base.OnModelCreating(modelBuilder);
        }
    }
}