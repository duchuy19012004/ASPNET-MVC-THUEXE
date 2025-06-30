using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bike.Repository;
using bike.Models;
using bike.Attributes;
using System.Security.Claims;

namespace bike.Controllers
{
    [CustomAuthorize("Admin", "Staff")]
    public class QuanLyHoaDonController : Controller
    {
        private readonly BikeDbContext _context;

        public QuanLyHoaDonController(BikeDbContext context)
        {
            _context = context;
        }

        // GET: QuanLyHoaDon - Danh sách hóa đơn
        public async Task<IActionResult> Index(string searchString, DateTime? tuNgay, DateTime? denNgay, int page = 1, int pageSize = 10)
        {
            var query = _context.HoaDon
                .Include(h => h.HopDong)
                    .ThenInclude(hd => hd.Xe)
                .Include(h => h.NguoiTao)
                .AsQueryable();

            // Tìm kiếm theo số điện thoại khách hàng
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(h => h.HopDong.SoDienThoai.Contains(searchString) ||
                                        h.HopDong.HoTenKhach.Contains(searchString));
            }

            // Lọc theo thời gian
            if (tuNgay.HasValue)
            {
                query = query.Where(h => h.NgayThanhToan >= tuNgay.Value);
            }
            if (denNgay.HasValue)
            {
                query = query.Where(h => h.NgayThanhToan <= denNgay.Value);
            }

            // Tính toán phân trang
            int totalItems = await query.CountAsync();
            var hoaDons = await query
                .OrderByDescending(h => h.NgayTao)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Thống kê
            var tongDoanhThu = await query.SumAsync(h => h.SoTien);
            var soHoaDonHomNay = await _context.HoaDon
                .Where(h => h.NgayThanhToan.Date == DateTime.Today)
                .CountAsync();

            ViewBag.SearchString = searchString;
            ViewBag.TuNgay = tuNgay;
            ViewBag.DenNgay = denNgay;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TongDoanhThu = tongDoanhThu;
            ViewBag.SoHoaDonHomNay = soHoaDonHomNay;

            return View(hoaDons);
        }

        // GET: QuanLyHoaDon/ChiTiet/5 - Xem chi tiết hóa đơn
        public async Task<IActionResult> ChiTiet(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hoaDon = await _context.HoaDon
                .Include(h => h.HopDong)
                    .ThenInclude(hd => hd.Xe)
                .Include(h => h.NguoiTao)
                .FirstOrDefaultAsync(h => h.MaHoaDon == id);

            if (hoaDon == null)
            {
                return NotFound();
            }

            return View(hoaDon);
        }

        // POST: QuanLyHoaDon/TaoHoaDon/5 - Tạo hóa đơn từ hợp đồng hoàn thành
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoHoaDon(int maHopDong, string? ghiChu)
        {
            try
            {
                // Kiểm tra hợp đồng
                var hopDong = await _context.HopDong
                    .Include(h => h.HoaDon)
                    .FirstOrDefaultAsync(h => h.MaHopDong == maHopDong);

                if (hopDong == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy hợp đồng!" });
                }

                // Kiểm tra trạng thái hợp đồng
                if (hopDong.TrangThai != "Hoàn thành")
                {
                    return Json(new { success = false, message = "Chỉ có thể tạo hóa đơn cho hợp đồng đã hoàn thành!" });
                }

                // Kiểm tra đã có hóa đơn chưa
                if (hopDong.HoaDon != null)
                {
                    return Json(new { success = false, message = "Hợp đồng này đã có hóa đơn!" });
                }

                // Tạo hóa đơn mới
                var hoaDon = new HoaDon
                {
                    MaHopDong = hopDong.MaHopDong,
                    NgayThanhToan = DateTime.Now,
                    SoTien = hopDong.TongTien,
                    TrangThai = "Đã thanh toán",
                    GhiChu = ghiChu,
                    NgayTao = DateTime.Now,
                    MaNguoiTao = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier))
                };

                _context.HoaDon.Add(hoaDon);
                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = $"Tạo hóa đơn thành công! Mã HD: HD{hoaDon.MaHoaDon:D6}",
                    hoaDonId = hoaDon.MaHoaDon
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // GET: QuanLyHoaDon/DanhSachHopDongChuaCoHoaDon - Danh sách hợp đồng chưa có hóa đơn
        public async Task<IActionResult> DanhSachHopDongChuaCoHoaDon()
        {
            var hopDongChuaCoHoaDon = await _context.HopDong
                .Include(h => h.Xe)
                .Include(h => h.HoaDon)
                .Where(h => h.TrangThai == "Hoàn thành" && h.HoaDon == null)
                .OrderByDescending(h => h.NgayTao)
                .ToListAsync();

            return View(hopDongChuaCoHoaDon);
        }

        // GET: QuanLyHoaDon/BaoCaoDoanhThu - Báo cáo doanh thu
        public async Task<IActionResult> BaoCaoDoanhThu(DateTime? tuNgay, DateTime? denNgay)
        {
            var startDate = tuNgay ?? DateTime.Now.AddDays(-30).Date;
            var endDate = denNgay ?? DateTime.Now.Date;

            var hoaDons = await _context.HoaDon
                .Include(h => h.HopDong)
                    .ThenInclude(hd => hd.Xe)
                .Where(h => h.NgayThanhToan >= startDate && h.NgayThanhToan <= endDate)
                .OrderByDescending(h => h.NgayThanhToan)
                .ToListAsync();

            ViewBag.TuNgay = startDate;
            ViewBag.DenNgay = endDate;
            ViewBag.TongDoanhThu = hoaDons.Sum(h => h.SoTien);

            return View(hoaDons);
        }
    }
}