using System.ComponentModel.DataAnnotations;

namespace bike.Models
{
    public class CartItem
    {
        public int MaXe { get; set; }
        public string TenXe { get; set; }
        public string BienSoXe { get; set; }
        public decimal GiaThue { get; set; }
        public string? HinhAnhXe { get; set; }
        public string? TenLoaiXe { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn ngày nhận xe")]
        [Display(Name = "Ngày nhận xe")]
        [DataType(DataType.Date)]
        public DateTime NgayNhanXe { get; set; } = DateTime.Now.AddDays(1);

        [Required(ErrorMessage = "Vui lòng chọn ngày trả xe")]
        [Display(Name = "Ngày trả xe")]
        [DataType(DataType.Date)]
        public DateTime NgayTraXe { get; set; } = DateTime.Now.AddDays(3);

        [Display(Name = "Ghi chú")]
        [StringLength(500)]
        public string? GhiChu { get; set; }

        // Calculated properties
        public int SoNgayThue => (NgayTraXe - NgayNhanXe).Days + 1;
        public decimal TongTien => GiaThue * SoNgayThue;
        
        // For display
        public string TenXeDay => $"{TenXe} ({NgayNhanXe:dd/MM} - {NgayTraXe:dd/MM})";
    }
} 