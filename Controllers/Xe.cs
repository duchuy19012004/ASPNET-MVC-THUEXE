using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bike.Models;
using bike.Attributes;
using bike.Repository;
using System.IO;

namespace bike.Controllers
{
    public class XeController : Controller
    {
        private readonly BikeDbContext _context;

        public XeController(BikeDbContext context)
        {
            _context = context;
        }

        // GET: Xe
        [PermissionAuthorize("CanViewXe")]
        public async Task<IActionResult> Index(string searchString, int? loaiXe, string hangXe, bool? showDeleted = false)
        {
            // Lấy danh sách xe với filtering
            var xeList = await GetFilteredXeList(searchString, loaiXe, hangXe, showDeleted);

            // Set ViewBag cho thống kê (chỉ tính xe chưa xóa)
            ViewBag.TongSoXe = await _context.Xe.CountAsync(x => x.TrangThai != "Đã xóa");
            ViewBag.XeSanSang = await _context.Xe.CountAsync(x => x.TrangThai == "Sẵn sàng");
            ViewBag.DangChoThue = await _context.Xe.CountAsync(x => x.TrangThai == "Đang thuê");
            ViewBag.BaoTri = await _context.Xe.CountAsync(x => x.TrangThai == "Bảo trì");
            ViewBag.ShowDeleted = showDeleted;

            // Set ViewBag cho dropdown filters với giá trị được chọn
            ViewBag.LoaiXeList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.LoaiXe, "MaLoaiXe", "TenLoaiXe", loaiXe);
            
            // Tạo danh sách hãng xe từ dữ liệu hiện có
            var hangXeList = await _context.Xe
                .Where(x => !string.IsNullOrEmpty(x.HangXe))
                .Select(x => x.HangXe)
                .Distinct()
                .ToListAsync();
            ViewBag.HangXeList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(hangXeList, hangXe);

            // Lưu giá trị filter vào ViewBag để JavaScript có thể khôi phục
            ViewBag.CurrentSearchString = searchString;
            ViewBag.CurrentLoaiXe = loaiXe;
            ViewBag.CurrentHangXe = hangXe;

            return View(xeList);
        }

        // Helper method để lọc xe
        private async Task<List<Xe>> GetFilteredXeList(string searchString, int? loaiXe, string hangXe, bool? showDeleted = false)
        {
            var query = _context.Xe
                .Include(x => x.LoaiXe)
                .Include(x => x.ChiTieu)
                .Include(x => x.HinhAnhXes)
                .AsQueryable();

            // Lọc theo trạng thái xe
            if (showDeleted.HasValue && showDeleted.Value)
            {
                // Chỉ hiển thị xe đã xóa
                query = query.Where(x => x.TrangThai == "Đã xóa");
            }
            else
            {
                // Hiển thị xe hoạt động (không bao gồm xe đã xóa)
                query = query.Where(x => x.TrangThai != "Đã xóa");
            }

            // Debug: Log trước khi filter
            System.Diagnostics.Debug.WriteLine($"Initial query count: {await query.CountAsync()}");

            // Tìm kiếm theo tên xe hoặc biển số
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(x => x.TenXe.Contains(searchString) || x.BienSoXe.Contains(searchString));
                System.Diagnostics.Debug.WriteLine($"After search filter: {await query.CountAsync()}");
            }

            // Lọc theo loại xe
            if (loaiXe.HasValue)
            {
                query = query.Where(x => x.MaLoaiXe == loaiXe.Value);
                System.Diagnostics.Debug.WriteLine($"After loaiXe filter: {await query.CountAsync()}");
            }

            // Lọc theo hãng xe
            if (!string.IsNullOrEmpty(hangXe))
            {
                query = query.Where(x => x.HangXe == hangXe);
                System.Diagnostics.Debug.WriteLine($"After hangXe filter: {await query.CountAsync()}");
            }

            var result = await query.ToListAsync();
            System.Diagnostics.Debug.WriteLine($"Final result count: {result.Count}");
            
            return result;
        }

        // GET: Xe/Create
        [PermissionAuthorize("CanCreateXe")]
        public IActionResult Create()
        {
            ViewBag.MaLoaiXe = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.LoaiXe, "MaLoaiXe", "TenLoaiXe");
            ViewBag.TrangThaiList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(new[] { "Sẵn sàng", "Đang thuê", "Bảo trì", "Hư hỏng", "Mất" }, "Sẵn sàng");
            return View();
        }

        // POST: Xe/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("CanCreateXe")]
        public async Task<IActionResult> Create([Bind("BienSoXe,TenXe,HangXe,DongXe,MaLoaiXe,GiaThue,TrangThai")] Xe xe, IFormFile hinhAnh, List<IFormFile> hinhAnhKhac)
        {
            if (ModelState.IsValid)
            {
                // Lưu xe trước
                _context.Add(xe);
                await _context.SaveChangesAsync();

                // Xử lý hình ảnh chính
                if (hinhAnh != null && hinhAnh.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(hinhAnh.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "xe", fileName);
                    
                    // Tạo thư mục nếu chưa tồn tại
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await hinhAnh.CopyToAsync(stream);
                    }

                    // Lưu thông tin hình ảnh chính vào database
                    var hinhAnhChinh = new HinhAnhXe
                    {
                        MaXe = xe.MaXe,
                        TenFile = fileName,
                        LaAnhChinh = true,
                        ThuTu = 1
                    };
                    _context.HinhAnhXe.Add(hinhAnhChinh);
                }

                // Xử lý các hình ảnh khác
                if (hinhAnhKhac != null && hinhAnhKhac.Count > 0)
                {
                    int thuTu = 2; // Bắt đầu từ 2 vì ảnh chính là 1
                    foreach (var file in hinhAnhKhac)
                    {
                        if (file.Length > 0)
                        {
                            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "xe", fileName);
                            
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            var hinhAnhKhacEntity = new HinhAnhXe
                            {
                                MaXe = xe.MaXe,
                                TenFile = fileName,
                                LaAnhChinh = false,
                                ThuTu = thuTu++
                            };
                            _context.HinhAnhXe.Add(hinhAnhKhacEntity);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.MaLoaiXe = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.LoaiXe, "MaLoaiXe", "TenLoaiXe", xe.MaLoaiXe);
            ViewBag.TrangThaiList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(new[] { "Sẵn sàng", "Đang thuê", "Bảo trì", "Hư hỏng", "Mất" }, xe.TrangThai);
            return View(xe);
        }

        // GET: Xe/Edit/5
        [PermissionAuthorize("CanEditXe")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var xe = await _context.Xe
                .Include(x => x.LoaiXe)
                .FirstOrDefaultAsync(x => x.MaXe == id);
            if (xe == null)
            {
                return NotFound();
            }
            
            ViewBag.MaLoaiXe = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.LoaiXe, "MaLoaiXe", "TenLoaiXe", xe.MaLoaiXe);
            ViewBag.TrangThaiList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(new[] { "Sẵn sàng", "Đang thuê", "Bảo trì", "Hư hỏng", "Mất" }, xe.TrangThai);
            
            return View(xe);
        }

        // POST: Xe/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("CanEditXe")]
        public async Task<IActionResult> Edit(int id, [Bind("MaXe,BienSoXe,TenXe,HangXe,DongXe,MaLoaiXe,GiaThue,TrangThai")] Xe xe)
        {
            if (id != xe.MaXe)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(xe);
                    await _context.SaveChangesAsync();
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
            ViewBag.MaLoaiXe = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.LoaiXe, "MaLoaiXe", "TenLoaiXe", xe.MaLoaiXe);
            ViewBag.TrangThaiList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(new[] { "Sẵn sàng", "Đang thuê", "Bảo trì", "Hư hỏng", "Mất" }, xe.TrangThai);
            return View(xe);
        }

        // GET: Xe/Details/5
        [PermissionAuthorize("CanViewXe")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var xe = await _context.Xe
                .Include(x => x.LoaiXe)
                .Include(x => x.ChiTieu)
                .Include(x => x.HinhAnhXes)
                .Include(x => x.ChiTietHopDong)
                    .ThenInclude(ct => ct.HopDong)
                .FirstOrDefaultAsync(x => x.MaXe == id);

            if (xe == null)
            {
                return NotFound();
            }

            return View(xe);
        }

        // GET: Xe/DetailsModal/5
        [PermissionAuthorize("CanViewXe")]
        public async Task<IActionResult> DetailsModal(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var xe = await _context.Xe
                .Include(x => x.LoaiXe)
                .Include(x => x.ChiTieu)
                .Include(x => x.HinhAnhXes)
                .Include(x => x.ChiTietHopDong)
                    .ThenInclude(ct => ct.HopDong)
                .FirstOrDefaultAsync(x => x.MaXe == id);

            if (xe == null)
            {
                return NotFound();
            }

            return PartialView("_CustomDetailsModal", xe);
        }

        // GET: Xe/Delete/5
        [PermissionAuthorize("CanDeleteXe")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var xe = await _context.Xe
                .Include(x => x.LoaiXe)
                .FirstOrDefaultAsync(m => m.MaXe == id);
            if (xe == null)
            {
                return NotFound();
            }

            // Kiểm tra xem xe có lịch sử hợp đồng không
            var hasContracts = await _context.ChiTietHopDong
                .AnyAsync(ct => ct.MaXe == id);
            ViewBag.HasContracts = hasContracts;

            return View(xe);
        }

        // POST: Xe/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("CanDeleteXe")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var xe = await _context.Xe.FindAsync(id);
            if (xe != null)
            {
                // Soft delete - chỉ thay đổi trạng thái thành "Đã xóa"
                xe.TrangThai = "Đã xóa";
                _context.Xe.Update(xe);
                
                TempData["Success"] = $"Đã xóa xe {xe.TenXe} (Biển số: {xe.BienSoXe}). Dữ liệu thống kê vẫn được giữ lại.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool XeExists(int id)
        {
            return _context.Xe.Any(e => e.MaXe == id);
        }

        // POST: Xe/Restore/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("CanDeleteXe")]
        public async Task<IActionResult> Restore(int id)
        {
            var xe = await _context.Xe.FindAsync(id);
            if (xe != null && xe.TrangThai == "Đã xóa")
            {
                xe.TrangThai = "Sẵn sàng"; // Khôi phục về trạng thái sẵn sàng
                _context.Xe.Update(xe);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"Đã khôi phục xe {xe.TenXe} (Biển số: {xe.BienSoXe}).";
            }

            return RedirectToAction(nameof(Index), new { showDeleted = true });
        }

        // GET: Xe/LichSuHopDong/5
        [PermissionAuthorize("CanViewXe")]
        public async Task<IActionResult> LichSuHopDong(int? id, string searchString, string timeFilter, DateTime? startDate, DateTime? endDate)
        {
            if (id == null)
            {
                return NotFound();
            }

            var xe = await _context.Xe
                .Include(x => x.LoaiXe)
                .FirstOrDefaultAsync(x => x.MaXe == id);

            if (xe == null)
            {
                return NotFound();
            }

            // Lấy lịch sử hợp đồng của xe
            var query = _context.ChiTietHopDong
                .Include(ct => ct.HopDong)
                .Include(ct => ct.HopDong.KhachHang)
                .Where(ct => ct.MaXe == id)
                .AsQueryable();

            // Lọc theo thời gian
            if (!string.IsNullOrEmpty(timeFilter))
            {
                var now = DateTime.Now;
                switch (timeFilter)
                {
                    case "week":
                        var weekStart = now.AddDays(-(int)now.DayOfWeek);
                        query = query.Where(ct => ct.HopDong.NgayNhanXe >= weekStart);
                        break;
                    case "month":
                        var monthStart = new DateTime(now.Year, now.Month, 1);
                        query = query.Where(ct => ct.HopDong.NgayNhanXe >= monthStart);
                        break;
                    case "year":
                        var yearStart = new DateTime(now.Year, 1, 1);
                        query = query.Where(ct => ct.HopDong.NgayNhanXe >= yearStart);
                        break;
                }
            }

            // Lọc theo khoảng thời gian tùy chỉnh
            if (startDate.HasValue)
            {
                query = query.Where(ct => ct.HopDong.NgayNhanXe >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(ct => ct.HopDong.NgayNhanXe <= endDate.Value);
            }

            // Tìm kiếm theo tên hoặc số điện thoại khách hàng
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(ct => 
                    ct.HopDong.HoTenKhach.Contains(searchString) || 
                    ct.HopDong.SoDienThoai.Contains(searchString));
            }

            var lichSuHopDong = await query
                .OrderByDescending(ct => ct.HopDong.NgayNhanXe)
                .ToListAsync();

            ViewBag.Xe = xe;
            ViewBag.SearchString = searchString;
            ViewBag.TimeFilter = timeFilter;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            // Kiểm tra nếu là AJAX request thì trả về partial view
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_LichSuHopDongPartial", lichSuHopDong);
            }

            return View(lichSuHopDong);
        }

        // GET: Xe/KiemTraBienSo
        [HttpGet]
        public async Task<IActionResult> KiemTraBienSo(string bienSoXe)
        {
            if (string.IsNullOrEmpty(bienSoXe))
                return Json(new { isValid = false, message = "Biển số xe không được để trống" });

            var existingXe = await _context.Xe.FirstOrDefaultAsync(x => x.BienSoXe == bienSoXe);
            if (existingXe != null)
            {
                return Json(new { isValid = false, message = "Biển số xe đã tồn tại trong hệ thống" });
            }

            return Json(new { isValid = true, message = "Biển số xe hợp lệ" });
        }

        // GET: Xe/FilterXe - Action cho lọc real-time
        [HttpGet]
        [PermissionAuthorize("CanViewXe")]
        public async Task<IActionResult> FilterXe(string searchString, int? loaiXe, string hangXe, bool? showDeleted = false)
        {
            // Debug: Log các tham số filter
            System.Diagnostics.Debug.WriteLine($"FilterXe called with: searchString='{searchString}', loaiXe={loaiXe}, hangXe='{hangXe}', showDeleted={showDeleted}");

            var xeList = await GetFilteredXeList(searchString, loaiXe, hangXe, showDeleted);
            
            // Debug: Log kết quả
            System.Diagnostics.Debug.WriteLine($"FilterXe result: {xeList.Count} vehicles found");
            foreach (var xe in xeList)
            {
                System.Diagnostics.Debug.WriteLine($"Filtered Vehicle: {xe.TenXe} - Status: '{xe.TrangThai}'");
            }

            // Trả về partial view cho AJAX request
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_XeTablePartial", xeList);
            }

            return View("Index", xeList);
        }
    }
}