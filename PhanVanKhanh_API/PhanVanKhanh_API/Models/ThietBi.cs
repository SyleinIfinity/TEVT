using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhanVanKhanh_API.Models // Namespace đúng
{
    [Table("tbTHIETBI")] // Ánh xạ với bảng tbTHIETBI
    public class ThietBi
    {
        [Key]
        [Column("MATHIETBI")]
        public int MaThietBi { get; set; }

        [Column("TENTHIETBI")]
        public string TenThietBi { get; set; }

        [Column("DONVITINH")]
        public string? DonViTinh { get; set; }

        [Column("SOLUONG")]
        public int? SoLuong { get; set; }

        [Column("DONGIA")]
        public decimal? DonGia { get; set; }

        [Column("HINHANH")]
        public string? HinhAnh { get; set; }

        [Column("MOTA")]
        public string? MoTa { get; set; }

        [Column("MANHOM")]
        public int? MaNhom { get; set; }

        // Navigation property để liên kết với Nhom (Quan trọng)
        [ForeignKey("MaNhom")]
        public virtual Nhom? Nhom { get; set; }
    }
}