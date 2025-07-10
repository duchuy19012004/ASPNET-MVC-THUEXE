using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bike.Models;
using bike.ViewModel;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using bike.Repository;

namespace bike.Controllers
{
    public class AccountController : Controller
    {
        private readonly BikeDbContext _context;

        public AccountController(BikeDbContext context)
        {
            _context = context;
        }

        // GET: Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra email đã tồn tại chưa
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email này đã được đăng ký!");
                    return View(model);
                }

                // Tạo user mới
                var user = new User
                {
                    Ten = model.Ten,
                    Email = model.Email,
                    MatKhau = HashPassword(model.MatKhau),
                    SoDienThoai = model.SoDienThoai,
                    VaiTro = "User" // Mặc định là User, có thể thay đổi nếu cần
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                TempData["ThongBao"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        // GET: Account/Login
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                // Tìm user theo email
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

                if (user != null && VerifyPassword(model.MatKhau, user.MatKhau))
                {
                    // Tạo claims
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.Ten),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, user.VaiTro)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                    };

                    // Đăng nhập
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    // Chuyển hướng theo role
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    // Redirect theo role mặc định
                    switch (user.VaiTro)
                    {
                        case "Admin":
                            return RedirectToAction("Index", "BaoCao");
                        case "Staff":
                            return RedirectToAction("Index", "QuanLyHopDong");
                        default:
                            return RedirectToAction("Index", "Home");
                    }
                }

                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng!");
            }

            return View(model);
        }
        // GET: Account/Logout - Không cần token
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // POST: Account/LogoutConfirm - Cần token (đổi tên để tránh trùng)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Logout")] // Vẫn map về route /Account/Logout
        public async Task<IActionResult> LogoutPost()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Helper methods
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            string hashOfInput = HashPassword(password);
            return hashOfInput == hashedPassword;
        }

        // GET: Account/Profile
        public async Task<IActionResult> Profile()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user == null)
            {
                return NotFound();
            }

            var profileModel = new ProfileViewModel
            {
                Id = user.Id,
                Ten = user.Ten,
                Email = user.Email,
                SoDienThoai = user.SoDienThoai,
                DiaChi = user.DiaChi,
                Avatar = user.Avatar,
                NgayTao = user.NgayTao
            };

            return View(profileModel);
        }

        // POST: Account/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user == null)
            {
                return NotFound();
            }

            // Kiểm tra email đã tồn tại chưa (ngoại trừ email của user hiện tại)
            if (model.Email != user.Email)
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng!");
                }
            }

            if (ModelState.IsValid)
            {
                user.Ten = model.Ten;
                user.Email = model.Email;
                user.SoDienThoai = model.SoDienThoai;
                user.DiaChi = model.DiaChi;

                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();

                    // Cập nhật lại claims nếu tên hoặc email thay đổi
                    if (user.Ten != User.FindFirst(ClaimTypes.Name)?.Value || 
                        user.Email != User.FindFirst(ClaimTypes.Email)?.Value)
                    {
                        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                            new Claim(ClaimTypes.Name, user.Ten),
                            new Claim(ClaimTypes.Email, user.Email),
                            new Claim(ClaimTypes.Role, user.VaiTro)
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity));
                    }

                    TempData["Success"] = "Cập nhật thông tin thành công!";
                    return RedirectToAction("Profile");
                }
                catch (Exception)
                {
                    TempData["Error"] = "Có lỗi xảy ra khi cập nhật thông tin!";
                }
            }

            return View(model);
        }

        // GET: Account/ChangePassword
        public IActionResult ChangePassword()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        // POST: Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

                if (user == null)
                {
                    return NotFound();
                }

                // Kiểm tra mật khẩu hiện tại
                if (!VerifyPassword(model.MatKhauHienTai, user.MatKhau))
                {
                    ModelState.AddModelError("MatKhauHienTai", "Mật khẩu hiện tại không đúng!");
                    return View(model);
                }

                // Cập nhật mật khẩu mới
                user.MatKhau = HashPassword(model.MatKhauMoi);

                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Đổi mật khẩu thành công!";
                    return RedirectToAction("Profile");
                }
                catch (Exception)
                {
                    TempData["Error"] = "Có lỗi xảy ra khi đổi mật khẩu!";
                }
            }

            return View(model);
        }

        // Debug action để check role - chỉ dùng tạm thời
        public IActionResult CheckRole()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { authenticated = false });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            return Json(new 
            {
                authenticated = true,
                userId = userId,
                userName = userName,
                userEmail = userEmail,
                userRole = userRole,
                allClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
            });
        }

        // Debug action để xem tất cả users trong database
        public async Task<IActionResult> CheckUsers()
        {
            var users = await _context.Users
                .Select(u => new { u.Id, u.Ten, u.Email, u.VaiTro, u.IsActive })
                .ToListAsync();

            return Json(users);
        }

        // Debug action để sửa role user
        public async Task<IActionResult> FixUserRole(int userId, string newRole)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "User không tồn tại" });
            }

            var oldRole = user.VaiTro;
            user.VaiTro = newRole;
            await _context.SaveChangesAsync();

            return Json(new 
            { 
                success = true, 
                message = $"Đã cập nhật role của {user.Email} từ '{oldRole}' thành '{newRole}'" 
            });
        }
    }
}
