using bike.Models;
using bike.Repository;
using System.Security.Claims;

namespace bike.Services
{
    public interface IPermissionService
    {
        Task<bool> HasPermissionAsync(int userId, string permissionName);
        Task<bool> HasPermissionAsync(ClaimsPrincipal user, string permissionName);
        Task<UserPermission> GetUserPermissionsAsync(int userId);
    }

    public class PermissionService : IPermissionService
    {
        private readonly IUserRepository _userRepository;

        public PermissionService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> HasPermissionAsync(int userId, string permissionName)
        {
            var permission = await _userRepository.GetUserPermissionAsync(userId);
            if (permission == null) return false;

            return permissionName switch
            {
                "CanViewXe" => permission.CanViewXe,
                "CanCreateXe" => permission.CanCreateXe,
                "CanEditXe" => permission.CanEditXe,
                "CanDeleteXe" => permission.CanDeleteXe,
                "CanViewLoaiXe" => permission.CanViewLoaiXe,
                "CanCreateLoaiXe" => permission.CanCreateLoaiXe,
                "CanEditLoaiXe" => permission.CanEditLoaiXe,
                "CanDeleteLoaiXe" => permission.CanDeleteLoaiXe,
                "CanViewHopDong" => permission.CanViewHopDong,
                "CanCreateHopDong" => permission.CanCreateHopDong,
                "CanEditHopDong" => permission.CanEditHopDong,
                "CanDeleteHopDong" => permission.CanDeleteHopDong,
                "CanPrintHopDong" => permission.CanPrintHopDong,
                "CanViewHoaDon" => permission.CanViewHoaDon,
                "CanCreateHoaDon" => permission.CanCreateHoaDon,
                "CanEditHoaDon" => permission.CanEditHoaDon,
                "CanDeleteHoaDon" => permission.CanDeleteHoaDon,
                "CanPrintHoaDon" => permission.CanPrintHoaDon,
                "CanViewNhanVien" => permission.CanViewNhanVien,
                "CanCreateNhanVien" => permission.CanCreateNhanVien,
                "CanEditNhanVien" => permission.CanEditNhanVien,
                "CanDeleteNhanVien" => permission.CanDeleteNhanVien,
                "CanViewUser" => permission.CanViewUser,
                "CanCreateUser" => permission.CanCreateUser,
                "CanEditUser" => permission.CanEditUser,
                "CanDeleteUser" => permission.CanDeleteUser,
                "CanViewBanner" => permission.CanViewBanner,
                "CanCreateBanner" => permission.CanCreateBanner,
                "CanEditBanner" => permission.CanEditBanner,
                "CanDeleteBanner" => permission.CanDeleteBanner,
                "CanViewChiTieu" => permission.CanViewChiTieu,
                "CanCreateChiTieu" => permission.CanCreateChiTieu,
                "CanEditChiTieu" => permission.CanEditChiTieu,
                "CanDeleteChiTieu" => permission.CanDeleteChiTieu,
                "CanViewThietHai" => permission.CanViewThietHai,
                "CanCreateThietHai" => permission.CanCreateThietHai,
                "CanEditThietHai" => permission.CanEditThietHai,
                "CanDeleteThietHai" => permission.CanDeleteThietHai,
                "CanThanhToanThietHai" => permission.CanThanhToanThietHai,
                "CanViewBaoCao" => permission.CanViewBaoCao,
                "CanViewThongKe" => permission.CanViewThongKe,
                "CanExportBaoCao" => permission.CanExportBaoCao,
                "CanViewCart" => permission.CanViewCart,
                "CanCheckout" => permission.CanCheckout,
                "CanDatCho" => permission.CanDatCho,
                "CanViewDatCho" => permission.CanViewDatCho,
                "CanViewHinhAnhXe" => permission.CanViewHinhAnhXe,
                "CanUploadHinhAnhXe" => permission.CanUploadHinhAnhXe,
                "CanDeleteHinhAnhXe" => permission.CanDeleteHinhAnhXe,

                _ => false
            };
        }

        public async Task<bool> HasPermissionAsync(ClaimsPrincipal user, string permissionName)
        {
            if (!user.Identity.IsAuthenticated) return false;

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return false;

            return await HasPermissionAsync(userId, permissionName);
        }

        public async Task<UserPermission> GetUserPermissionsAsync(int userId)
        {
            return await _userRepository.GetUserPermissionAsync(userId);
        }
    }
} 