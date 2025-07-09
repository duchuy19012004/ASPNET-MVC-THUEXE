using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bike.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên")]
        [Display(Name = "Họ và tên")]
        [StringLength(100)]
        public string? Ten { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100)]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [Display(Name = "Mật khẩu")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [DataType(DataType.Password)]
        public string? MatKhau { get; set; }

        [Display(Name = "Vai trò")]
        [StringLength(20)]
        public string VaiTro { get; set; } = "User"; // User, Staff, Admin
        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(20)]
        public string? SoDienThoai { get; set; }

        [Display(Name = "Ảnh đại diện")]
        [StringLength(255)]
        public string? Avatar { get; set; }

        [Display(Name = "Địa chỉ")]
        [StringLength(255)]
        public string? DiaChi { get; set; }

        [Display(Name = "Trạng thái")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Ngày tạo")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Thông tin nhân viên
        [Display(Name = "Ngày vào làm")]
        [DataType(DataType.Date)]
        public DateTime? NgayVaoLam { get; set; }

        [Display(Name = "Ngày nghỉ việc")]
        [DataType(DataType.Date)]
        public DateTime? NgayNghiViec { get; set; }

        [Display(Name = "Mức lương")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MucLuong { get; set; }

        // Navigation properties
        public virtual ICollection<HopDong> HopDongKhachHang { get; set; } = new List<HopDong>();
        public virtual ICollection<HopDong> HopDongNguoiTao { get; set; } = new List<HopDong>();
    }
}