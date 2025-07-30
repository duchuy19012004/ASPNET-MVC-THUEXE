using Microsoft.AspNetCore.Mvc;
using bike.Models;
using bike.Repository;
using Microsoft.EntityFrameworkCore;

namespace bike.Controllers
{
    public class QuanLyThietHaiController : Controller
    {
        private readonly BikeDbContext _context;

        public QuanLyThietHaiController(BikeDbContext context)
        {
            _context = context;
        }

        // GET: QuanLyThietHai
        public async Task<IActionResult> Index(string trangThai, string loaiThietHai, DateTime? tuNgay)
        {
            var query = _context.BaoCaoThietHai
                .Include(b => b.ChiTietHopDong)
                .ThenInclude(ct => ct.HopDong)
                .Include(b => b.ChiTietHopDong)
                .ThenInclude(ct => ct.Xe)
                .AsQueryable();

            // Áp dụng filter
            if (!string.IsNullOrEmpty(trangThai))
            {
                query = query.Where(b => b.TrangThaiThanhToan == trangThai);
            }

            if (!string.IsNullOrEmpty(loaiThietHai))
            {
                query = query.Where(b => b.LoaiThietHai == loaiThietHai);
            }

            if (tuNgay.HasValue)
            {
                query = query.Where(b => b.NgayPhatHien.Date >= tuNgay.Value.Date);
            }

            var baoCaoThietHai = await query.OrderByDescending(b => b.NgayPhatHien).ToListAsync();

            // Truyền filter values vào ViewBag
            ViewBag.TrangThai = trangThai;
            ViewBag.LoaiThietHai = loaiThietHai;
            ViewBag.TuNgay = tuNgay;

            return View(baoCaoThietHai);
        }

        // GET: QuanLyThietHai/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var baoCaoThietHai = await _context.BaoCaoThietHai
                .Include(b => b.ChiTietHopDong)
                .ThenInclude(ct => ct.HopDong)
                .Include(b => b.ChiTietHopDong)
                .ThenInclude(ct => ct.Xe)
                .FirstOrDefaultAsync(m => m.MaBaoCao == id);
            
            if (baoCaoThietHai == null)
            {
                return NotFound();
            }

            return View(baoCaoThietHai);
        }

        // GET: QuanLyThietHai/ThanhToan/5
        public async Task<IActionResult> ThanhToan(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var baoCaoThietHai = await _context.BaoCaoThietHai
                .Include(b => b.ChiTietHopDong)
                .ThenInclude(ct => ct.HopDong)
                .Include(b => b.ChiTietHopDong)
                .ThenInclude(ct => ct.Xe)
                .FirstOrDefaultAsync(m => m.MaBaoCao == id);
            
            if (baoCaoThietHai == null)
            {
                return NotFound();
            }

            return View(baoCaoThietHai);
        }

        // POST: QuanLyThietHai/ThanhToan
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThanhToan(int id, [Bind("MaBaoCao,SoTienDaThanhToan,GhiChuThanhToan")] ThanhToanThietHaiRequest request)
        {
            var baoCaoThietHai = await _context.BaoCaoThietHai.FindAsync(id);
            if (baoCaoThietHai == null)
            {
                return NotFound();
            }

            // Log để debug
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["ErrorMessage"] = string.Join("; ", errors);
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    // Cập nhật thông tin thanh toán
                    baoCaoThietHai.SoTienDaThanhToan += request.SoTienDaThanhToan;
                    baoCaoThietHai.NgayThanhToan = DateTime.Now;
                    baoCaoThietHai.GhiChuThanhToan = request.GhiChuThanhToan;
                    baoCaoThietHai.NgayCapNhat = DateTime.Now;

                    // Cập nhật trạng thái thanh toán
                    if (baoCaoThietHai.SoTienDaThanhToan >= baoCaoThietHai.ChiPhiSuaChuaUocTinh)
                    {
                        baoCaoThietHai.TrangThaiThanhToan = "Đã thanh toán đủ";
                    }
                    else if (baoCaoThietHai.SoTienDaThanhToan > 0)
                    {
                        baoCaoThietHai.TrangThaiThanhToan = "Đã thanh toán một phần";
                    }

                    _context.Update(baoCaoThietHai);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Đã cập nhật thanh toán thành công. Số tiền đã thanh toán: {baoCaoThietHai.SoTienDaThanhToan:N0}đ";
                    return RedirectToAction(nameof(Details), new { id = id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BaoCaoThietHaiExists(baoCaoThietHai.MaBaoCao))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Nếu có lỗi, load lại dữ liệu để hiển thị
            var baoCaoReload = await _context.BaoCaoThietHai
                .Include(b => b.ChiTietHopDong)
                .ThenInclude(ct => ct.HopDong)
                .Include(b => b.ChiTietHopDong)
                .ThenInclude(ct => ct.Xe)
                .FirstOrDefaultAsync(m => m.MaBaoCao == id);

            return View(baoCaoReload);
        }

        // API để lấy thông tin thanh toán
        [HttpGet]
        public async Task<IActionResult> GetThanhToanInfo(int maHopDong)
        {
            try
            {
                var baoCaoThietHai = await _context.BaoCaoThietHai
                    .Where(b => b.ChiTietHopDong.HopDong.MaHopDong == maHopDong)
                    .ToListAsync();

                var result = baoCaoThietHai.Select(b => new
                {
                    MaBaoCao = b.MaBaoCao,
                    LoaiThietHai = b.LoaiThietHai,
                    PhiDenBuKhachHang = b.PhiDenBuKhachHang,
                    SoTienDaThanhToan = b.SoTienDaThanhToan,
                    SoTienConLai = b.SoTienConLai,
                    TrangThaiThanhToan = b.TrangThaiThanhToan,
                    NgayThanhToan = b.NgayThanhToan
                });

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: QuanLyThietHai/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var baoCaoThietHai = await _context.BaoCaoThietHai
                .Include(b => b.ChiTietHopDong)
                .ThenInclude(ct => ct.HopDong)
                .Include(b => b.ChiTietHopDong)
                .ThenInclude(ct => ct.Xe)
                .FirstOrDefaultAsync(m => m.MaBaoCao == id);
            
            if (baoCaoThietHai == null)
            {
                return NotFound();
            }

            return View(baoCaoThietHai);
        }

        // POST: QuanLyThietHai/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaBaoCao,MaChiTiet,NgayTao,NgayPhatHien,LoaiThietHai,LaThietHaiNang,ViTriThietHai,MoTaChiTiet,ChiPhiSuaChuaUocTinh,ChiPhiSuaChuaThucTe,GiaTriXeTruocKhiHong,GiaTriXeSauKhiHong,PhiDenBuKhachHang,SoTienDaThanhToan,TrangThaiThanhToan,GhiChuThanhToan")] BaoCaoThietHai baoCaoThietHai)
        {
            if (id != baoCaoThietHai.MaBaoCao)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["ErrorMessage"] = string.Join("; ", errors);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Cập nhật ngày cập nhật
                    baoCaoThietHai.NgayCapNhat = DateTime.Now;

                    // Cập nhật trạng thái thanh toán dựa trên số tiền đã thanh toán
                    if (baoCaoThietHai.SoTienDaThanhToan >= baoCaoThietHai.PhiDenBuKhachHang)
                    {
                        baoCaoThietHai.TrangThaiThanhToan = "Đã thanh toán đủ";
                    }
                    else if (baoCaoThietHai.SoTienDaThanhToan > 0)
                    {
                        baoCaoThietHai.TrangThaiThanhToan = "Đã thanh toán một phần";
                    }
                    else
                    {
                        baoCaoThietHai.TrangThaiThanhToan = "Chưa thanh toán";
                    }

                    _context.Update(baoCaoThietHai);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Cập nhật báo cáo thiệt hại thành công!";
                    return RedirectToAction(nameof(Details), new { id = id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BaoCaoThietHaiExists(baoCaoThietHai.MaBaoCao))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Nếu có lỗi, load lại dữ liệu để hiển thị
            var baoCaoReload = await _context.BaoCaoThietHai
                .Include(b => b.ChiTietHopDong)
                .ThenInclude(ct => ct.HopDong)
                .Include(b => b.ChiTietHopDong)
                .ThenInclude(ct => ct.Xe)
                .FirstOrDefaultAsync(m => m.MaBaoCao == id);

            return View(baoCaoReload);
        }

        // API để tính tổng tiền khách đã trả (bao gồm cả phí đền bù)
        [HttpGet]
        public async Task<IActionResult> GetTongTienKhachDaTra(int maHopDong)
        {
            try
            {
                // Lấy thông tin hợp đồng
                var hopDong = await _context.HopDong
                    .Include(h => h.HoaDon)
                    .FirstOrDefaultAsync(h => h.MaHopDong == maHopDong);

                if (hopDong == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy hợp đồng" });
                }

                // Tính tiền thuê xe (từ hóa đơn)
                decimal tienThueXe = hopDong.HoaDon?.SoTien ?? hopDong.TongTien;

                // Tính tổng phí đền bù đã thanh toán
                var tongPhiDenBuDaTra = await _context.BaoCaoThietHai
                    .Where(b => b.ChiTietHopDong.HopDong.MaHopDong == maHopDong)
                    .SumAsync(b => b.SoTienDaThanhToan);

                var tongTienKhachDaTra = tienThueXe + tongPhiDenBuDaTra;

                return Json(new { 
                    success = true, 
                    data = new {
                        TienThueXe = tienThueXe,
                        TongPhiDenBuDaTra = tongPhiDenBuDaTra,
                        TongTienKhachDaTra = tongTienKhachDaTra
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private bool BaoCaoThietHaiExists(int id)
        {
            return _context.BaoCaoThietHai.Any(e => e.MaBaoCao == id);
        }
    }

    public class ThanhToanThietHaiRequest
    {
        public int MaBaoCao { get; set; }
        public decimal SoTienDaThanhToan { get; set; }
        public string? GhiChuThanhToan { get; set; }
    }
} 