using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bike.Repository;
using bike.Models;
using bike.Attributes;
using bike.ViewModel;
using System.Security.Claims;

namespace bike.Controllers
{
    [CustomAuthorize("Admin")]
    public class QuanLyNhanVienController : Controller
    {
        private readonly BikeDbContext _context;

        public QuanLyNhanVienController(BikeDbContext context)
        {
            _context = context;
        }

        // GET: QuanLyNhanVien
        public async Task<IActionResult> Index(string searchString, string vaiTro, string trangThai, int page = 1, int pageSize = 10)
        {
            var query = _context.Users
                .Where(u => u.VaiTro == "Admin" || u.VaiTro == "Staff")
                .AsQueryable();

            // Tìm kiếm theo tên hoặc email
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(u => u.Ten.Contains(searchString) || 
                                        u.Email.Contains(searchString) ||
                                        u.SoDienThoai.Contains(searchString));
            }

            // Lọc theo vai trò
            if (!string.IsNullOrEmpty(vaiTro))
            {
                query = query.Where(u => u.VaiTro == vaiTro);
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(trangThai))
            {
                if (trangThai == "active")
                {
                    query = query.Where(u => u.IsActive && !u.NgayNghiViec.HasValue);
                }
                else if (trangThai == "inactive")
                {
                    query = query.Where(u => !u.IsActive || u.NgayNghiViec.HasValue);
                }
            }

            // Tính toán phân trang
            int totalItems = await query.CountAsync();
            var nhanViens = await query
                .OrderByDescending(u => u.NgayTao)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Thống kê
            var tongNhanVien = await _context.Users
                .Where(u => u.VaiTro == "Admin" || u.VaiTro == "Staff")
                .CountAsync();
            var dangLamViec = await _context.Users
                .Where(u => (u.VaiTro == "Admin" || u.VaiTro == "Staff") && u.IsActive && !u.NgayNghiViec.HasValue)
                .CountAsync();
            var nghiViec = await _context.Users
                .Where(u => (u.VaiTro == "Admin" || u.VaiTro == "Staff") && u.NgayNghiViec.HasValue)
                .CountAsync();

            ViewBag.SearchString = searchString;
            ViewBag.VaiTro = vaiTro;
            ViewBag.TrangThai = trangThai;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TongNhanVien = tongNhanVien;
            ViewBag.DangLamViec = dangLamViec;
            ViewBag.NghiViec = nghiViec;

            return View(nhanViens);
        }

        // GET: QuanLyNhanVien/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nhanVien = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && (u.VaiTro == "Admin" || u.VaiTro == "Staff"));

            if (nhanVien == null)
            {
                return NotFound();
            }

            return View(nhanVien);
        }

        // GET: QuanLyNhanVien/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: QuanLyNhanVien/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User nhanVien)
        {
            try
            {
                // Kiểm tra email trùng lặp
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == nhanVien.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng!");
                    return View(nhanVien);
                }

                // Đảm bảo vai trò hợp lệ
                if (nhanVien.VaiTro != "Admin" && nhanVien.VaiTro != "Staff")
                {
                    nhanVien.VaiTro = "Staff";
                }

                // Hash mật khẩu (sử dụng SHA256 - trong thực tế nên dùng BCrypt)
                nhanVien.MatKhau = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(nhanVien.MatKhau)).ToString();
                nhanVien.NgayTao = DateTime.Now;
                nhanVien.IsActive = true;

                // Nếu có ngày vào làm, set mặc định là hôm nay
                if (!nhanVien.NgayVaoLam.HasValue)
                {
                    nhanVien.NgayVaoLam = DateTime.Today;
                }

                // Đảm bảo mức lương được xử lý đúng cách
                if (nhanVien.MucLuong.HasValue && nhanVien.MucLuong.Value < 0)
                {
                    nhanVien.MucLuong = null;
                }

                _context.Users.Add(nhanVien);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Thêm nhân viên thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                return View(nhanVien);
            }
        }

        // GET: QuanLyNhanVien/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nhanVien = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && (u.VaiTro == "Admin" || u.VaiTro == "Staff"));

            if (nhanVien == null)
            {
                return NotFound();
            }

            // Map User model sang EditUserViewModel
            var editViewModel = new EditUserViewModel
            {
                Id = nhanVien.Id,
                Ten = nhanVien.Ten,
                Email = nhanVien.Email,
                VaiTro = nhanVien.VaiTro,
                SoDienThoai = nhanVien.SoDienThoai,
                DiaChi = nhanVien.DiaChi,
                IsActive = nhanVien.IsActive,
                NgayVaoLam = nhanVien.NgayVaoLam,
                NgayNghiViec = nhanVien.NgayNghiViec,
                MucLuong = nhanVien.MucLuong,
                NgayTao = nhanVien.NgayTao,
                MatKhau = null // Không set mật khẩu
            };

            return View(editViewModel);
        }

        // POST: QuanLyNhanVien/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditUserViewModel editModel)
        {
            if (id != editModel.Id)
            {
                return NotFound();
            }

            // Kiểm tra ModelState (EditUserViewModel không có [Required] cho MatKhau)
            if (!ModelState.IsValid)
            {
                return View(editModel);
            }

            try
            {
                var existingUser = await _context.Users.FindAsync(id);
                if (existingUser == null)
                {
                    return NotFound();
                }

                // Kiểm tra email trùng lặp (trừ chính user này)
                var duplicateEmail = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == editModel.Email && u.Id != id);
                if (duplicateEmail != null)
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng!");
                    return View(editModel);
                }

                // Cập nhật thông tin từ EditUserViewModel
                existingUser.Ten = editModel.Ten;
                existingUser.Email = editModel.Email;
                existingUser.SoDienThoai = editModel.SoDienThoai;
                existingUser.DiaChi = editModel.DiaChi;
                existingUser.VaiTro = editModel.VaiTro;
                existingUser.IsActive = editModel.IsActive;
                existingUser.NgayVaoLam = editModel.NgayVaoLam;
                existingUser.NgayNghiViec = editModel.NgayNghiViec;
                
                // Đảm bảo mức lương được xử lý đúng cách
                if (editModel.MucLuong.HasValue && editModel.MucLuong.Value < 0)
                {
                    existingUser.MucLuong = null;
                }
                else
                {
                    existingUser.MucLuong = editModel.MucLuong;
                }

                // Chỉ cập nhật mật khẩu nếu có nhập mới
                if (!string.IsNullOrEmpty(editModel.MatKhau))
                {
                    existingUser.MatKhau = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(editModel.MatKhau)).ToString();
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = "Cập nhật nhân viên thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                return View(editModel);
            }
        }

        // GET: QuanLyNhanVien/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nhanVien = await _context.Users     
                .FirstOrDefaultAsync(u => u.Id == id && (u.VaiTro == "Admin" || u.VaiTro == "Staff"));

            if (nhanVien == null)
            {
                return NotFound();
            }

            return View(nhanVien);
        }

        // POST: QuanLyNhanVien/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var nhanVien = await _context.Users.FindAsync(id);
                if (nhanVien != null && (nhanVien.VaiTro == "Admin" || nhanVien.VaiTro == "Staff"))
                {
                    // Thay vì xóa, ta set ngày nghỉ việc
                    nhanVien.NgayNghiViec = DateTime.Today;
                    nhanVien.IsActive = false;
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Đã đánh dấu nhân viên nghỉ việc!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: QuanLyNhanVien/KichHoat/5 - Kích hoạt lại nhân viên
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KichHoat(int id)
        {
            try
            {
                var nhanVien = await _context.Users.FindAsync(id);
                if (nhanVien != null && (nhanVien.VaiTro == "Admin" || nhanVien.VaiTro == "Staff"))
                {
                    nhanVien.NgayNghiViec = null;
                    nhanVien.IsActive = true;
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Đã kích hoạt lại nhân viên!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
} 