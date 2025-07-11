using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bike.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using bike.Repository;
using Microsoft.AspNetCore.Authorization;
using bike.Attributes;

namespace bike.Controllers
{
    [CustomAuthorize("Admin", "Staff")]
    public class XeController : Controller
    {
        private readonly BikeDbContext _context;

        // Constructor
        public XeController(BikeDbContext context)
        {
            _context = context;
        }

        // Helper method để kiểm tra xe có đang được thuê không
        private async Task<bool> IsCarRented(int maXe)
        {
            return await _context.HopDong
                .AnyAsync(h => h.ChiTietHopDong.Any(ct => ct.MaXe == maXe && ct.TrangThaiXe == "Đang thuê"));
        }

        // Helper method để lưu hình ảnh
        private async Task<string> SaveImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return string.Empty;

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/xe", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }

        // Helper method để xóa hình ảnh
        private void DeleteImage(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return;

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/xe", fileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        // GET: Xe - Hiển thị danh sách xe
        public async Task<IActionResult> Index(string searchString, int? loaiXe, string hangXe, string trangThai)
        {
            // Query cơ bản
            var xeQuery = _context.Xe
                      .Include(x => x.LoaiXe)
                      .Include(x => x.ChiTieu)
                      .Include(x => x.HinhAnhXes)
                      .AsQueryable();

            // Tìm kiếm theo tên xe
            if (!string.IsNullOrEmpty(searchString))
            {
                xeQuery = xeQuery.Where(x => x.TenXe.Contains(searchString));
            }

            // Lọc theo loại xe
            if (loaiXe.HasValue)
            {
                xeQuery = xeQuery.Where(x => x.MaLoaiXe == loaiXe.Value);
            }

            // Lọc theo hãng xe
            if (!string.IsNullOrEmpty(hangXe))
            {
                xeQuery = xeQuery.Where(x => x.HangXe == hangXe);
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(trangThai))
            {
                xeQuery = xeQuery.Where(x => x.TrangThai == trangThai);
            }

            // Thống kê
            ViewBag.TongSoXe = await _context.Xe.CountAsync();
            ViewBag.XeSanSang = await _context.Xe.CountAsync(x => x.TrangThai == "Sẵn sàng");
            ViewBag.DangChoThue = await _context.Xe.CountAsync(x => x.TrangThai == "Đang thuê");
            ViewBag.BaoTri = await _context.Xe.CountAsync(x => x.TrangThai == "Bảo trì");
            ViewBag.HuHong = await _context.Xe.CountAsync(x => x.TrangThai == "Hư hỏng");
            ViewBag.Mat = await _context.Xe.CountAsync(x => x.TrangThai == "Mất");

            // Dropdown lists
            ViewBag.LoaiXeList = new SelectList(await _context.LoaiXe.ToListAsync(), "MaLoaiXe", "TenLoaiXe");
            ViewBag.HangXeList = new SelectList(await _context.Xe.Select(x => x.HangXe).Distinct().ToListAsync());
            ViewBag.TrangThaiList = new SelectList(new[] { "Sẵn sàng", "Đang thuê", "Bảo trì", "Hư hỏng", "Mất" });

            return View(await xeQuery.ToListAsync());
        }

        // GET: Xe/Create - Form thêm xe mới
        public async Task<IActionResult> Create()
        {
            ViewBag.MaLoaiXe = new SelectList(await _context.LoaiXe.ToListAsync(), "MaLoaiXe", "TenLoaiXe");
            ViewBag.TrangThaiList = new SelectList(new[] { "Sẵn sàng" });
            return View();
        }

        // POST: Xe/Create - Xử lý thêm xe mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Xe xe, IFormFile? hinhAnh, List<IFormFile>? hinhAnhKhac)
        {
            // Kiểm tra biển số đã tồn tại chưa
            if (await _context.Xe.AnyAsync(x => x.BienSoXe == xe.BienSoXe))
            {
                ModelState.AddModelError("BienSoXe", "Biển số xe này đã tồn tại trong hệ thống!");
            }

            if (ModelState.IsValid)
            {
                // Lưu xe trước để có MaXe
                _context.Add(xe);
                await _context.SaveChangesAsync();

                // Xử lý upload tất cả hình ảnh
                var hinhAnhXeList = new List<HinhAnhXe>();
                var thuTu = 1;

                // Thêm hình ảnh chính nếu có
                if (hinhAnh != null && hinhAnh.Length > 0)
                {
                    var fileName = await SaveImageAsync(hinhAnh);
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        hinhAnhXeList.Add(new HinhAnhXe
                        {
                            MaXe = xe.MaXe,
                            TenFile = fileName,
                            MoTa = "Hình ảnh chính",
                            ThuTu = thuTu++,
                            LaAnhChinh = true,
                            NgayThem = DateTime.Now
                        });
                    }
                }

                // Thêm các hình ảnh khác
                if (hinhAnhKhac != null && hinhAnhKhac.Count > 0)
                {
                    foreach (var file in hinhAnhKhac)
                    {
                        if (file != null && file.Length > 0)
                        {
                            var fileName = await SaveImageAsync(file);
                            if (!string.IsNullOrEmpty(fileName))
                            {
                                hinhAnhXeList.Add(new HinhAnhXe
                                {
                                    MaXe = xe.MaXe,
                                    TenFile = fileName,
                                    MoTa = $"Hình ảnh {thuTu}",
                                    ThuTu = thuTu++,
                                    LaAnhChinh = false,
                                    NgayThem = DateTime.Now
                                });
                            }
                        }
                    }
                }

                if (hinhAnhXeList.Count > 0)
                {
                    _context.HinhAnhXe.AddRange(hinhAnhXeList);
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Thêm xe mới thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.MaLoaiXe = new SelectList(await _context.LoaiXe.ToListAsync(), "MaLoaiXe", "TenLoaiXe", xe.MaLoaiXe);
            ViewBag.TrangThaiList = new SelectList(new[] { "Sẵn sàng", "Đang thuê", "Bảo trì" }, xe.TrangThai);
            return View(xe);
        }

        // GET: Xe/Edit/5 - Form sửa xe
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var xe = await _context.Xe
                .Include(x => x.HinhAnhXes.OrderBy(h => h.ThuTu))
                .FirstOrDefaultAsync(x => x.MaXe == id);
            
            if (xe == null)
            {
                return NotFound();
            }

            // Kiểm tra xe có đang được thuê không
            if (await IsCarRented(xe.MaXe))
            {
                TempData["Error"] = "Không thể chỉnh sửa xe đang cho thuê!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.MaLoaiXe = new SelectList(await _context.LoaiXe.ToListAsync(), "MaLoaiXe", "TenLoaiXe", xe.MaLoaiXe);
            ViewBag.TrangThaiList = new SelectList(new[] { "Sẵn sàng", "Đang thuê", "Bảo trì" }, xe.TrangThai);
            return View(xe);
        }

        // POST: Xe/Edit/5 - Xử lý cập nhật xe
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Xe xe, IFormFile? hinhAnh, List<IFormFile>? hinhAnhKhac)
        {
            if (id != xe.MaXe)
            {
                return NotFound();
            }

            // Kiểm tra xe có đang được thuê không
            if (await IsCarRented(xe.MaXe))
            {
                TempData["Error"] = "Không thể chỉnh sửa xe đang cho thuê!";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra biển số đã tồn tại chưa (ngoại trừ xe hiện tại)
            if (await _context.Xe.AnyAsync(x => x.BienSoXe == xe.BienSoXe && x.MaXe != id))
            {
                ModelState.AddModelError("BienSoXe", "Biển số xe này đã được sử dụng cho xe khác!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Cập nhật thông tin xe (không bao gồm hình ảnh)
                    _context.Update(xe);
                    await _context.SaveChangesAsync();

                    // Xử lý upload hình ảnh chính mới
                    if (hinhAnh != null && hinhAnh.Length > 0)
                    {
                        var fileName = await SaveImageAsync(hinhAnh);
                        if (!string.IsNullOrEmpty(fileName))
                        {
                            // Tìm và cập nhật ảnh chính hiện tại
                            var currentMainImage = await _context.HinhAnhXe
                                .FirstOrDefaultAsync(h => h.MaXe == xe.MaXe && h.LaAnhChinh);
                            
                            if (currentMainImage != null)
                            {
                                // Xóa ảnh cũ
                                DeleteImage(currentMainImage.TenFile);
                                currentMainImage.TenFile = fileName;
                                currentMainImage.NgayThem = DateTime.Now;
                            }
                            else
                            {
                                // Tạo ảnh chính mới
                                var newMainImage = new HinhAnhXe
                                {
                                    MaXe = xe.MaXe,
                                    TenFile = fileName,
                                    MoTa = "Hình ảnh chính",
                                    ThuTu = 1,
                                    LaAnhChinh = true,
                                    NgayThem = DateTime.Now
                                };
                                _context.HinhAnhXe.Add(newMainImage);
                            }
                            
                            await _context.SaveChangesAsync();
                        }
                    }

                    // Xử lý upload các hình ảnh khác
                    if (hinhAnhKhac != null && hinhAnhKhac.Count > 0)
                    {
                        var hinhAnhXeList = new List<HinhAnhXe>();
                        var maxThuTu = await _context.HinhAnhXe.Where(h => h.MaXe == xe.MaXe).MaxAsync(h => (int?)h.ThuTu) ?? 0;
                        var thuTu = maxThuTu + 1;

                        foreach (var file in hinhAnhKhac)
                        {
                            if (file != null && file.Length > 0)
                            {
                                var fileName = await SaveImageAsync(file);
                                if (!string.IsNullOrEmpty(fileName))
                                {
                                    hinhAnhXeList.Add(new HinhAnhXe
                                    {
                                        MaXe = xe.MaXe,
                                        TenFile = fileName,
                                        MoTa = $"Hình ảnh {thuTu}",
                                        ThuTu = thuTu++,
                                        LaAnhChinh = false,
                                        NgayThem = DateTime.Now
                                    });
                                }
                            }
                        }

                        if (hinhAnhXeList.Count > 0)
                        {
                            _context.HinhAnhXe.AddRange(hinhAnhXeList);
                            await _context.SaveChangesAsync();
                        }
                    }

                    TempData["Success"] = "Cập nhật thông tin xe thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!XeExists(xe.MaXe))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            // Reload hình ảnh nếu có lỗi
            var xeWithImages = await _context.Xe
                .Include(x => x.HinhAnhXes.OrderBy(h => h.ThuTu))
                .FirstOrDefaultAsync(x => x.MaXe == id);
            xe.HinhAnhXes = xeWithImages?.HinhAnhXes;

            ViewBag.MaLoaiXe = new SelectList(await _context.LoaiXe.ToListAsync(), "MaLoaiXe", "TenLoaiXe", xe.MaLoaiXe);
            ViewBag.TrangThaiList = new SelectList(new[] { "Sẵn sàng", "Đang thuê", "Bảo trì" }, xe.TrangThai);
            return View(xe);
        }

        // GET: Xe/Delete/5 - Xác nhận xóa
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var xe = await _context.Xe
                .Include(x => x.LoaiXe)
                .Include(x => x.HinhAnhXes)
                .FirstOrDefaultAsync(m => m.MaXe == id);
            if (xe == null)
            {
                return NotFound();
            }

            // Kiểm tra xe có đang được thuê không
            if (await IsCarRented(xe.MaXe))
            {
                TempData["Error"] = "Không thể xóa xe đang cho thuê!";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra xe có lịch sử hợp đồng không
            var hasContracts = await _context.HopDong.AnyAsync(h => h.ChiTietHopDong.Any(ct => ct.MaXe == xe.MaXe));
            ViewBag.HasContracts = hasContracts;

            return View(xe);
        }

        // POST: Xe/Delete/5 - Xử lý xóa
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var xe = await _context.Xe.FindAsync(id);
            if (xe == null)
            {
                return NotFound();
            }

            // Kiểm tra xe có đang được thuê không
            if (await IsCarRented(xe.MaXe))
            {
                TempData["Error"] = "Không thể xóa xe đang cho thuê!";
                return RedirectToAction(nameof(Index));
            }

            // Xóa tất cả hình ảnh của xe
            var hinhAnhXes = await _context.HinhAnhXe.Where(h => h.MaXe == xe.MaXe).ToListAsync();
            foreach (var hinhAnh in hinhAnhXes)
            {
                DeleteImage(hinhAnh.TenFile);
            }
            
            // Xóa records hình ảnh
            if (hinhAnhXes.Any())
            {
                _context.HinhAnhXe.RemoveRange(hinhAnhXes);
            }

            _context.Xe.Remove(xe);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Xóa xe thành công!";

            return RedirectToAction(nameof(Index));
        }

        // GET: Xe/Details/5 - Xem chi tiết xe
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var xe = await _context.Xe
                .Include(x => x.LoaiXe)
                .Include(x => x.HinhAnhXes)
                .FirstOrDefaultAsync(m => m.MaXe == id);

            if (xe == null)
            {
                return NotFound();
            }

            // Lấy lịch sử hợp đồng của xe
            ViewBag.LichSuHopDong = await _context.HopDong
                .Include(h => h.ChiTietHopDong)
                .ThenInclude(ct => ct.Xe)
                .Where(h => h.ChiTietHopDong.Any(ct => ct.MaXe == xe.MaXe))
                .OrderByDescending(h => h.NgayTao)
                .Take(5)
                .ToListAsync();

            return View(xe);
        }

        // Kiểm tra xe có tồn tại không
        private bool XeExists(int id)
        {
            return _context.Xe.Any(e => e.MaXe == id);
        }

        // AJAX: Kiểm tra biển số có tồn tại không
        [HttpGet]
        public async Task<IActionResult> KiemTraBienSo(string bienSoXe, int? maXe)
        {
            if (string.IsNullOrEmpty(bienSoXe))
            {
                return Json(true);
            }

            // Nếu đang edit (có maXe), loại trừ xe hiện tại
            bool exists;
            if (maXe.HasValue)
            {
                exists = await _context.Xe.AnyAsync(x => x.BienSoXe == bienSoXe && x.MaXe != maXe.Value);
            }
            else
            {
                exists = await _context.Xe.AnyAsync(x => x.BienSoXe == bienSoXe);
            }

            if (exists)
            {
                return Json("Biển số xe này đã tồn tại!");
            }

            return Json(true);
        }

        // API: Xóa hình ảnh
        [HttpPost]
        public async Task<IActionResult> DeleteImage(int id)
        {
            try
            {
                var hinhAnh = await _context.HinhAnhXe.FindAsync(id);
                if (hinhAnh == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy hình ảnh" });
                }

                // Không cho phép xóa hình ảnh chính
                if (hinhAnh.LaAnhChinh)
                {
                    return Json(new { success = false, message = "Không thể xóa hình ảnh chính" });
                }

                // Xóa file
                DeleteImage(hinhAnh.TenFile);

                // Xóa record
                _context.HinhAnhXe.Remove(hinhAnh);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Xóa hình ảnh thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // API: Đặt hình ảnh chính
        [HttpPost]
        public async Task<IActionResult> SetMainImage(int id)
        {
            try
            {
                var hinhAnh = await _context.HinhAnhXe.FindAsync(id);
                if (hinhAnh == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy hình ảnh" });
                }

                // Bỏ đặt tất cả hình ảnh khác của xe này làm hình chính
                var allImages = await _context.HinhAnhXe
                    .Where(h => h.MaXe == hinhAnh.MaXe)
                    .ToListAsync();

                foreach (var img in allImages)
                {
                    img.LaAnhChinh = (img.MaHinhAnh == id);
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đặt hình ảnh chính thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // API: Cập nhật mô tả hình ảnh
        [HttpPost]
        public async Task<IActionResult> UpdateImageDescription(int id, string description)
        {
            try
            {
                var hinhAnh = await _context.HinhAnhXe.FindAsync(id);
                if (hinhAnh == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy hình ảnh" });
                }

                hinhAnh.MoTa = description;
                _context.Update(hinhAnh);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Cập nhật mô tả thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }
    }
}