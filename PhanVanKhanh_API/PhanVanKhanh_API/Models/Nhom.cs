using System.Collections.Generic; // Thêm using này
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhanVanKhanh_API.Models // Namespace đúng
{
    [Table("tbNHOM")] // Ánh xạ với bảng tbNHOM
    public class Nhom
    {
        [Key]
        [Column("MANHOM")]
        public int MaNhom { get; set; }

        [Column("TENNHOM")]
        public string? TenNhom { get; set; }

        // Navigation property để liên kết với ThietBi (Quan trọng)
        public virtual ICollection<ThietBi>? ThietBis { get; set; } = new List<ThietBi>();
    }
}