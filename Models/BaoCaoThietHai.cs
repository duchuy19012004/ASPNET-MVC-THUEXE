using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bike.Models
{
    public class BaoCaoThietHai
    {
        [Key]
        public int MaBaoCao { get; set; }

        // Liên kết với ChiTietHopDong
        [Required]
        [Display(Name = "Mã chi tiết hợp đồng")]
        public int MaChiTiet { get; set; }

        [ForeignKey("MaChiTiet")]
        public ChiTietHopDong? ChiTietHopDong { get; set; }

        // Thông tin thiệt hại
        [Required]
        [Display(Name = "Loại thiệt hại")]
        [StringLength(50)]
        public string LoaiThietHai { get; set; } // Hư hỏng nhẹ, Hư hỏng nặng, Mất, Tai nạn

        [Required]
        [Display(Name = "Mô tả chi tiết")]
        [StringLength(2000)]
        public string MoTaChiTiet { get; set; }

        [Display(Name = "Ngày phát hiện")]
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime NgayPhatHien { get; set; } = DateTime.Now;

        [Display(Name = "Vị trí thiệt hại")]
        [StringLength(500)]
        public string? ViTriThietHai { get; set; }

        // Thông tin tài chính
        [Display(Name = "Chi phí sửa chữa ước tính")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ChiPhiSuaChuaUocTinh { get; set; } = 0;

        [Display(Name = "Chi phí sửa chữa thực tế")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ChiPhiSuaChuaThucTe { get; set; } = 0;

        [Display(Name = "Phí đền bù khách hàng")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? PhiDenBuKhachHang { get; set; } = 0;

        [Display(Name = "Giá trị xe trước khi hỏng")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? GiaTriXeTruocKhiHong { get; set; } = 0;

        [Display(Name = "Giá trị xe sau khi hỏng")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? GiaTriXeSauKhiHong { get; set; } = 0;

        // Thông tin xử lý
        [Display(Name = "Trạng thái xử lý")]
        [StringLength(50)]
        public string TrangThaiXuLy { get; set; } = "Chờ xử lý"; // Chờ xử lý, Đang sửa chữa, Hoàn thành, Không thể sửa

        [Display(Name = "Ghi chú")]
        [StringLength(1000)]
        public string? GhiChu { get; set; }

        [Display(Name = "Người tạo báo cáo")]
        public int? MaNguoiTao { get; set; }

        [ForeignKey("MaNguoiTao")]
        public User? NguoiTao { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        [Display(Name = "Ngày cập nhật")]
        public DateTime NgayCapNhat { get; set; } = DateTime.Now;

        // Thông tin thanh toán
        [Display(Name = "Số tiền đã thanh toán")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? SoTienDaThanhToan { get; set; } = 0;

        [Display(Name = "Ngày thanh toán")]
        [DataType(DataType.DateTime)]
        public DateTime? NgayThanhToan { get; set; }

        [Display(Name = "Ghi chú thanh toán")]
        [StringLength(500)]
        public string? GhiChuThanhToan { get; set; }

        [Display(Name = "Trạng thái thanh toán")]
        [StringLength(50)]
        public string TrangThaiThanhToan { get; set; } = "Chưa thanh toán"; // Chưa thanh toán, Đã thanh toán một phần, Đã thanh toán đủ

        // Computed properties
        [NotMapped]
        public decimal TongThietHai => (GiaTriXeTruocKhiHong ?? 0) - (GiaTriXeSauKhiHong ?? 0);

        [NotMapped]
        public decimal TyLeDenBu => (GiaTriXeTruocKhiHong ?? 0) > 0 
            ? ((PhiDenBuKhachHang ?? 0) / (GiaTriXeTruocKhiHong ?? 1)) * 100 
            : 0;

        [NotMapped]
        public bool LaThietHaiNang => LoaiThietHai == "Hư hỏng nặng" || LoaiThietHai == "Mất" || LoaiThietHai == "Tai nạn";

        [NotMapped]
        public decimal SoTienConLai => (ChiPhiSuaChuaUocTinh ?? 0) - (SoTienDaThanhToan ?? 0);

        [NotMapped]
        public bool DaThanhToanDu => (SoTienDaThanhToan ?? 0) >= (ChiPhiSuaChuaUocTinh ?? 0);
    }
} 