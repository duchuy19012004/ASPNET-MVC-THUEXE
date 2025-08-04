using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        public async Task<IActionResult> Index(string searchString, int? loaiXe, string hangXe, string trangThai, bool? showDeleted = false)
        {
            var xeList = await GetFilteredXeList(searchString, loaiXe, hangXe, trangThai, showDeleted);
            
            ViewBag.TongSoXe = await _context.Xe.CountAsync(x => x.TrangThai != "Đã xóa");
            ViewBag.XeSanSang = await _context.Xe.CountAsync(x => x.TrangThai == "Sẵn sàng");
            ViewBag.DangChoThue = await _context.Xe.CountAsync(x => x.TrangThai == "Đang thuê");
            ViewBag.BaoTri = await _context.Xe.CountAsync(x => x.TrangThai == "Bảo trì");
            ViewBag.ShowDeleted = showDeleted;

            ViewBag.LoaiXeList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.LoaiXe, "MaLoaiXe", "TenLoaiXe", loaiXe);
            
            var hangXeList = await _context.Xe
                .Where(x => !string.IsNullOrEmpty(x.HangXe))
                .Select(x => x.HangXe)
                .Distinct()
                .ToListAsync();
            ViewBag.HangXeList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(hangXeList, hangXe);

            // Tạo danh sách trạng thái với value và text giống nhau
            var trangThaiList = new List<SelectListItem>
            {
                new SelectListItem { Value = "Sẵn sàng", Text = "Sẵn sàng" },
                new SelectListItem { Value = "Đang thuê", Text = "Đang thuê" },
                new SelectListItem { Value = "Bảo trì", Text = "Bảo trì" },
                new SelectListItem { Value = "Hư hỏng", Text = "Hư hỏng" },
                new SelectListItem { Value = "Mất", Text = "Mất" }
            };
            ViewBag.TrangThaiList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(trangThaiList, "Value", "Text", trangThai);

            ViewBag.CurrentSearchString = searchString;
            ViewBag.CurrentLoaiXe = loaiXe;
            ViewBag.CurrentHangXe = hangXe;
            ViewBag.CurrentTrangThai = trangThai;

            return View(xeList);
        }
        private async Task<List<Xe>> GetFilteredXeList(string searchString, int? loaiXe, string hangXe, string trangThai, bool? showDeleted = false)
        {
            var query = _context.Xe
                .Include(x => x.LoaiXe)
                .Include(x => x.ChiTieu)
                .Include(x => x.HinhAnhXes)
                .AsQueryable();

            if (!string.IsNullOrEmpty(trangThai))
            {
                query = query.Where(x => x.TrangThai == trangThai);
            }
            else if (showDeleted.HasValue && showDeleted.Value)
            {
                query = query.Where(x => x.TrangThai == "Đã xóa");
            }
            else
            {
                query = query.Where(x => x.TrangThai != "Đã xóa");
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(x => x.TenXe.Contains(searchString) || x.BienSoXe.Contains(searchString));
            }

            if (loaiXe.HasValue)
            {
                query = query.Where(x => x.MaLoaiXe == loaiXe.Value);
            }

            if (!string.IsNullOrEmpty(hangXe))
            {
                query = query.Where(x => x.HangXe == hangXe);
            }

            var result = await query.ToListAsync();
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
                _context.Add(xe);
                await _context.SaveChangesAsync();

                if (hinhAnh != null && hinhAnh.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(hinhAnh.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "xe", fileName);
                    
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await hinhAnh.CopyToAsync(stream);
                    }

                    var hinhAnhChinh = new HinhAnhXe
                    {
                        MaXe = xe.MaXe,
                        TenFile = fileName,
                        LaAnhChinh = true,
                        ThuTu = 1
                    };
                    _context.HinhAnhXe.Add(hinhAnhChinh);
                }

                if (hinhAnhKhac != null && hinhAnhKhac.Count > 0)
                {
                    int thuTu = 2;
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
                xe.TrangThai = "Sẵn sàng";
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

            var query = _context.ChiTietHopDong
                .Include(ct => ct.HopDong)
                .Include(ct => ct.HopDong.KhachHang)
                .Where(ct => ct.MaXe == id)
                .AsQueryable();

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

            if (startDate.HasValue)
            {
                query = query.Where(ct => ct.HopDong.NgayNhanXe >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(ct => ct.HopDong.NgayNhanXe <= endDate.Value);
            }

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
                return Json(false);
            var existingXe = await _context.Xe.FirstOrDefaultAsync(x => x.BienSoXe == bienSoXe);
            if (existingXe != null)
            {
                return Json("Biển số xe đã tồn tại trong hệ thống");
            }
            return Json(true);
        }

        // GET: Xe/FilterXe
        [HttpGet]
        [PermissionAuthorize("CanViewXe")]
        public async Task<IActionResult> FilterXe(string searchString, int? loaiXe, string hangXe, string trangThai, bool? showDeleted = false)
        {
            var xeList = await GetFilteredXeList(searchString, loaiXe, hangXe, trangThai, showDeleted);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_XeTablePartial", xeList);
            }

            return View("Index", xeList);
        }

        // GET: Xe/GetXeDetails
        [HttpGet]
        [PermissionAuthorize("CanViewXe")]
        public async Task<IActionResult> GetXeDetails(int id)
        {
            var xe = await _context.Xe
                .Include(x => x.LoaiXe)
                .Include(x => x.ChiTieu)
                .Include(x => x.HinhAnhXes)
                .Include(x => x.ChiTietHopDong)
                    .ThenInclude(ct => ct.HopDong)
                .FirstOrDefaultAsync(x => x.MaXe == id);

            if (xe == null)
            {
                return Json(new { success = false, message = "Không tìm thấy xe" });
            }

            // Tính toán thống kê
            var tongChiPhi = xe.ChiTieu?.Sum(c => c.SoTien) ?? 0;
            var soHopDong = xe.ChiTietHopDong?.Count ?? 0;
            var soHinhAnh = xe.HinhAnhXes?.Count ?? 0;

            var xeData = new
            {
                success = true,
                xe = new
                {
                    maXe = xe.MaXe,
                    tenXe = xe.TenXe,
                    bienSoXe = xe.BienSoXe,
                    hangXe = xe.HangXe,
                    dongXe = xe.DongXe,
                    trangThai = xe.TrangThai,
                    giaThue = xe.GiaThue,
                    giaTriXe = xe.GiaTriXe,
                    ngayGapSuCo = xe.NgayGapSuCo,
                    moTaThietHai = xe.MoTaThietHai,
                    chiPhiSuaChua = xe.ChiPhiSuaChua,
                    loaiXe = xe.LoaiXe?.TenLoaiXe,
                    hinhAnhHienThi = xe.HinhAnhHienThi,
                    tongChiPhi = tongChiPhi,
                    soHopDong = soHopDong,
                    soHinhAnh = soHinhAnh
                }
            };

            return Json(xeData);
        }

        [HttpGet]
        [PermissionAuthorize("CanViewXe")]
        public async Task<IActionResult> GetXeLichSuHopDong(int id)
        {
            var xe = await _context.Xe
                .Include(x => x.ChiTietHopDong)
                    .ThenInclude(ct => ct.HopDong)
                .Include(x => x.ChiTietHopDong)
                    .ThenInclude(ct => ct.HopDong.HoaDon)
                .FirstOrDefaultAsync(x => x.MaXe == id);

            if (xe == null)
            {
                return Json(new { success = false, message = "Không tìm thấy xe" });
            }

            var lichSuHopDong = xe.ChiTietHopDong
                .OrderByDescending(ct => ct.HopDong.NgayNhanXe)
                .Select(ct => new
                {
                    maHopDong = ct.HopDong.MaHopDong,
                    hoTenKhach = ct.HopDong.HoTenKhach,
                    soCCCD = ct.HopDong.SoCCCD,
                    soDienThoai = ct.HopDong.SoDienThoai,
                    ngayNhanXe = ct.HopDong.NgayNhanXe,
                    ngayTraXeThucTe = ct.HopDong.NgayTraXeThucTe,
                    thanhTienTinhToan = ct.ThanhTienTinhToan,
                    coHoaDon = ct.HopDong.HoaDon != null,
                    maHoaDon = ct.HopDong.HoaDon?.MaHoaDon
                })
                .ToList();

            var thongKe = new
            {
                tongHopDong = lichSuHopDong.Count,
                daHoanThanh = lichSuHopDong.Count(x => x.ngayTraXeThucTe.HasValue),
                dangThue = lichSuHopDong.Count(x => !x.ngayTraXeThucTe.HasValue),
                tongDoanhThu = lichSuHopDong.Sum(x => x.thanhTienTinhToan)
            };

            var result = new
            {
                success = true,
                lichSuHopDong = lichSuHopDong,
                thongKe = thongKe
            };

            return Json(result);
        }
    }
}