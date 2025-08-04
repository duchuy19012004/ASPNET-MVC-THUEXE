using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bike.Models
{
    public class ThietHai
    {
        [Key]
        public int MaThietHai { get; set; }

        // Liên kết với xe bị thiệt hại
        [Required(ErrorMessage = "Vui lòng chọn xe bị thiệt hại")]
        [Display(Name = "Xe bị thiệt hại")]
        public int MaXe { get; set; }

        [ForeignKey("MaXe")]
        public Xe? Xe { get; set; }

        // Liên kết với hợp đồng (nếu có)
        [Display(Name = "Hợp đồng liên quan")]
        public int? MaHopDong { get; set; }

        [ForeignKey("MaHopDong")]
        public HopDong? HopDong { get; set; }

        // Thông tin thiệt hại
        [Required(ErrorMessage = "Loại thiệt hại là bắt buộc")]
        [Display(Name = "Loại thiệt hại")]
        [StringLength(50)]
        public string LoaiThietHai { get; set; } // Mất xe, Hư hỏng phụ kiện, Hư hỏng thân xe, Khác

        [Display(Name = "Mô tả thiệt hại")]
        [StringLength(1000)]
        public string? MoTaThietHai { get; set; }

        [Display(Name = "Ngày xảy ra thiệt hại")]
        [Required(ErrorMessage = "Ngày xảy ra thiệt hại là bắt buộc")]
        [DataType(DataType.Date)]
        public DateTime NgayXayRa { get; set; }

        // Thông tin khách hàng gây thiệt hại (lấy từ hợp đồng)
        [Display(Name = "Khách hàng gây thiệt hại")]
        public int? MaKhachHang { get; set; }

        [ForeignKey("MaKhachHang")]
        public User? KhachHang { get; set; }

        // Thông tin xử lý
        [Display(Name = "Trạng thái xử lý")]
        [StringLength(50)]
        public string TrangThaiXuLy { get; set; } = "Chưa xử lý"; // Chưa xử lý, Đang xử lý, Đã xử lý, Đã đền bù

        [Display(Name = "Phương án xử lý")]
        [StringLength(500)]
        public string? PhuongAnXuLy { get; set; }

        [Display(Name = "Số tiền khách đền bù")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Số tiền đền bù phải lớn hơn hoặc bằng 0")]
        public decimal SoTienDenBu { get; set; } = 0;

        [Display(Name = "Ngày hoàn thành xử lý")]
        [DataType(DataType.Date)]
        public DateTime? NgayHoanThanh { get; set; }

        // Thông tin bổ sung
        [Display(Name = "Ghi chú")]
        [StringLength(1000)]
        public string? GhiChu { get; set; }

        [Display(Name = "Người báo cáo")]
        public int? MaNguoiBaoCao { get; set; }

        [ForeignKey("MaNguoiBaoCao")]
        public User? NguoiBaoCao { get; set; }

        [Display(Name = "Ngày tạo báo cáo")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        [Display(Name = "Ngày cập nhật cuối")]
        public DateTime? NgayCapNhat { get; set; }

        // Computed properties
        [NotMapped]
        public decimal SoTienConLai => 0 - SoTienDenBu; // ChiPhiXuLy đã bị xóa, giờ là 0 - SoTienDenBu

        [NotMapped]
        public bool DaHoanThanh => TrangThaiXuLy == "Đã xử lý" || TrangThaiXuLy == "Đã đền bù";

        [NotMapped]
        public string TrangThaiClass
        {
            get
            {
                return TrangThaiXuLy switch
                {
                    "Chưa xử lý" => "danger",
                    "Đang xử lý" => "warning",
                    "Đã xử lý" => "success",
                    "Đã đền bù" => "info",
                    _ => "secondary"
                };
            }
        }

        [NotMapped]
        public string LoaiThietHaiClass
        {
            get
            {
                return LoaiThietHai switch
                {
                    "Mất xe" => "danger",
                    "Hư hỏng phụ kiện" => "warning",
                    "Hư hỏng thân xe" => "warning",
                    "Khác" => "secondary",
                    _ => "info"
                };
            }
        }
    }
} 