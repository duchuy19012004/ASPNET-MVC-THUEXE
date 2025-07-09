using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bike.Models
{
    public class HopDong
    {
        [Key]
        public int MaHopDong { get; set; }

        // Liên kết với phiếu đặt chỗ
        [Display(Name = "Phiếu đặt chỗ")]
        public int? MaDatCho { get; set; }

        [ForeignKey("MaDatCho")]
        public DatCho? DatCho { get; set; }

        // Liên kết với User (khách hàng)
        [Display(Name = "Khách hàng")]
        public int? MaKhachHang { get; set; }

        [ForeignKey("MaKhachHang")]
        public User? KhachHang { get; set; }

        // Thông tin khách hàng (backup cho khách vãng lai)
        [Display(Name = "Họ tên khách")]
        [Required]
        [StringLength(100)]
        public string? HoTenKhach { get; set; }

        [Display(Name = "Số điện thoại")]
        [Required]
        [StringLength(20)]
        public string? SoDienThoai { get; set; }

        [Display(Name = "CCCD/CMND")]
        [Required]
        [StringLength(20)]
        public string? SoCCCD { get; set; }

        [Display(Name = "Địa chỉ")]
        [StringLength(255)]
        public string? DiaChi { get; set; }

        // Thời gian thuê
        [Display(Name = "Ngày nhận xe")]
        [Required]
        public DateTime NgayNhanXe { get; set; }

        [Display(Name = "Ngày trả xe dự kiến")]
        [Required]
        public DateTime NgayTraXeDuKien { get; set; }

        [Display(Name = "Ngày trả xe thực tế")]
        public DateTime? NgayTraXeThucTe { get; set; }

        // Tài chính tổng
        [Display(Name = "Tiền cọc")]
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TienCoc { get; set; }

        [Display(Name = "Phụ phí")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PhuPhi { get; set; } = 0;

        [Display(Name = "Tổng tiền")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TongTien { get; set; }

        // Thông tin khác
        [Display(Name = "Ghi chú")]
        [StringLength(500)]
        public string? GhiChu { get; set; }

        [Display(Name = "Ngày tạo hợp đồng")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        [Display(Name = "Trạng thái")]
        [StringLength(50)]
        public string TrangThai { get; set; } = "Đang thuê"; // Đang thuê, Hoàn thành, Hủy

        [Display(Name = "Người tạo")]
        public int? MaNguoiTao { get; set; }

        [ForeignKey("MaNguoiTao")]
        public User? NguoiTao { get; set; }

        // Navigation property cho chi tiết xe trong hợp đồng (quan hệ n-n)
        public virtual ICollection<ChiTietHopDong> ChiTietHopDong { get; set; } = new List<ChiTietHopDong>();

        // Navigation property cho quan hệ 1-1 với HoaDon
        public HoaDon? HoaDon { get; set; }

        // Computed properties
        [NotMapped]
        public int SoNgayThue => ChiTietHopDong.Any() 
            ? ChiTietHopDong.Max(ct => ct.SoNgayThueTinhToan)
            : (NgayTraXeDuKien - NgayNhanXe).Days + 1;

        [NotMapped]
        public decimal TongTienXe => ChiTietHopDong.Sum(ct => ct.ThanhTienTinhToan);

        [NotMapped]
        public decimal TongTienDuKien => TongTienXe + PhuPhi;

        [NotMapped]
        public int SoXeThue => ChiTietHopDong.Count;

        [NotMapped]
        public bool DaCoHoaDon => HoaDon != null;
    }
}