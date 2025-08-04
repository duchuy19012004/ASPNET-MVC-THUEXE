using System.ComponentModel.DataAnnotations;

namespace bike.ViewModel
{
    public class UserPermissionViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        
        // Quyền quản lý xe
        [Display(Name = "Quản lý xe")]
        public string XePermission { get; set; } = "View"; // None, View, Create, Edit, Delete, All
        
        // Quyền quản lý loại xe
        [Display(Name = "Quản lý loại xe")]
        public string LoaiXePermission { get; set; } = "View";
        
        // Quyền quản lý hợp đồng
        [Display(Name = "Quản lý hợp đồng")]
        public string HopDongPermission { get; set; } = "View";
        
        // Quyền quản lý hóa đơn
        [Display(Name = "Quản lý hóa đơn")]
        public string HoaDonPermission { get; set; } = "View";
        
        // Quyền quản lý nhân viên
        [Display(Name = "Quản lý nhân viên")]
        public string NhanVienPermission { get; set; } = "None";
        
        // Quyền quản lý user
        [Display(Name = "Quản lý người dùng")]
        public string UserPermission { get; set; } = "None";
        
        // Quyền quản lý banner
        [Display(Name = "Quản lý banner")]
        public string BannerPermission { get; set; } = "View";
        
        // Quyền quản lý chi tiêu
        [Display(Name = "Quản lý chi tiêu")]
        public string ChiTieuPermission { get; set; } = "None";
        
        // Quyền quản lý thiệt hại
        [Display(Name = "Quản lý thiệt hại")]
        public string ThietHaiPermission { get; set; } = "View";
        
        // Quyền báo cáo thống kê
        [Display(Name = "Báo cáo thống kê")]
        public string BaoCaoPermission { get; set; } = "None";
        
        // Quyền quản lý hình ảnh xe
        [Display(Name = "Quản lý hình ảnh xe")]
        public string HinhAnhXePermission { get; set; } = "View";
        

        
        // Các tùy chọn quyền
        public static List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> PermissionOptions => new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
        {
            new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = "None", Text = "Không có quyền" },
            new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = "View", Text = "Chỉ xem" },
            new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = "Create", Text = "Xem + Thêm mới" },
            new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = "Edit", Text = "Xem + Chỉnh sửa" },
            new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = "Delete", Text = "Xem + Xóa" },
            new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = "All", Text = "Toàn quyền" }
        };
    }
} 