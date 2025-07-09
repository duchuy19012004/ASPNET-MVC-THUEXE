using System.ComponentModel.DataAnnotations;

namespace bike.Models
{
    public class Banner
    {
        [Key]
        public int MaBanner { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề banner")]
        [Display(Name = "Tiêu đề")]
        [StringLength(200)]
        public string TieuDe { get; set; } = string.Empty;

        [Display(Name = "Mô tả")]
        [StringLength(500)]
        public string? MoTa { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn hình ảnh")]
        [Display(Name = "Hình ảnh")]
        [StringLength(255)]
        public string HinhAnh { get; set; } = string.Empty;

        [Display(Name = "Link liên kết")]
        [StringLength(500)]
        public string? LinkLienKet { get; set; }

        [Display(Name = "Thứ tự hiển thị")]
        [Range(1, 3, ErrorMessage = "Thứ tự hiển thị từ 1 đến 3")]
        public int ThuTu { get; set; } = 1;

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true; // true: Hiển thị, false: Ẩn

        [Display(Name = "Ngày tạo")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        [Display(Name = "Ngày cập nhật")]
        public DateTime? NgayCapNhat { get; set; }
    }
} 