using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bike.Models;
using bike.Repository;
using bike.Attributes;

namespace bike.Controllers
{
    [CustomAuthorize("Admin", "Staff")] 
    public class QuanLyBannerController : Controller
    {
        private readonly BikeDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public QuanLyBannerController(BikeDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: QuanLyBanner
        public async Task<IActionResult> Index()
        {
            var banners = await _context.Banner
                .OrderBy(b => b.ThuTu)
                .ThenByDescending(b => b.NgayTao)
                .ToListAsync();
            return View(banners);
        }

        // GET: QuanLyBanner/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: QuanLyBanner/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Banner banner, IFormFile imageFile)
        {
            try
            {
                // Validation cơ bản
                if (string.IsNullOrWhiteSpace(banner.TieuDe))
                {
                    ModelState.AddModelError("TieuDe", "Vui lòng nhập tiêu đề banner!");
                }

                if (imageFile == null || imageFile.Length == 0)
                {
                    ModelState.AddModelError("", "Vui lòng chọn hình ảnh cho banner!");
                    return View(banner);
                }

                // Kiểm tra định dạng file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(imageFile.FileName).ToLower();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("", "Chỉ chấp nhận file hình ảnh (.jpg, .jpeg, .png, .gif, .webp)!");
                    return View(banner);
                }

                // Kiểm tra kích thước file (max 5MB)
                if (imageFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("", "Kích thước file không được vượt quá 5MB!");
                    return View(banner);
                }

                // Kiểm tra số lượng banner hiển thị (tối đa 3)
                if (banner.TrangThai)
                {
                    var activeBannersCount = await _context.Banner.CountAsync(b => b.TrangThai);
                    if (activeBannersCount >= 3)
                    {
                        ModelState.AddModelError("TrangThai", "Chỉ được hiển thị tối đa 3 banner cùng lúc!");
                        return View(banner);
                    }

                    // Kiểm tra thứ tự trùng lặp
                    var existingBanner = await _context.Banner
                        .FirstOrDefaultAsync(b => b.ThuTu == banner.ThuTu && b.TrangThai);
                    if (existingBanner != null)
                    {
                        ModelState.AddModelError("ThuTu", $"Thứ tự {banner.ThuTu} đã được sử dụng!");
                        return View(banner);
                    }
                }

                // Upload file
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "banner");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                // Lưu vào database
                banner.HinhAnh = uniqueFileName;
                banner.NgayTao = DateTime.Now;
                banner.NgayCapNhat = DateTime.Now;

                _context.Banner.Add(banner);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Thêm banner thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                return View(banner);
            }
        }

        // GET: QuanLyBanner/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var banner = await _context.Banner.FindAsync(id);
            if (banner == null)
            {
                return NotFound();
            }
            return View(banner);
        }

        // POST: QuanLyBanner/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Banner banner, IFormFile? imageFile)
        {
            if (id != banner.MaBanner)
            {
                return NotFound();
            }

            try
            {
                var originalBanner = await _context.Banner.AsNoTracking().FirstOrDefaultAsync(b => b.MaBanner == id);
                if (originalBanner == null)
                {
                    return NotFound();
                }

                // Xử lý upload hình ảnh mới (nếu có)
                if (imageFile != null && imageFile.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var fileExtension = Path.GetExtension(imageFile.FileName).ToLower();
                    
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("", "Chỉ chấp nhận file hình ảnh (.jpg, .jpeg, .png, .gif, .webp)!");
                        return View(banner);
                    }

                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "banner");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Xóa file cũ
                    if (!string.IsNullOrEmpty(originalBanner.HinhAnh))
                    {
                        string oldFilePath = Path.Combine(uploadsFolder, originalBanner.HinhAnh);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Upload file mới
                    string uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    banner.HinhAnh = uniqueFileName;
                }
                else
                {
                    banner.HinhAnh = originalBanner.HinhAnh;
                }

                // Kiểm tra validation
                if (banner.TrangThai)
                {
                    var activeBannersCount = await _context.Banner
                        .CountAsync(b => b.TrangThai && b.MaBanner != id);
                    if (activeBannersCount >= 3)
                    {
                        ModelState.AddModelError("TrangThai", "Chỉ được hiển thị tối đa 3 banner cùng lúc!");
                        return View(banner);
                    }

                    var existingBanner = await _context.Banner
                        .FirstOrDefaultAsync(b => b.ThuTu == banner.ThuTu && b.TrangThai && b.MaBanner != id);
                    if (existingBanner != null)
                    {
                        ModelState.AddModelError("ThuTu", $"Thứ tự {banner.ThuTu} đã được sử dụng!");
                        return View(banner);
                    }
                }

                banner.NgayCapNhat = DateTime.Now;
                banner.NgayTao = originalBanner.NgayTao;
                
                _context.Update(banner);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Cập nhật banner thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BannerExists(banner.MaBanner))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // GET: QuanLyBanner/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var banner = await _context.Banner
                .FirstOrDefaultAsync(m => m.MaBanner == id);
            if (banner == null)
            {
                return NotFound();
            }

            return View(banner);
        }

        // POST: QuanLyBanner/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var banner = await _context.Banner.FindAsync(id);
            if (banner != null)
            {
                // Xóa file hình ảnh
                if (!string.IsNullOrEmpty(banner.HinhAnh))
                {
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "banner");
                    string filePath = Path.Combine(uploadsFolder, banner.HinhAnh);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Banner.Remove(banner);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa banner thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool BannerExists(int id)
        {
            return _context.Banner.Any(e => e.MaBanner == id);
        }
    }
} 