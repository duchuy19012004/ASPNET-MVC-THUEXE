using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bike.Models
{
    public class HinhAnhXe
    {
        [Key]
        public int MaHinhAnh { get; set; }

        [Required]
        [Display(Name = "Mã xe")]
        public int MaXe { get; set; }

        [Required(ErrorMessage = "Tên file hình ảnh là bắt buộc")]
        [Display(Name = "Tên file")]
        [StringLength(255)]
        public string TenFile { get; set; }

        [Display(Name = "Mô tả")]
        [StringLength(200)]
        public string? MoTa { get; set; }

        [Display(Name = "Thứ tự hiển thị")]
        public int ThuTu { get; set; } = 1;

        [Display(Name = "Là ảnh chính")]
        public bool LaAnhChinh { get; set; } = false;

        [Display(Name = "Ngày thêm")]
        [DataType(DataType.DateTime)]
        public DateTime NgayThem { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey("MaXe")]
        public virtual Xe? Xe { get; set; }
    }
} 