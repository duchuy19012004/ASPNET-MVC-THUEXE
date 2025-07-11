using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using bike.Attributes;

namespace bike.Models
{
    public class Xe
    {
        [Key]
        public int MaXe { get; set; }

        [Required(ErrorMessage = "Tên xe là bắt buộc")]
        [Display(Name = "Tên xe")]
        [StringLength(100)]
        public string TenXe { get; set; }

        [Required(ErrorMessage = "Biển số xe là bắt buộc")]
        [Display(Name = "Biển số")]
        [StringLength(20)]
        public string BienSoXe { get; set; }

        [Required(ErrorMessage = "Hãng xe là bắt buộc")]
        [Display(Name = "Hãng xe")]
        [StringLength(50)]
        public string HangXe { get; set; }

        [Required(ErrorMessage = "Dòng xe là bắt buộc")]
        [Display(Name = "Dòng xe")]
        [StringLength(50)]
        public string? DongXe { get; set; }

        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        [Display(Name = "Trạng thái")]
        [StringLength(20)]
        public string? TrangThai { get; set; } // Sẵn sàng, Đang thuê, Bảo trì, Hư hỏng, Mất

        [Display(Name = "Giá thuê/ngày")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá thuê phải lớn hơn 0")]
        public decimal GiaThue { get; set; }
        // Thông tin thiệt hại và đền bù
        [Display(Name = "Giá trị xe (đền bù)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal GiaTriXe { get; set; } = 0; // Giá trị để tính đền bù khi mất

        [Display(Name = "Ngày gặp sự cố")]
        [DataType(DataType.Date)]
        public DateTime? NgayGapSuCo { get; set; }

        [Display(Name = "Mô tả thiệt hại")]
        [StringLength(1000)]
        public string? MoTaThietHai { get; set; }

        [Display(Name = "Chi phí sửa chữa")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ChiPhiSuaChua { get; set; } = 0;

        // Thêm thuộc tính khóa ngoại
        [Display(Name = "Mã loại xe")]
        public int MaLoaiXe { get; set; }

        // Khai báo navigation property
        [ForeignKey("MaLoaiXe")]
        public LoaiXe? LoaiXe { get; set; }

        public virtual ICollection<ChiTieu> ChiTieu { get; set; } = new List<ChiTieu>();
        
        // Navigation property cho quan hệ n-n với HopDong thông qua ChiTietHopDong
        public virtual ICollection<ChiTietHopDong> ChiTietHopDong { get; set; } = new List<ChiTietHopDong>();
        
        // Navigation property cho nhiều hình ảnh
        public virtual ICollection<HinhAnhXe> HinhAnhXes { get; set; } = new List<HinhAnhXe>();

        // Helper property để lấy hình ảnh chính
        [NotMapped]
        public string? HinhAnhChinh => HinhAnhXes?.FirstOrDefault(h => h.LaAnhChinh)?.TenFile;
        
        // Helper property để lấy hình ảnh cho hiển thị (ưu tiên ảnh chính, fallback về ảnh đầu tiên)
        [NotMapped]
        public string? HinhAnhHienThi => HinhAnhChinh ?? HinhAnhXes?.OrderBy(h => h.ThuTu).FirstOrDefault()?.TenFile;
    }
}
