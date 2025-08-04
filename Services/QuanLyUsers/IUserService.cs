using bike.Models;
using bike.ViewModel;
using bike.Repository;
using Microsoft.AspNetCore.Mvc.Rendering;
using UserModel = bike.Models.User;

namespace bike.Services.QuanLyUsers
{
    public interface IUserService
    {
        Task<IEnumerable<UserModel>> GetAllUsersAsync();
        Task<UserModel?> GetUserByIdAsync(int id);
        Task<EditUserViewModel?> GetUserForEditAsync(int id);
        Task<ServiceResult<UserModel>> CreateUserAsync(UserModel user);
        Task<ServiceResult<UserModel>> UpdateUserAsync(EditUserViewModel model);
        Task<ServiceResult<bool>> DeleteUserAsync(int id, int currentUserId);
        IEnumerable<SelectListItem> GetRoleOptions();
        
        // Quản lý quyền user
        Task<UserPermissionViewModel> GetUserPermissionsAsync(int userId);
        Task<bool> UpdateUserPermissionsAsync(UserPermissionViewModel model);
        Task<UserPermission> GetUserPermissionEntityAsync(int userId);
        Task<bool> CreateUserPermissionAsync(int userId);
        Task<bool> HasPermissionAsync(int userId, string permissionName);
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserValidator _userValidator;
        private readonly IPasswordService _passwordService;

        public UserService(IUserRepository userRepository, IUserValidator userValidator, IPasswordService passwordService)
        {
            _userRepository = userRepository;
            _userValidator = userValidator;
            _passwordService = passwordService;
        }

        public async Task<IEnumerable<UserModel>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllUsersAsync();
        }

        public async Task<UserModel?> GetUserByIdAsync(int id)
        {
            return await _userRepository.GetUserByIdAsync(id);
        }

        public async Task<EditUserViewModel?> GetUserForEditAsync(int id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null) return null;

            return new EditUserViewModel
            {
                Id = user.Id,
                Ten = user.Ten,
                Email = user.Email,
                VaiTro = user.VaiTro,
                SoDienThoai = user.SoDienThoai,
                DiaChi = user.DiaChi,
                IsActive = user.IsActive,
                NgayTao = user.NgayTao
            };
        }

        public async Task<ServiceResult<UserModel>> CreateUserAsync(UserModel user)
        {
            var result = new ServiceResult<UserModel>();

            // Validate user data
            var validationResult = await _userValidator.ValidateCreateUserAsync(user);
            if (!validationResult.IsValid)
            {
                result.Errors = validationResult.Errors;
                return result;
            }

            try
            {
                // Hash password
                user.MatKhau = _passwordService.HashPassword(user.MatKhau);
                user.NgayTao = DateTime.Now;

                // Create user
                var createdUser = await _userRepository.CreateUserAsync(user);
                result.Data = createdUser;
                result.IsSuccess = true;
                result.Message = "Thêm người dùng thành công!";
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = $"Có lỗi xảy ra: {ex.Message}";
            }

            return result;
        }

        public async Task<ServiceResult<UserModel>> UpdateUserAsync(EditUserViewModel model)
        {
            var result = new ServiceResult<UserModel>();

            // Validate model
            var validationResult = await _userValidator.ValidateUpdateUserAsync(model);
            if (!validationResult.IsValid)
            {
                result.Errors = validationResult.Errors;
                return result;
            }

            try
            {
                var existingUser = await _userRepository.GetUserByIdAsync(model.Id);
                if (existingUser == null)
                {
                    result.IsSuccess = false;
                    result.Message = "Không tìm thấy người dùng!";
                    return result;
                }

                // Update user properties
                existingUser.Ten = model.Ten;
                existingUser.Email = model.Email;
                existingUser.SoDienThoai = model.SoDienThoai;
                existingUser.DiaChi = model.DiaChi;
                existingUser.VaiTro = model.VaiTro;
                existingUser.IsActive = model.IsActive;

                // Update password if provided
                if (!string.IsNullOrEmpty(model.MatKhau))
                {
                    existingUser.MatKhau = _passwordService.HashPassword(model.MatKhau);
                }

                var updatedUser = await _userRepository.UpdateUserAsync(existingUser);
                result.Data = updatedUser;
                result.IsSuccess = true;
                result.Message = "Cập nhật thông tin thành công!";
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = $"Có lỗi xảy ra: {ex.Message}";
            }

            return result;
        }

        public async Task<ServiceResult<bool>> DeleteUserAsync(int id, int currentUserId)
        {
            var result = new ServiceResult<bool>();

            try
            {
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                {
                    result.IsSuccess = false;
                    result.Message = "Không tìm thấy người dùng!";
                    return result;
                }

                // Validate user can be deleted
                var validationResult = _userValidator.ValidateUserCanBeDeleted(user, currentUserId);
                if (!validationResult.IsValid)
                {
                    result.Errors = validationResult.Errors;
                    return result;
                }

                bool deleted = await _userRepository.DeleteUserAsync(id);
                if (deleted)
                {
                    result.Data = true;
                    result.IsSuccess = true;
                    result.Message = "Xóa người dùng thành công!";
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = "Không thể xóa người dùng!";
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = $"Có lỗi xảy ra: {ex.Message}";
            }

            return result;
        }

        public IEnumerable<SelectListItem> GetRoleOptions()
        {
            return new[]
            {
                new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = "Admin", Text = "Quản trị viên" },
                new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = "Staff", Text = "Nhân viên" },
                new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = "User", Text = "Khách hàng" }
            };
        }

        // Implement các method quản lý quyền
        public async Task<UserPermissionViewModel> GetUserPermissionsAsync(int userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null) return null;

            var permission = await GetUserPermissionEntityAsync(userId);
            if (permission == null)
            {
                await CreateUserPermissionAsync(userId);
                permission = await GetUserPermissionEntityAsync(userId);
            }

            return new UserPermissionViewModel
            {
                UserId = userId,
                UserName = user.Ten,
                UserEmail = user.Email,
                XePermission = GetPermissionLevel(permission.CanViewXe, permission.CanCreateXe, permission.CanEditXe, permission.CanDeleteXe),
                LoaiXePermission = GetPermissionLevel(permission.CanViewLoaiXe, permission.CanCreateLoaiXe, permission.CanEditLoaiXe, permission.CanDeleteLoaiXe),
                HopDongPermission = GetPermissionLevel(permission.CanViewHopDong, permission.CanCreateHopDong, permission.CanEditHopDong, permission.CanDeleteHopDong),
                HoaDonPermission = GetPermissionLevel(permission.CanViewHoaDon, permission.CanCreateHoaDon, permission.CanEditHoaDon, permission.CanDeleteHoaDon),
                NhanVienPermission = GetPermissionLevel(permission.CanViewNhanVien, permission.CanCreateNhanVien, permission.CanEditNhanVien, permission.CanDeleteNhanVien),
                UserPermission = GetPermissionLevel(permission.CanViewUser, permission.CanCreateUser, permission.CanEditUser, permission.CanDeleteUser),
                BannerPermission = GetPermissionLevel(permission.CanViewBanner, permission.CanCreateBanner, permission.CanEditBanner, permission.CanDeleteBanner),
                ChiTieuPermission = GetPermissionLevel(permission.CanViewChiTieu, permission.CanCreateChiTieu, permission.CanEditChiTieu, permission.CanDeleteChiTieu),
                ThietHaiPermission = GetPermissionLevel(permission.CanViewThietHai, permission.CanCreateThietHai, permission.CanEditThietHai, permission.CanDeleteThietHai),
                BaoCaoPermission = GetBaoCaoPermissionLevel(permission.CanViewBaoCao, permission.CanViewThongKe, permission.CanExportBaoCao),
                HinhAnhXePermission = GetPermissionLevel(permission.CanViewHinhAnhXe, permission.CanUploadHinhAnhXe, permission.CanEditHinhAnhXe, permission.CanDeleteHinhAnhXe),

            };
        }

        public async Task<bool> UpdateUserPermissionsAsync(UserPermissionViewModel model)
        {
            try
            {
                var permission = await GetUserPermissionEntityAsync(model.UserId);
                if (permission == null)
                {
                    await CreateUserPermissionAsync(model.UserId);
                    permission = await GetUserPermissionEntityAsync(model.UserId);
                }

                // Cập nhật quyền xe
                UpdatePermissionFromLevel(permission, model.XePermission, 
                    "CanViewXe", "CanCreateXe", "CanEditXe", "CanDeleteXe");

                // Cập nhật quyền loại xe
                UpdatePermissionFromLevel(permission, model.LoaiXePermission,
                    "CanViewLoaiXe", "CanCreateLoaiXe", "CanEditLoaiXe", "CanDeleteLoaiXe");

                // Cập nhật quyền hợp đồng
                UpdatePermissionFromLevel(permission, model.HopDongPermission,
                    "CanViewHopDong", "CanCreateHopDong", "CanEditHopDong", "CanDeleteHopDong");

                // Cập nhật quyền hóa đơn
                UpdatePermissionFromLevel(permission, model.HoaDonPermission,
                    "CanViewHoaDon", "CanCreateHoaDon", "CanEditHoaDon", "CanDeleteHoaDon");

                // Cập nhật quyền nhân viên
                UpdatePermissionFromLevel(permission, model.NhanVienPermission,
                    "CanViewNhanVien", "CanCreateNhanVien", "CanEditNhanVien", "CanDeleteNhanVien");

                // Cập nhật quyền user
                UpdatePermissionFromLevel(permission, model.UserPermission,
                    "CanViewUser", "CanCreateUser", "CanEditUser", "CanDeleteUser");

                // Cập nhật quyền banner
                UpdatePermissionFromLevel(permission, model.BannerPermission,
                    "CanViewBanner", "CanCreateBanner", "CanEditBanner", "CanDeleteBanner");

                // Cập nhật quyền chi tiêu
                UpdatePermissionFromLevel(permission, model.ChiTieuPermission,
                    "CanViewChiTieu", "CanCreateChiTieu", "CanEditChiTieu", "CanDeleteChiTieu");

                // Cập nhật quyền thiệt hại
                UpdatePermissionFromLevel(permission, model.ThietHaiPermission,
                    "CanViewThietHai", "CanCreateThietHai", "CanEditThietHai", "CanDeleteThietHai");

                // Cập nhật quyền báo cáo - xử lý riêng vì có cấu trúc khác
                UpdateBaoCaoPermission(permission, model.BaoCaoPermission);

                // Cập nhật quyền hình ảnh xe
                UpdatePermissionFromLevel(permission, model.HinhAnhXePermission,
                    "CanViewHinhAnhXe", "CanUploadHinhAnhXe", "CanEditHinhAnhXe", "CanDeleteHinhAnhXe");



                await _userRepository.UpdateUserPermissionAsync(permission);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<UserPermission> GetUserPermissionEntityAsync(int userId)
        {
            return await _userRepository.GetUserPermissionAsync(userId);
        }

        public async Task<bool> CreateUserPermissionAsync(int userId)
        {
            try
            {
                var permission = new UserPermission
                {
                    UserId = userId,
                    CanViewXe = true,
                    CanViewLoaiXe = true,
                    CanViewHopDong = true,
                    CanViewHoaDon = true,
                    CanViewBanner = true,
                    CanViewThietHai = true,
                    CanViewCart = true,
                    CanCheckout = true,
                    CanDatCho = true,
                    CanViewDatCho = true,
                    CanViewHinhAnhXe = true,

                };

                await _userRepository.CreateUserPermissionAsync(permission);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> HasPermissionAsync(int userId, string permissionName)
        {
            var permission = await GetUserPermissionEntityAsync(userId);
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
                "CanEditHinhAnhXe" => permission.CanEditHinhAnhXe,
                "CanDeleteHinhAnhXe" => permission.CanDeleteHinhAnhXe,
                _ => false
            };
        }

        private string GetPermissionLevel(bool canView, bool canCreate, bool canEdit, bool canDelete)
        {
            if (canView && canCreate && canEdit && canDelete) return "All";
            if (canView && canDelete) return "Delete";
            if (canView && canEdit) return "Edit";
            if (canView && canCreate) return "Create";
            if (canView) return "View";
            return "None";
        }

        private void UpdatePermissionFromLevel(UserPermission permission, string level, 
            string viewProp, string createProp, string editProp, string deleteProp)
        {
            switch (level)
            {
                case "None":
                    SetProperty(permission, viewProp, false);
                    SetProperty(permission, createProp, false);
                    SetProperty(permission, editProp, false);
                    SetProperty(permission, deleteProp, false);
                    break;
                case "View":
                    SetProperty(permission, viewProp, true);
                    SetProperty(permission, createProp, false);
                    SetProperty(permission, editProp, false);
                    SetProperty(permission, deleteProp, false);
                    break;
                case "Create":
                    SetProperty(permission, viewProp, true);
                    SetProperty(permission, createProp, true);
                    SetProperty(permission, editProp, false);
                    SetProperty(permission, deleteProp, false);
                    break;
                case "Edit":
                    SetProperty(permission, viewProp, true);
                    SetProperty(permission, createProp, false);
                    SetProperty(permission, editProp, true);
                    SetProperty(permission, deleteProp, false);
                    break;
                case "Delete":
                    SetProperty(permission, viewProp, true);
                    SetProperty(permission, createProp, false);
                    SetProperty(permission, editProp, false);
                    SetProperty(permission, deleteProp, true);
                    break;
                case "All":
                    SetProperty(permission, viewProp, true);
                    SetProperty(permission, createProp, true);
                    SetProperty(permission, editProp, true);
                    SetProperty(permission, deleteProp, true);
                    break;
            }
        }

        private void SetProperty(UserPermission permission, string propertyName, bool value)
        {
            var property = typeof(UserPermission).GetProperty(propertyName);
            if (property != null)
            {
                property.SetValue(permission, value);
            }
        }

        private void UpdateBaoCaoPermission(UserPermission permission, string level)
        {
            switch (level)
            {
                case "None":
                    permission.CanViewBaoCao = false;
                    permission.CanViewThongKe = false;
                    permission.CanExportBaoCao = false;
                    break;
                case "View":
                    permission.CanViewBaoCao = true;
                    permission.CanViewThongKe = false;
                    permission.CanExportBaoCao = false;
                    break;
                case "Create":
                    permission.CanViewBaoCao = true;
                    permission.CanViewThongKe = true;
                    permission.CanExportBaoCao = false;
                    break;
                case "Edit":
                    permission.CanViewBaoCao = true;
                    permission.CanViewThongKe = true;
                    permission.CanExportBaoCao = false;
                    break;
                case "Delete":
                    permission.CanViewBaoCao = true;
                    permission.CanViewThongKe = false;
                    permission.CanExportBaoCao = true;
                    break;
                case "All":
                    permission.CanViewBaoCao = true;
                    permission.CanViewThongKe = true;
                    permission.CanExportBaoCao = true;
                    break;
            }
        }

        private string GetBaoCaoPermissionLevel(bool canView, bool canViewThongKe, bool canExport)
        {
            if (canView && canViewThongKe && canExport) return "All";
            if (canView && canExport) return "Delete";
            if (canView && canViewThongKe) return "Create";
            if (canView) return "View";
            return "None";
        }
    }

    public class ServiceResult<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public Dictionary<string, List<string>> Errors { get; set; } = new();

        public bool HasErrors => Errors.Any();
    }
} 