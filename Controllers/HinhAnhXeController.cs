using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using bike.Models;
using bike.Repository;
using bike.Attributes;

namespace bike.Controllers
{
    [CustomAuthorize("Admin", "Staff")]
    public class HinhAnhXeController : Controller
    {
        private readonly BikeDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HinhAnhXeController(BikeDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: HinhAnhXe
        public async Task<IActionResult> Index(int? maXe)
        {
            var query = _context.HinhAnhXe.Include(h => h.Xe).AsQueryable();

            if (maXe.HasValue)
            {
                query = query.Where(h => h.MaXe == maXe.Value);
            }

            var hinhAnhXes = await query.OrderBy(h => h.MaXe).ThenBy(h => h.ThuTu).ToListAsync();

            // Lấy danh sách xe để filter
            ViewBag.XeList = new SelectList(await _context.Xe.ToListAsync(), "MaXe", "TenXe");
            ViewBag.MaXeSelected = maXe;

            return View(hinhAnhXes);
        }

        // GET: HinhAnhXe/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hinhAnhXe = await _context.HinhAnhXe
                .Include(h => h.Xe)
                .FirstOrDefaultAsync(m => m.MaHinhAnh == id);
            if (hinhAnhXe == null)
            {
                return NotFound();
            }

            return View(hinhAnhXe);
        }

        // GET: HinhAnhXe/Create
        public IActionResult Create(int? maXe)
        {
            var model = new HinhAnhXe();
            if (maXe.HasValue)
            {
                model.MaXe = maXe.Value;
            }

            ViewBag.MaXe = new SelectList(_context.Xe, "MaXe", "TenXe", maXe);
            return View(model);
        }

        // POST: HinhAnhXe/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaXe,MoTa,ThuTu,LaAnhChinh")] HinhAnhXe hinhAnhXe, IFormFile HinhAnh)
        {
            ModelState.Remove("TenFile");

            if (ModelState.IsValid)
            {
                // Xử lý upload file
                if (HinhAnh != null && HinhAnh.Length > 0)
                {
                    var fileName = await SaveImageAsync(HinhAnh);
                    if (fileName != null)
                    {
                        hinhAnhXe.TenFile = fileName;
                        hinhAnhXe.NgayThem = DateTime.Now;

                        // Nếu đây là ảnh chính, bỏ ảnh chính cũ
                        if (hinhAnhXe.LaAnhChinh)
                        {
                            var oldMainImages = await _context.HinhAnhXe
                                .Where(h => h.MaXe == hinhAnhXe.MaXe && h.LaAnhChinh)
                                .ToListAsync();

                            foreach (var oldImage in oldMainImages)
                            {
                                oldImage.LaAnhChinh = false;
                            }
                        }

                        _context.Add(hinhAnhXe);
                        await _context.SaveChangesAsync();
                        TempData["Success"] = "Thêm hình ảnh thành công!";
                        return RedirectToAction(nameof(Index), new { maXe = hinhAnhXe.MaXe });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Có lỗi khi tải lên hình ảnh");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Vui lòng chọn hình ảnh");
                }
            }

            ViewBag.MaXe = new SelectList(_context.Xe, "MaXe", "TenXe", hinhAnhXe.MaXe);
            return View(hinhAnhXe);
        }

        // GET: HinhAnhXe/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hinhAnhXe = await _context.HinhAnhXe.FindAsync(id);
            if (hinhAnhXe == null)
            {
                return NotFound();
            }
            ViewBag.MaXe = new SelectList(_context.Xe, "MaXe", "TenXe", hinhAnhXe.MaXe);
            return View(hinhAnhXe);
        }

        // POST: HinhAnhXe/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaHinhAnh,MaXe,TenFile,MoTa,ThuTu,LaAnhChinh,NgayThem")] HinhAnhXe hinhAnhXe, IFormFile? HinhAnh)
        {
            if (id != hinhAnhXe.MaHinhAnh)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Xử lý upload file mới nếu có
                    if (HinhAnh != null && HinhAnh.Length > 0)
                    {
                        // Xóa file cũ
                        if (!string.IsNullOrEmpty(hinhAnhXe.TenFile))
                        {
                            DeleteImageFile(hinhAnhXe.TenFile);
                        }

                        var fileName = await SaveImageAsync(HinhAnh);
                        if (fileName != null)
                        {
                            hinhAnhXe.TenFile = fileName;
                        }
                    }

                    // Nếu đây là ảnh chính, bỏ ảnh chính cũ
                    if (hinhAnhXe.LaAnhChinh)
                    {
                        var oldMainImages = await _context.HinhAnhXe
                            .Where(h => h.MaXe == hinhAnhXe.MaXe && h.LaAnhChinh && h.MaHinhAnh != hinhAnhXe.MaHinhAnh)
                            .ToListAsync();

                        foreach (var oldImage in oldMainImages)
                        {
                            oldImage.LaAnhChinh = false;
                        }
                    }

                    _context.Update(hinhAnhXe);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật hình ảnh thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HinhAnhXeExists(hinhAnhXe.MaHinhAnh))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { maXe = hinhAnhXe.MaXe });
            }
            ViewBag.MaXe = new SelectList(_context.Xe, "MaXe", "TenXe", hinhAnhXe.MaXe);
            return View(hinhAnhXe);
        }

        // GET: HinhAnhXe/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hinhAnhXe = await _context.HinhAnhXe
                .Include(h => h.Xe)
                .FirstOrDefaultAsync(m => m.MaHinhAnh == id);
            if (hinhAnhXe == null)
            {
                return NotFound();
            }

            return View(hinhAnhXe);
        }

        // POST: HinhAnhXe/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hinhAnhXe = await _context.HinhAnhXe.FindAsync(id);
            if (hinhAnhXe != null)
            {
                // Xóa file hình ảnh
                if (!string.IsNullOrEmpty(hinhAnhXe.TenFile))
                {
                    DeleteImageFile(hinhAnhXe.TenFile);
                }

                _context.HinhAnhXe.Remove(hinhAnhXe);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa hình ảnh thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        // Helper methods
        private async Task<string?> SaveImageAsync(IFormFile file)
        {
            try
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "xe");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                return fileName;
            }
            catch
            {
                return null;
            }
        }

        private void DeleteImageFile(string fileName)
        {
            try
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "xe", fileName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            catch
            {
                // Ignore errors when deleting files
            }
        }

        private bool HinhAnhXeExists(int id)
        {
            return _context.HinhAnhXe.Any(e => e.MaHinhAnh == id);
        }

        // AJAX: Set main image
        [HttpPost]
        public async Task<IActionResult> SetMainImage(int id)
        {
            try
            {
                var hinhAnhXe = await _context.HinhAnhXe.FindAsync(id);
                if (hinhAnhXe == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy hình ảnh" });
                }

                // Bỏ ảnh chính cũ
                var oldMainImages = await _context.HinhAnhXe
                    .Where(h => h.MaXe == hinhAnhXe.MaXe && h.LaAnhChinh)
                    .ToListAsync();

                foreach (var oldImage in oldMainImages)
                {
                    oldImage.LaAnhChinh = false;
                }

                // Set ảnh mới làm ảnh chính
                hinhAnhXe.LaAnhChinh = true;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã đặt làm ảnh chính" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }
    }
} 