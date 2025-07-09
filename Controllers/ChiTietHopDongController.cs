using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using bike.Models;
using bike.Repository;
using bike.Attributes;
using System.Security.Claims;

namespace bike.Controllers
{
    [CustomAuthorize("Admin", "Staff")]
    public class ChiTietHopDongController : Controller
    {
        private readonly BikeDbContext _context;

        public ChiTietHopDongController(BikeDbContext context)
        {
            _context = context;
        }

        // GET: ChiTietHopDong/ThemXe/5 - Form thêm xe vào hợp đồng
        [HttpGet]
        public async Task<IActionResult> ThemXe(int? hopDongId)
        {
            if (hopDongId == null)
            {
                return NotFound();
            }

            var hopDong = await _context.HopDong
                .Include(h => h.ChiTietHopDong)
                .ThenInclude(ct => ct.Xe)
                .FirstOrDefaultAsync(h => h.MaHopDong == hopDongId);

            if (hopDong == null)
            {
                return NotFound();
            }

            // Kiểm tra trạng thái hợp đồng
            if (hopDong.TrangThai != "Đang thuê")
            {
                TempData["Error"] = "Chỉ có thể thêm xe vào hợp đồng đang thuê!";
                return RedirectToAction("ChiTiet", "QuanLyHopDong", new { id = hopDongId });
            }

            // Lấy danh sách xe đã có trong hợp đồng
            var xeDaCoTrongHopDong = hopDong.ChiTietHopDong.Select(ct => ct.MaXe).ToList();

            // Lấy danh sách xe khả dụng (sẵn sàng và chưa có trong hợp đồng)
            var xeKhaDung = await _context.Xe
                .Where(x => x.TrangThai == "Sẵn sàng" && !xeDaCoTrongHopDong.Contains(x.MaXe))
                .Select(x => new
                {
                    x.MaXe,
                    Display = $"{x.TenXe} - {x.BienSoXe} - {x.GiaThue:N0}đ/ngày"
                })
                .ToListAsync();

            if (!xeKhaDung.Any())
            {
                TempData["Error"] = "Không có xe nào khả dụng để thêm vào hợp đồng!";
                return RedirectToAction("ChiTiet", "QuanLyHopDong", new { id = hopDongId });
            }

            ViewBag.XeList = new SelectList(xeKhaDung, "MaXe", "Display");
            ViewBag.HopDong = hopDong;

            // Tạo model với giá trị mặc định
            var chiTiet = new ChiTietHopDong
            {
                MaHopDong = hopDong.MaHopDong,
                NgayNhanXe = hopDong.NgayNhanXe,
                NgayTraXeDuKien = hopDong.NgayTraXeDuKien,
                TrangThaiXe = "Đang thuê"
            };

            return View(chiTiet);
        }

        // POST: ChiTietHopDong/ThemXe
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemXe(ChiTietHopDong chiTiet)
        {
            var hopDong = await _context.HopDong
                .Include(h => h.ChiTietHopDong)
                .FirstOrDefaultAsync(h => h.MaHopDong == chiTiet.MaHopDong);

            if (hopDong == null)
            {
                return NotFound();
            }

            // Kiểm tra xe
            var xe = await _context.Xe.FindAsync(chiTiet.MaXe);
            if (xe == null || xe.TrangThai != "Sẵn sàng")
            {
                ModelState.AddModelError("MaXe", "Xe không khả dụng!");
            }

            // Kiểm tra xe đã có trong hợp đồng chưa
            var xeDaTonTai = hopDong.ChiTietHopDong.Any(ct => ct.MaXe == chiTiet.MaXe);
            if (xeDaTonTai)
            {
                ModelState.AddModelError("MaXe", "Xe này đã có trong hợp đồng!");
            }

            if (ModelState.IsValid)
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Set thông tin tự động
                        chiTiet.GiaThueNgay = xe.GiaThue;
                        chiTiet.SoNgayThue = (chiTiet.NgayTraXeDuKien - chiTiet.NgayNhanXe).Days + 1;
                        chiTiet.ThanhTien = chiTiet.GiaThueNgay * chiTiet.SoNgayThue;
                        chiTiet.NgayTao = DateTime.Now;
                        chiTiet.TrangThaiXe = "Đang thuê";

                        // Thêm chi tiết hợp đồng
                        _context.ChiTietHopDong.Add(chiTiet);
                        await _context.SaveChangesAsync();

                        // Cập nhật trạng thái xe
                        xe.TrangThai = "Đang thuê";

                        // Cập nhật tổng tiền hợp đồng
                        hopDong.TongTien = hopDong.ChiTietHopDong.Sum(ct => ct.ThanhTien) + chiTiet.ThanhTien + hopDong.PhuPhi;

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        TempData["Success"] = $"Thêm xe {xe.TenXe} vào hợp đồng thành công!";
                        return RedirectToAction("ChiTiet", "QuanLyHopDong", new { id = chiTiet.MaHopDong });
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                    }
                }
            }

            // Reload data nếu có lỗi
            var xeDaCoTrongHopDong = hopDong.ChiTietHopDong.Select(ct => ct.MaXe).ToList();
            var xeKhaDung = await _context.Xe
                .Where(x => x.TrangThai == "Sẵn sàng" && !xeDaCoTrongHopDong.Contains(x.MaXe))
                .Select(x => new
                {
                    x.MaXe,
                    Display = $"{x.TenXe} - {x.BienSoXe} - {x.GiaThue:N0}đ/ngày"
                })
                .ToListAsync();

            ViewBag.XeList = new SelectList(xeKhaDung, "MaXe", "Display");
            ViewBag.HopDong = hopDong;

            return View(chiTiet);
        }

        // POST: ChiTietHopDong/XoaXe/5 - Xóa xe khỏi hợp đồng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaXe(int id)
        {
            var chiTiet = await _context.ChiTietHopDong
                .Include(ct => ct.HopDong)
                .Include(ct => ct.Xe)
                .FirstOrDefaultAsync(ct => ct.MaChiTiet == id);

            if (chiTiet == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin xe trong hợp đồng!" });
            }

            // Kiểm tra trạng thái hợp đồng
            if (chiTiet.HopDong.TrangThai != "Đang thuê")
            {
                return Json(new { success = false, message = "Chỉ có thể xóa xe khỏi hợp đồng đang thuê!" });
            }

            // Kiểm tra số lượng xe trong hợp đồng (phải có ít nhất 1 xe)
            var soXeTrongHopDong = await _context.ChiTietHopDong.CountAsync(ct => ct.MaHopDong == chiTiet.MaHopDong);
            if (soXeTrongHopDong <= 1)
            {
                return Json(new { success = false, message = "Hợp đồng phải có ít nhất 1 xe!" });
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Xóa chi tiết hợp đồng
                    _context.ChiTietHopDong.Remove(chiTiet);

                    // Cập nhật trạng thái xe về sẵn sàng
                    chiTiet.Xe.TrangThai = "Sẵn sàng";

                    // Cập nhật tổng tiền hợp đồng
                    var hopDong = chiTiet.HopDong;
                    hopDong.TongTien = hopDong.TongTien - chiTiet.ThanhTien;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Json(new { 
                        success = true, 
                        message = $"Xóa xe {chiTiet.Xe.TenXe} khỏi hợp đồng thành công!" 
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
                }
            }
        }

        // GET: ChiTietHopDong/SuaXe/5 - Form sửa thông tin xe trong hợp đồng
        [HttpGet]
        public async Task<IActionResult> SuaXe(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chiTiet = await _context.ChiTietHopDong
                .Include(ct => ct.HopDong)
                .Include(ct => ct.Xe)
                .FirstOrDefaultAsync(ct => ct.MaChiTiet == id);

            if (chiTiet == null)
            {
                return NotFound();
            }

            // Kiểm tra trạng thái hợp đồng
            if (chiTiet.HopDong.TrangThai != "Đang thuê")
            {
                TempData["Error"] = "Chỉ có thể sửa thông tin xe trong hợp đồng đang thuê!";
                return RedirectToAction("ChiTiet", "QuanLyHopDong", new { id = chiTiet.MaHopDong });
            }

            return View(chiTiet);
        }

        // POST: ChiTietHopDong/SuaXe/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaXe(int id, ChiTietHopDong model)
        {
            if (id != model.MaChiTiet)
            {
                return NotFound();
            }

            var chiTiet = await _context.ChiTietHopDong
                .Include(ct => ct.HopDong)
                .Include(ct => ct.Xe)
                .FirstOrDefaultAsync(ct => ct.MaChiTiet == id);

            if (chiTiet == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Lưu thành tiền cũ để tính toán lại tổng tiền
                        var thanhTienCu = chiTiet.ThanhTien;

                        // Cập nhật thông tin
                        chiTiet.GiaThueNgay = model.GiaThueNgay;
                        chiTiet.NgayNhanXe = model.NgayNhanXe;
                        chiTiet.NgayTraXeDuKien = model.NgayTraXeDuKien;
                        chiTiet.SoNgayThue = (model.NgayTraXeDuKien - model.NgayNhanXe).Days + 1;
                        chiTiet.ThanhTien = chiTiet.GiaThueNgay * chiTiet.SoNgayThue;
                        chiTiet.GhiChu = model.GhiChu;

                        // Cập nhật tổng tiền hợp đồng
                        var hopDong = chiTiet.HopDong;
                        hopDong.TongTien = hopDong.TongTien - thanhTienCu + chiTiet.ThanhTien;

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        TempData["Success"] = "Cập nhật thông tin xe thành công!";
                        return RedirectToAction("ChiTiet", "QuanLyHopDong", new { id = chiTiet.MaHopDong });
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                    }
                }
            }

            return View(chiTiet);
        }

        // GET: ChiTietHopDong/DanhSachXe/5 - Xem danh sách xe trong hợp đồng
        public async Task<IActionResult> DanhSachXe(int? hopDongId)
        {
            if (hopDongId == null)
            {
                return NotFound();
            }

            var hopDong = await _context.HopDong
                .Include(h => h.ChiTietHopDong)
                .ThenInclude(ct => ct.Xe)
                .FirstOrDefaultAsync(h => h.MaHopDong == hopDongId);

            if (hopDong == null)
            {
                return NotFound();
            }

            ViewBag.HopDong = hopDong;
            return View(hopDong.ChiTietHopDong.ToList());
        }

        // GET: ChiTietHopDong/GetXeInfo/5 - API lấy thông tin xe
        [HttpGet]
        public async Task<IActionResult> GetXeInfo(int maXe)
        {
            var xe = await _context.Xe.FindAsync(maXe);
            if (xe == null)
            {
                return Json(new { success = false, message = "Không tìm thấy xe" });
            }

            return Json(new
            {
                success = true,
                data = new
                {
                    xe.MaXe,
                    xe.TenXe,
                    xe.BienSoXe,
                    xe.HangXe,
                    xe.DongXe,
                    xe.GiaThue,
                    xe.TrangThai
                }
            });
        }
    }
} 