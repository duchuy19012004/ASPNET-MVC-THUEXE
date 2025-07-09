using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bike.Models
{
    // Bảng trung gian xử lý quan hệ n-n giữa HopDong và Xe
    public class ChiTietHopDong
    {
        [Key]
        public int MaChiTiet { get; set; }

        // Liên kết với HopDong
        [Required]
        [Display(Name = "Mã hợp đồng")]
        public int MaHopDong { get; set; }

        [ForeignKey("MaHopDong")]
        public HopDong HopDong { get; set; }

        // Liên kết với Xe
        [Required]
        [Display(Name = "Mã xe")]
        public int MaXe { get; set; }

        [ForeignKey("MaXe")]
        public Xe Xe { get; set; }

        // Thông tin riêng cho từng xe trong hợp đồng
        [Display(Name = "Giá thuê/ngày")]
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal GiaThueNgay { get; set; }

        [Display(Name = "Ngày nhận xe")]
        [Required]
        [DataType(DataType.Date)]
        public DateTime NgayNhanXe { get; set; }

        [Display(Name = "Ngày trả xe dự kiến")]
        [Required]
        [DataType(DataType.Date)]
        public DateTime NgayTraXeDuKien { get; set; }

        [Display(Name = "Ngày trả xe thực tế")]
        [DataType(DataType.Date)]
        public DateTime? NgayTraXeThucTe { get; set; }

        [Display(Name = "Số ngày thuê")]
        public int SoNgayThue { get; set; }

        [Display(Name = "Thành tiền")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ThanhTien { get; set; }

        [Display(Name = "Trạng thái xe")]
        [StringLength(50)]
        public string TrangThaiXe { get; set; } = "Đang thuê"; // Đang thuê, Đã trả, Quá hạn

        [Display(Name = "Ghi chú")]
        [StringLength(500)]
        public string? GhiChu { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Computed properties
        [NotMapped]
        public int SoNgayThueTinhToan => NgayTraXeThucTe.HasValue
            ? (NgayTraXeThucTe.Value - NgayNhanXe).Days + 1
            : (NgayTraXeDuKien - NgayNhanXe).Days + 1;

        [NotMapped]
        public decimal ThanhTienTinhToan => GiaThueNgay * SoNgayThueTinhToan;
    }
} 