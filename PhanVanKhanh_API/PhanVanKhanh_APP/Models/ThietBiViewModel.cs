namespace PhanVanKhanh_APP.Models // Namespace đúng
{
    public class ThietBiViewModel
    {
        public int MaThietBi { get; set; } // Khớp
        public string TenThietBi { get; set; } // Khớp
        public decimal? DonGia { get; set; } // Khớp
        public string HinhAnh { get; set; } // Khớp
        public string MoTa { get; set; } // Khớp
        public int MaNhom { get; set; } // Khớp
        public int? SoLuong { get; set; } // Thêm thuộc tính này
    }
}