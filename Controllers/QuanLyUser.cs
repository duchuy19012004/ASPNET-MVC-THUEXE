using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bike.Models;
using bike.Repository;
using bike.Attributes;
using bike.ViewModel;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;

namespace bike.Controllers
{
    [CustomAuthorize("Admin")] // Chỉ Admin mới được quản lý user
    public class QuanLyUser : Controller
    {
        private readonly BikeDbContext _context;
        private readonly ILogger<QuanLyUser> _logger;

        public QuanLyUser(BikeDbContext context, ILogger<QuanLyUser> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: User
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.OrderBy(u => u.Ten).ToListAsync();
            return View(users);
        }

        // GET: User/Create
        public IActionResult Create()
        {
            ViewBag.Roles = new SelectList(new[]
            {
                new { Value = "Admin", Text = "Quản trị viên" },
                new { Value = "Staff", Text = "Nhân viên" },
                new { Value = "User", Text = "Khách hàng" }
            }, "Value", "Text");

            return View();
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "Email đã tồn tại!");
            }

            // Validate password confirmation
            if (!string.IsNullOrEmpty(user.MatKhau) && user.MatKhau != user.XacNhanMatKhau)
            {
                ModelState.AddModelError("XacNhanMatKhau", "Mật khẩu và xác nhận mật khẩu không khớp!");
            }

            if (ModelState.IsValid)
            {
                // Hash password
                user.MatKhau = HashPassword(user.MatKhau);

                _context.Add(user);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Thêm người dùng thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Roles = new SelectList(new[]
            {
                new { Value = "Admin", Text = "Quản trị viên" },
                new { Value = "Staff", Text = "Nhân viên" },
                new { Value = "User", Text = "Khách hàng" }
            }, "Value", "Text", user.VaiTro);

            return View(user);
        }

        // GET: User/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Map User to EditUserViewModel
            var editViewModel = new EditUserViewModel
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

            ViewBag.Roles = new SelectList(new[]
            {
                new { Value = "Admin", Text = "Quản trị viên" },
                new { Value = "Staff", Text = "Nhân viên" },
                new { Value = "User", Text = "Khách hàng" }
            }, "Value", "Text", editViewModel.VaiTro);

            // If AJAX request, return partial view
            bool isAjaxRequest = Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                                Request.Headers.Accept.ToString().Contains("application/json");
            
            if (isAjaxRequest)
            {
                ViewData["IsPartial"] = true;
                return PartialView("Edit", editViewModel);
            }

            return View(editViewModel);
        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditUserViewModel model, bool isModal = false)
        {
            if (id != model.Id)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Không tìm thấy người dùng!" });
                }
                return NotFound();
            }

            // Check email unique
            if (await _context.Users.AnyAsync(u => u.Email == model.Email && u.Id != id))
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng!");
            }

            // Validate password confirmation if password is provided
            if (!string.IsNullOrEmpty(model.MatKhau) && model.MatKhau != model.XacNhanMatKhau)
            {
                ModelState.AddModelError("XacNhanMatKhau", "Mật khẩu và xác nhận mật khẩu không khớp!");
            }

            // If AJAX request with validation errors, return JSON immediately
            if (!ModelState.IsValid && Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                return Json(new { success = false, message = "Dữ liệu không hợp lệ!", errors = errors });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _context.Users.FindAsync(id);
                    if (existingUser == null)
                    {
                        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        {
                            return Json(new { success = false, message = "Không tìm thấy người dùng!" });
                        }
                        return NotFound();
                    }

                    // Update fields
                    existingUser.Ten = model.Ten;
                    existingUser.Email = model.Email;
                    existingUser.SoDienThoai = model.SoDienThoai;
                    existingUser.DiaChi = model.DiaChi;
                    existingUser.VaiTro = model.VaiTro;
                    existingUser.IsActive = model.IsActive;

                    // Update password if provided
                    if (!string.IsNullOrEmpty(model.MatKhau))
                    {
                        existingUser.MatKhau = HashPassword(model.MatKhau);
                    }

                    await _context.SaveChangesAsync();
                    
                    // Always return JSON for AJAX requests (modal submissions)
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, message = "Cập nhật thông tin thành công!" });
                    }
                    
                    // Direct access to Edit page - redirect to Index with success message
                    TempData["Success"] = "Cập nhật thông tin thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!UserExists(model.Id))
                    {
                        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        {
                            return Json(new { success = false, message = "Người dùng không tồn tại!" });
                        }
                        return NotFound();
                    }
                    else
                    {
                        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        {
                            return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật!" });
                        }
                        throw;
                    }
                }
            }

            // This should not happen as we handle AJAX validation errors earlier
            // But keep for safety
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });
            }

            ViewBag.Roles = new SelectList(new[]
            {
                new { Value = "Admin", Text = "Quản trị viên" },
                new { Value = "Staff", Text = "Nhân viên" },
                new { Value = "User", Text = "Khách hàng" }
            }, "Value", "Text", model.VaiTro);

            return View(model);
        }

        // GET: User/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            // Không cho xóa chính mình
            if (user.Id == int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value))
            {
                TempData["Error"] = "Không thể xóa tài khoản của chính mình!";
                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            
            if (user == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Không tìm thấy người dùng!" });
                }
                TempData["Error"] = "Không tìm thấy người dùng!";
                return RedirectToAction(nameof(Index));
            }

            // Không cho xóa chính mình
            if (user.Id == int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value))
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Không thể xóa tài khoản của chính mình!" });
                }
                TempData["Error"] = "Không thể xóa tài khoản của chính mình!";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Xóa người dùng thành công!" });
                }
                
                TempData["Success"] = "Xóa người dùng thành công!";
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Có lỗi xảy ra khi xóa người dùng!" });
                }
                TempData["Error"] = "Có lỗi xảy ra khi xóa người dùng!";
            }

            return RedirectToAction(nameof(Index));
        }

        // Helper methods
        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}