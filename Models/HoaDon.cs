using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bike.Models
{
    public class HoaDon
    {
        [Key]
        public int MaHoaDon { get; set; }

        [Required]
        [Display(Name = "Mã hợp đồng")]
        public int MaHopDong { get; set; }

        [ForeignKey("MaHopDong")]
        public HopDong? HopDong { get; set; }

        [Required]
        [Display(Name = "Ngày thanh toán")]
        [DataType(DataType.Date)]
        public DateTime NgayThanhToan { get; set; }

        [Required]
        [Display(Name = "Số tiền")]
        [DataType(DataType.Currency)]
        public decimal SoTien { get; set; }

        [Required]
        [Display(Name = "Trạng thái")]
        [StringLength(50)]
        public string TrangThai { get; set; } = "Đã thanh toán";

        [Display(Name = "Ghi chú")]
        [StringLength(500)]
        public string? GhiChu { get; set; }

        [Required]
        [Display(Name = "Ngày tạo")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        [Display(Name = "Người tạo")]
        public int? MaNguoiTao { get; set; }

        [ForeignKey("MaNguoiTao")]
        public User? NguoiTao { get; set; }
    }
}