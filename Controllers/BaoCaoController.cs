using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using bike.Repository;
using bike.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using bike.Attributes;

namespace bike.Controllers
{
    [CustomAuthorize("Admin", "Staff")]
    public class BaoCaoController : Controller
    {
        private readonly BikeDbContext _context;

        public BaoCaoController(BikeDbContext context)
        {
            _context = context;
        }

        // GET: BaoCao
        public async Task<IActionResult> Index(DateTime? tuNgay, DateTime? denNgay)
        {
            // Nếu không có ngày, mặc định lấy 30 ngày gần nhất
            var endDate = denNgay ?? DateTime.Now.Date;
            var startDate = tuNgay ?? endDate.AddDays(-30);

            var viewModel = new BaoCaoViewModel
            {
                TuNgay = startDate,
                DenNgay = endDate
            };

            // 1. Thống kê tổng quan
            // Tổng đơn đặt xe trong khoảng thời gian
            viewModel.TongDonDatXe = await _context.DatCho
                .Where(d => d.NgayDat >= startDate && d.NgayDat <= endDate.AddDays(1))
                .CountAsync();
            // Doanh thu hôm nay - CHỈ TÍNH KHI ĐÃ TRẢ XE THÀNH CÔNG, TRỪ CHI TIÊU
            var today = DateTime.Now.Date;
            var doanhThuTheoHopDong = await _context.HopDong
                .Where(h => h.TrangThai == "Hoàn thành" && 
                           h.NgayTraXeThucTe.HasValue && 
                           h.NgayTraXeThucTe.Value.Date == today)
                .SumAsync(h => h.TongTien);

            // Tính tổng chi tiêu trong ngày
            var tongChiTieuHomNay = await _context.ChiTieu
                .Where(ct => ct.NgayChi.Date == today)
                .SumAsync(ct => ct.SoTien);

            // Doanh thu thực = Doanh thu hợp đồng - Chi tiêu (tối thiểu là 0)
            viewModel.DoanhThuHomNay = Math.Max(0, doanhThuTheoHopDong - tongChiTieuHomNay);

            // Xe đang cho thuê
            viewModel.XeDangChoThue = await _context.Xe
                .Where(x => x.TrangThai == "Đang thuê")
                .CountAsync();

            // Hợp đồng hoạt động (đang thuê)
            viewModel.HopDongHoatDong = await _context.HopDong
                .Where(h => h.TrangThai == "Đang thuê")
                .CountAsync();

            // Khách hàng mới hôm nay - chỉ đếm user với vai trò "User"
            viewModel.KhachHangMoi = await _context.Users
                .Where(u => u.NgayTao.Date == today && u.VaiTro == "User")
                .CountAsync();

            // Tổng số xe trong hệ thống
            viewModel.TongSoXe = await _context.Xe.CountAsync();

            // 2. Tính % tăng/giảm so với kỳ trước - DỰA TRÊN NGÀY TRẢ XE
            var previousPeriodDays = (endDate - startDate).Days;
            var previousStartDate = startDate.AddDays(-previousPeriodDays - 1);
            var previousEndDate = startDate.AddDays(-1);

            var previousDonDat = await _context.DatCho
                .Where(d => d.NgayDat >= previousStartDate && d.NgayDat <= previousEndDate)
                .CountAsync();

            if (previousDonDat > 0)
            {
                viewModel.PhanTramDonDat = ((double)(viewModel.TongDonDatXe - previousDonDat) / previousDonDat) * 100;
            }

            // Tính doanh thu kỳ trước để so sánh (trừ chi tiêu)
            var previousRevenueGross = await _context.HopDong
                .Where(h => h.TrangThai == "Hoàn thành" && 
                           h.NgayTraXeThucTe.HasValue &&
                           h.NgayTraXeThucTe.Value.Date >= previousStartDate && 
                           h.NgayTraXeThucTe.Value.Date <= previousEndDate)
                .SumAsync(h => h.TongTien);

            var previousExpenses = await _context.ChiTieu
                .Where(ct => ct.NgayChi.Date >= previousStartDate && 
                            ct.NgayChi.Date <= previousEndDate)
                .SumAsync(ct => ct.SoTien);

            var previousRevenue = Math.Max(0, previousRevenueGross - previousExpenses);

            var currentRevenueGross = await _context.HopDong
                .Where(h => h.TrangThai == "Hoàn thành" && 
                           h.NgayTraXeThucTe.HasValue &&
                           h.NgayTraXeThucTe.Value.Date >= startDate && 
                           h.NgayTraXeThucTe.Value.Date <= endDate)
                .SumAsync(h => h.TongTien);

            var currentExpenses = await _context.ChiTieu
                .Where(ct => ct.NgayChi.Date >= startDate && 
                            ct.NgayChi.Date <= endDate)
                .SumAsync(ct => ct.SoTien);

            var currentRevenue = Math.Max(0, currentRevenueGross - currentExpenses);

            if (previousRevenue > 0)
            {
                viewModel.PhanTramDoanhThu = ((double)(currentRevenue - previousRevenue) / (double)previousRevenue) * 100;
            }

            // 3. Dữ liệu biểu đồ doanh thu (7 ngày gần nhất) - CHỈ TÍNH KHI ĐÃ TRẢ XE, TRỪ CHI TIÊU
            var last7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.Now.Date.AddDays(-6 + i))
                .ToList();

            foreach (var date in last7Days)
            {
                var doanhThuNgayGross = await _context.HopDong
                    .Where(h => h.TrangThai == "Hoàn thành" && 
                               h.NgayTraXeThucTe.HasValue && 
                               h.NgayTraXeThucTe.Value.Date == date)
                    .SumAsync(h => h.TongTien);

                var chiTieuNgay = await _context.ChiTieu
                    .Where(ct => ct.NgayChi.Date == date)
                    .SumAsync(ct => ct.SoTien);

                var doanhThuNgay = Math.Max(0, doanhThuNgayGross - chiTieuNgay);

                viewModel.BieuDoDoanhThu.Add(new BieuDoItem
                {
                    Label = date.ToString("dd/MM"),
                    Value = doanhThuNgay
                });
            }

            // 4. Dữ liệu biểu đồ đơn đặt xe (7 ngày gần nhất)
            foreach (var date in last7Days)
            {
                var donDatNgay = await _context.DatCho
                    .Where(d => d.NgayDat.Date == date)
                    .CountAsync();

                viewModel.BieuDoDonDat.Add(new BieuDoItem
                {
                    Label = date.ToString("dd/MM"),
                    Value = donDatNgay
                });
            }

            // 5. Top 5 xe được thuê nhiều nhất - CHỈ TÍNH HỢP ĐỒNG ĐÃ HOÀN THÀNH
            var topXe = await _context.ChiTietHopDong
                .Include(ct => ct.Xe)
                .Include(ct => ct.HopDong)
                .Where(ct => ct.HopDong.TrangThai == "Hoàn thành" && 
                            ct.HopDong.NgayTraXeThucTe.HasValue &&
                            ct.HopDong.NgayTraXeThucTe.Value.Date >= startDate && 
                            ct.HopDong.NgayTraXeThucTe.Value.Date <= endDate)
                .GroupBy(ct => new { ct.Xe.TenXe, ct.Xe.BienSoXe })
                .Select(g => new XeThueNhieuItem
                {
                    TenXe = g.Key.TenXe,
                    BienSo = g.Key.BienSoXe,
                    SoLanThue = g.Count(),
                    DoanhThu = g.Sum(ct => ct.ThanhTien)
                })
                .OrderByDescending(x => x.SoLanThue)
                .Take(5)
                .ToListAsync();

            viewModel.TopXeThueNhieu = topXe;

            // 6. 10 đơn đặt gần đây
            var donGanDay = await _context.DatCho
                .Include(d => d.Xe)
                .OrderByDescending(d => d.NgayDat)
                .Take(10)
                .Select(d => new DonDatGanDayItem
                {
                    MaDatCho = d.MaDatCho,
                    TenKhach = d.HoTen,
                    TenXe = d.Xe.TenXe,
                    NgayDat = d.NgayDat,
                    TrangThai = d.TrangThai,
                    TongTien = d.TongTienDuKien
                })
                .ToListAsync();

            viewModel.DonDatGanDay = donGanDay;

            return View(viewModel);
        }

        // GET: BaoCao/DoanhThuTheoThang - Báo cáo doanh thu theo tháng
        public async Task<IActionResult> DoanhThuTheoThang(int? year)
        {
            var currentYear = year ?? DateTime.Now.Year;
            var monthlyRevenue = new List<BieuDoItem>();

            for (int month = 1; month <= 12; month++)
            {
                var startDate = new DateTime(currentYear, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                // CHỈ TÍNH DOANH THU KHI ĐÃ TRẢ XE THÀNH CÔNG, TRỪ CHI TIÊU
                var revenueGross = await _context.HopDong
                    .Where(h => h.TrangThai == "Hoàn thành" && 
                               h.NgayTraXeThucTe.HasValue &&
                               h.NgayTraXeThucTe.Value.Date >= startDate && 
                               h.NgayTraXeThucTe.Value.Date <= endDate)
                    .SumAsync(h => h.TongTien);

                var expenses = await _context.ChiTieu
                    .Where(ct => ct.NgayChi.Date >= startDate && 
                                ct.NgayChi.Date <= endDate)
                    .SumAsync(ct => ct.SoTien);

                var revenue = Math.Max(0, revenueGross - expenses);

                monthlyRevenue.Add(new BieuDoItem
                {
                    Label = $"Tháng {month}",
                    Value = revenue
                });
            }

            ViewBag.Year = currentYear;
            ViewBag.Years = Enumerable.Range(2020, DateTime.Now.Year - 2020 + 1).Reverse();

            return View(monthlyRevenue);
        }

        // GET: BaoCao/ExportExcel - Xuất báo cáo Excel
        public async Task<IActionResult> ExportExcel(DateTime? tuNgay, DateTime? denNgay)
        {
            // TODO: Implement Excel export using EPPlus or similar library
            TempData["Info"] = "Chức năng xuất Excel đang được phát triển";
            return RedirectToAction(nameof(Index));
        }
    }
}