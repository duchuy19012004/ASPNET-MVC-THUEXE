using bike.Models;
using bike.ViewModel;
using bike.Repository;
using UserModel = bike.Models.User;

namespace bike.Services.QuanLyUsers
{
    public interface IUserValidator
    {
        Task<ValidationResult> ValidateCreateUserAsync(UserModel user);
        Task<ValidationResult> ValidateUpdateUserAsync(EditUserViewModel model);
        Task<ValidationResult> ValidatePasswordAsync(string password, string confirmPassword);
        ValidationResult ValidateUserCanBeDeleted(UserModel user, int currentUserId);
    }

    public class UserValidator : IUserValidator
    {
        private readonly IUserRepository _userRepository;

        public UserValidator(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ValidationResult> ValidateCreateUserAsync(UserModel user)
        {
            var result = new ValidationResult();

            // Validate required fields
            if (string.IsNullOrWhiteSpace(user.Ten))
                result.AddError("Ten", "Tên là bắt buộc");

            if (string.IsNullOrWhiteSpace(user.Email))
                result.AddError("Email", "Email là bắt buộc");

            if (string.IsNullOrWhiteSpace(user.MatKhau))
                result.AddError("MatKhau", "Mật khẩu là bắt buộc");

            if (string.IsNullOrWhiteSpace(user.VaiTro))
                result.AddError("VaiTro", "Vai trò là bắt buộc");

            // Validate email format
            if (!string.IsNullOrWhiteSpace(user.Email) && !IsValidEmail(user.Email))
                result.AddError("Email", "Email không hợp lệ");

            // Validate email uniqueness
            if (!string.IsNullOrWhiteSpace(user.Email) && await _userRepository.EmailExistsAsync(user.Email))
                result.AddError("Email", "Email đã tồn tại!");

            // Validate password confirmation
            if (!string.IsNullOrWhiteSpace(user.MatKhau) && user.MatKhau != user.XacNhanMatKhau)
                result.AddError("XacNhanMatKhau", "Mật khẩu và xác nhận mật khẩu không khớp!");

            return result;
        }

        public async Task<ValidationResult> ValidateUpdateUserAsync(EditUserViewModel model)
        {
            var result = new ValidationResult();

            // Validate required fields
            if (string.IsNullOrWhiteSpace(model.Ten))
                result.AddError("Ten", "Tên là bắt buộc");

            if (string.IsNullOrWhiteSpace(model.Email))
                result.AddError("Email", "Email là bắt buộc");

            if (string.IsNullOrWhiteSpace(model.VaiTro))
                result.AddError("VaiTro", "Vai trò là bắt buộc");

            // Validate email format
            if (!string.IsNullOrWhiteSpace(model.Email) && !IsValidEmail(model.Email))
                result.AddError("Email", "Email không hợp lệ");

            // Validate email uniqueness
            if (!string.IsNullOrWhiteSpace(model.Email) && await _userRepository.EmailExistsAsync(model.Email, model.Id))
                result.AddError("Email", "Email đã được sử dụng!");

            // Validate password confirmation if password is provided
            if (!string.IsNullOrWhiteSpace(model.MatKhau) && model.MatKhau != model.XacNhanMatKhau)
                result.AddError("XacNhanMatKhau", "Mật khẩu và xác nhận mật khẩu không khớp!");

            return result;
        }

        public async Task<ValidationResult> ValidatePasswordAsync(string password, string confirmPassword)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(password))
                result.AddError("MatKhau", "Mật khẩu là bắt buộc");

            if (password != confirmPassword)
                result.AddError("XacNhanMatKhau", "Mật khẩu và xác nhận mật khẩu không khớp!");

            return result;
        }

        public ValidationResult ValidateUserCanBeDeleted(UserModel user, int currentUserId)
        {
            var result = new ValidationResult();

            if (user.Id == currentUserId)
                result.AddError("Delete", "Không thể xóa tài khoản của chính mình!");

            return result;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }

    public class ValidationResult
    {
        public bool IsValid => !Errors.Any();
        public Dictionary<string, List<string>> Errors { get; } = new();

        public void AddError(string field, string message)
        {
            if (!Errors.ContainsKey(field))
                Errors[field] = new List<string>();

            Errors[field].Add(message);
        }

        public IEnumerable<string> GetErrors(string field)
        {
            return Errors.ContainsKey(field) ? Errors[field] : Enumerable.Empty<string>();
        }

        public string GetFirstError(string field)
        {
            return GetErrors(field).FirstOrDefault() ?? string.Empty;
        }
    }
} 