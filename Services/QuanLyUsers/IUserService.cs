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
                new SelectListItem { Value = "Admin", Text = "Quản trị viên" },
                new SelectListItem { Value = "Staff", Text = "Nhân viên" },
                new SelectListItem { Value = "User", Text = "Khách hàng" }
            };
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