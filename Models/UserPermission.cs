using System.ComponentModel.DataAnnotations;

namespace bike.Models
{
    public class UserPermission
    {
        [Key]
        public int Id { get; set; }
        
        public int UserId { get; set; }
        public virtual User User { get; set; }
        
        // Quyền quản lý xe
        public bool CanManageXe { get; set; } = false;
        public bool CanViewXe { get; set; } = true;
        public bool CanCreateXe { get; set; } = false;
        public bool CanEditXe { get; set; } = false;
        public bool CanDeleteXe { get; set; } = false;
        
        // Quyền quản lý loại xe
        public bool CanManageLoaiXe { get; set; } = false;
        public bool CanViewLoaiXe { get; set; } = true;
        public bool CanCreateLoaiXe { get; set; } = false;
        public bool CanEditLoaiXe { get; set; } = false;
        public bool CanDeleteLoaiXe { get; set; } = false;
        
        // Quyền quản lý hợp đồng
        public bool CanManageHopDong { get; set; } = false;
        public bool CanViewHopDong { get; set; } = true;
        public bool CanCreateHopDong { get; set; } = false;
        public bool CanEditHopDong { get; set; } = false;
        public bool CanDeleteHopDong { get; set; } = false;
        public bool CanPrintHopDong { get; set; } = false;
        
        // Quyền quản lý hóa đơn
        public bool CanManageHoaDon { get; set; } = false;
        public bool CanViewHoaDon { get; set; } = true;
        public bool CanCreateHoaDon { get; set; } = false;
        public bool CanEditHoaDon { get; set; } = false;
        public bool CanDeleteHoaDon { get; set; } = false;
        public bool CanPrintHoaDon { get; set; } = false;
        
        // Quyền quản lý nhân viên
        public bool CanManageNhanVien { get; set; } = false;
        public bool CanViewNhanVien { get; set; } = false;
        public bool CanCreateNhanVien { get; set; } = false;
        public bool CanEditNhanVien { get; set; } = false;
        public bool CanDeleteNhanVien { get; set; } = false;
        
        // Quyền quản lý user
        public bool CanManageUser { get; set; } = false;
        public bool CanViewUser { get; set; } = false;
        public bool CanCreateUser { get; set; } = false;
        public bool CanEditUser { get; set; } = false;
        public bool CanDeleteUser { get; set; } = false;
        
        // Quyền quản lý banner
        public bool CanManageBanner { get; set; } = false;
        public bool CanViewBanner { get; set; } = true;
        public bool CanCreateBanner { get; set; } = false;
        public bool CanEditBanner { get; set; } = false;
        public bool CanDeleteBanner { get; set; } = false;
        
        // Quyền quản lý chi tiêu
        public bool CanManageChiTieu { get; set; } = false;
        public bool CanViewChiTieu { get; set; } = false;
        public bool CanCreateChiTieu { get; set; } = false;
        public bool CanEditChiTieu { get; set; } = false;
        public bool CanDeleteChiTieu { get; set; } = false;
        
        // Quyền quản lý thiệt hại
        public bool CanManageThietHai { get; set; } = false;
        public bool CanViewThietHai { get; set; } = true;
        public bool CanCreateThietHai { get; set; } = false;
        public bool CanEditThietHai { get; set; } = false;
        public bool CanDeleteThietHai { get; set; } = false;
        public bool CanThanhToanThietHai { get; set; } = false;
        
        // Quyền báo cáo thống kê
        public bool CanViewBaoCao { get; set; } = false;
        public bool CanViewThongKe { get; set; } = false;
        public bool CanExportBaoCao { get; set; } = false;
        
        // Quyền quản lý giỏ hàng
        public bool CanManageCart { get; set; } = false;
        public bool CanViewCart { get; set; } = true;
        public bool CanCheckout { get; set; } = true;
        
        // Quyền đặt chỗ
        public bool CanDatCho { get; set; } = true;
        public bool CanViewDatCho { get; set; } = true;
        
        // Quyền quản lý hình ảnh xe
        public bool CanManageHinhAnhXe { get; set; } = false;
        public bool CanViewHinhAnhXe { get; set; } = true;
        public bool CanUploadHinhAnhXe { get; set; } = false;
        public bool CanEditHinhAnhXe { get; set; } = false;
        public bool CanDeleteHinhAnhXe { get; set; } = false;
        

    }
} 