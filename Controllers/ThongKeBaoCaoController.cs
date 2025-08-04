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
    public class ThongKeBaoCaoController : Controller
    {
        private readonly BikeDbContext _context;

        public ThongKeBaoCaoController(BikeDbContext context)
        {
            _context = context;
        }

        // GET: ThongKeBaoCao
        [PermissionAuthorize("CanViewBaoCao")]
        public async Task<IActionResult> Index(string chartFilter = "today")
        {
            var viewModel = new BaoCaoViewModel
            {
                ChartFilter = chartFilter ?? "7days"
            };

            // Dữ liệu biểu đồ theo filter được chọn
            var chartPeriods = GetChartPeriods(chartFilter);

            // 1. Dữ liệu biểu đồ doanh thu
            foreach (var period in chartPeriods)
            {
                var doanhThuNgayGross = await _context.HopDong
                    .Where(h => h.TrangThai == "Hoàn thành" && 
                               h.NgayTraXeThucTe.HasValue && 
                               h.NgayTraXeThucTe.Value.Date >= period.StartDate &&
                               h.NgayTraXeThucTe.Value.Date <= period.EndDate)
                    .SumAsync(h => h.TongTien);

                var chiTieuNgay = await _context.ChiTieu
                    .Where(ct => ct.NgayChi.Date >= period.StartDate &&
                                ct.NgayChi.Date <= period.EndDate)
                    .SumAsync(ct => ct.SoTien);

                var doanhThuNgay = Math.Max(0, doanhThuNgayGross - chiTieuNgay);
                viewModel.BieuDoDoanhThu.Add(new BieuDoItem
                {
                    Label = period.Label,
                    Value = doanhThuNgay
                });
            }

            // 1.1. Dữ liệu biểu đồ thiệt hại - chỉ tính những thiệt hại chưa được đền bù
            foreach (var period in chartPeriods)
            {
                var thietHaiNgay = await _context.ThietHai
                    .Where(t => t.NgayXayRa.Date >= period.StartDate &&
                               t.NgayXayRa.Date <= period.EndDate &&
                               t.TrangThaiXuLy != "Đã đền bù")
                    .SumAsync(t => t.SoTienDenBu);

                viewModel.BieuDoThietHai.Add(new BieuDoItem
                {
                    Label = period.Label,
                    Value = thietHaiNgay
                });
            }

            // 2. Dữ liệu biểu đồ hợp đồng hoàn thành
            foreach (var period in chartPeriods)
            {
                var hopDongHoanThanhNgay = await _context.HopDong
                    .Where(h => h.TrangThai == "Hoàn thành" && 
                               h.NgayTraXeThucTe.HasValue &&
                               h.NgayTraXeThucTe.Value.Date >= period.StartDate &&
                               h.NgayTraXeThucTe.Value.Date <= period.EndDate)
                    .CountAsync();

                viewModel.BieuDoDonDat.Add(new BieuDoItem
                {
                    Label = period.Label,
                    Value = hopDongHoanThanhNgay
                });
            }

            // 3. Dữ liệu biểu đồ khách hàng mới
            foreach (var period in chartPeriods)
            {
                var khachHangMoiNgay = await _context.Users
                    .Where(u => u.NgayTao.Date >= period.StartDate &&
                               u.NgayTao.Date <= period.EndDate && 
                               u.VaiTro == "User")
                    .CountAsync();

                viewModel.BieuDoKhachHangMoi.Add(new BieuDoItem
                {
                    Label = period.Label,
                    Value = khachHangMoiNgay
                });
            }

            // 4. Top 5 xe được thuê nhiều nhất
            var topXe = await _context.ChiTietHopDong
                .Include(ct => ct.Xe)
                .Include(ct => ct.HopDong)
                .Where(ct => ct.HopDong.TrangThai == "Hoàn thành" && 
                            ct.HopDong.NgayTraXeThucTe.HasValue)
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

            // 5. Thống kê loại xe theo danh mục
            var thongKeLoaiXe = await _context.Xe
                .Include(x => x.LoaiXe)
                .GroupBy(x => x.LoaiXe.TenLoaiXe)
                .Select(g => new BieuDoItem
                {
                    Label = g.Key,
                    Value = g.Count()
                })
                .OrderByDescending(x => x.Value)
                .ToListAsync();

            viewModel.BieuDoLoaiXe = thongKeLoaiXe;

            // 5. Thống kê tổng quan
            var today = DateTime.Now.Date;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            // Doanh thu gốc theo filter được chọn (chỉ tính hợp đồng hoàn thành - chưa trừ gì cả)
            var doanhThuGross = await _context.HopDong
                .Where(h => h.TrangThai == "Hoàn thành" && 
                           h.NgayTraXeThucTe.HasValue && 
                           h.NgayTraXeThucTe.Value.Date >= chartPeriods.First().StartDate &&
                           h.NgayTraXeThucTe.Value.Date <= chartPeriods.Last().EndDate)
                .SumAsync(h => h.TongTien);

            var chiTieu = await _context.ChiTieu
                .Where(ct => ct.NgayChi.Date >= chartPeriods.First().StartDate &&
                            ct.NgayChi.Date <= chartPeriods.Last().EndDate)
                .SumAsync(ct => ct.SoTien);

            viewModel.DoanhThuGoc = doanhThuGross; // Lưu doanh thu gốc
            viewModel.TongChiTieu = chiTieu; // Lưu tổng chi tiêu
            viewModel.DoanhThuHomNay = Math.Max(0, doanhThuGross - chiTieu);

            // Tổng tiền thiệt hại theo filter được chọn - chỉ tính những thiệt hại chưa được đền bù
            viewModel.TongThietHai = await _context.ThietHai
                .Where(t => t.NgayXayRa.Date >= chartPeriods.First().StartDate &&
                           t.NgayXayRa.Date <= chartPeriods.Last().EndDate &&
                           t.TrangThaiXuLy != "Đã đền bù")
                .SumAsync(t => t.SoTienDenBu);

            // Tổng đơn đặt theo filter được chọn
            viewModel.TongDonDatXe = await _context.DatCho
                .Where(d => d.NgayDat.Date >= chartPeriods.First().StartDate && 
                           d.NgayDat.Date <= chartPeriods.Last().EndDate)
                .CountAsync();

            // Xe đang cho thuê
            viewModel.XeDangChoThue = await _context.Xe
                .Where(x => x.TrangThai == "Đang thuê")
                .CountAsync();

            // Hợp đồng hoạt động
            viewModel.HopDongHoatDong = await _context.HopDong
                .Where(h => h.TrangThai == "Đang thuê")
                .CountAsync();

            // Khách hàng mới theo filter được chọn
            viewModel.KhachHangMoi = await _context.Users
                .Where(u => u.NgayTao.Date >= chartPeriods.First().StartDate && 
                           u.NgayTao.Date <= chartPeriods.Last().EndDate && 
                           u.VaiTro == "User")
                .CountAsync();

            // Tổng số xe
            viewModel.TongSoXe = await _context.Xe.CountAsync();

            // 6. Thống kê hợp đồng theo trạng thái
            var thongKeHopDongTheoTrangThai = await _context.HopDong
                .Where(h => h.NgayTao.Date >= chartPeriods.First().StartDate && 
                           h.NgayTao.Date <= chartPeriods.Last().EndDate)
                .GroupBy(h => h.TrangThai)
                .Select(g => new BieuDoItem
                {
                    Label = g.Key,
                    Value = g.Count()
                })
                .OrderByDescending(x => x.Value)
                .ToListAsync();

            viewModel.BieuDoHopDongTrangThai = thongKeHopDongTheoTrangThai;

            // 7. 10 đơn đặt gần đây
            var donGanDay = await _context.DatCho
                .Include(d => d.Xe)
                .OrderByDescending(d => d.NgayDat)
                .Take(10)
                .Select(d => new DonDatGanDayItem
                {
                    TenKhach = d.HoTen,
                    TenXe = d.Xe.TenXe,
                    NgayDat = d.NgayDat,
                    NgayTra = d.NgayTraXe,
                    TrangThai = d.TrangThai,
                    TongTien = d.TongTienDuKien
                })
                .ToListAsync();

            viewModel.DonDatGanDay = donGanDay;

            return View(viewModel);
        }

        // GET: ThongKeBaoCao/BarChart - Hiển thị biểu đồ cột
        [PermissionAuthorize("CanViewBaoCao")]
        public async Task<IActionResult> BarChart(string chartFilter = "7days")
        {
            return View(new { ChartFilter = chartFilter ?? "7days" });
        }

        // GET: ThongKeBaoCao/GetChartData - Lấy dữ liệu charts theo filter
        [HttpGet]
        [PermissionAuthorize("CanViewBaoCao")]
        public async Task<IActionResult> GetChartData(string filter = "today")
        {
            try
            {
                var chartPeriods = GetChartPeriods(filter);
                
                // Dữ liệu doanh thu
                var doanhThuData = new List<decimal>();
                var chiTieuData = new List<decimal>();
                var thietHaiData = new List<decimal>();
                var donDatData = new List<int>();
                var khachHangMoiData = new List<int>();
                var labels = new List<string>();

                foreach (var period in chartPeriods)
                {
                    // Doanh thu
                    var doanhThuGross = await _context.HopDong
                        .Where(h => h.TrangThai == "Hoàn thành" && 
                                   h.NgayTraXeThucTe.HasValue && 
                                   h.NgayTraXeThucTe.Value.Date >= period.StartDate &&
                                   h.NgayTraXeThucTe.Value.Date <= period.EndDate)
                        .SumAsync(h => h.TongTien);

                    var chiTieu = await _context.ChiTieu
                        .Where(ct => ct.NgayChi.Date >= period.StartDate &&
                                    ct.NgayChi.Date <= period.EndDate)
                        .SumAsync(ct => ct.SoTien);

                    var doanhThu = Math.Max(0, doanhThuGross - chiTieu);

                    doanhThuData.Add(doanhThu);
                    chiTieuData.Add(chiTieu);

                    // Tổng tiền thiệt hại
                    var thietHai = await _context.ThietHai
                        .Where(t => t.NgayXayRa.Date >= period.StartDate &&
                                   t.NgayXayRa.Date <= period.EndDate)
                        .SumAsync(t => t.SoTienDenBu);

                    thietHaiData.Add(thietHai);

                    // Đơn đặt xe
                    var donDat = await _context.DatCho
                        .Where(d => d.NgayDat >= period.StartDate &&
                                   d.NgayDat <= period.EndDate)
                        .CountAsync();

                    // Khách hàng mới
                    var khachHangMoi = await _context.Users
                        .Where(u => u.NgayTao.Date >= period.StartDate &&
                                   u.NgayTao.Date <= period.EndDate && 
                                   u.VaiTro == "User")
                        .CountAsync();

                    donDatData.Add(donDat);
                    khachHangMoiData.Add(khachHangMoi);
                    labels.Add(period.Label);
                }

                // Top xe được thuê nhiều
                var topXe = await _context.ChiTietHopDong
                    .Include(ct => ct.Xe)
                    .Include(ct => ct.HopDong)
                    .Where(ct => ct.HopDong.TrangThai == "Hoàn thành" && 
                                ct.HopDong.NgayTraXeThucTe.HasValue)
                    .GroupBy(ct => new { ct.Xe.TenXe, ct.Xe.BienSoXe })
                    .Select(g => new 
                    {
                        TenXe = g.Key.TenXe + " (" + g.Key.BienSoXe + ")",
                        SoLanThue = g.Count()
                    })
                    .OrderByDescending(x => x.SoLanThue)
                    .Take(5)
                    .ToListAsync();

                // Thống kê loại xe theo danh mục
                var thongKeLoaiXe = await _context.Xe
                    .Include(x => x.LoaiXe)
                    .GroupBy(x => x.LoaiXe.TenLoaiXe)
                    .Select(g => new 
                    {
                        TenLoaiXe = g.Key,
                        SoLuong = g.Count()
                    })
                    .OrderByDescending(x => x.SoLuong)
                    .ToListAsync();

                // Thống kê hợp đồng theo trạng thái
                var thongKeHopDongTrangThai = await _context.HopDong
                    .Where(h => h.NgayTao.Date >= chartPeriods.First().StartDate && 
                               h.NgayTao.Date <= chartPeriods.Last().EndDate)
                    .GroupBy(h => h.TrangThai)
                    .Select(g => new 
                    {
                        TrangThai = g.Key,
                        SoLuong = g.Count()
                    })
                    .OrderByDescending(x => x.SoLuong)
                    .ToListAsync();

                return Json(new
                {
                    success = true,
                    doanhThu = new { labels, data = doanhThuData },
                    chiTieu = new { labels, data = chiTieuData },
                    thietHai = new { labels, data = thietHaiData },
                    donDat = new { labels, data = donDatData },
                    khachHangMoi = new { labels, data = khachHangMoiData },
                    topXe = new { 
                        labels = topXe.Select(x => x.TenXe).ToList(), 
                        data = topXe.Select(x => x.SoLanThue).ToList() 
                    },
                    loaiXe = new { 
                        labels = thongKeLoaiXe.Select(x => x.TenLoaiXe).ToList(), 
                        data = thongKeLoaiXe.Select(x => x.SoLuong).ToList() 
                    },
                    hopDongTrangThai = new { 
                        labels = thongKeHopDongTrangThai.Select(x => x.TrangThai).ToList(), 
                        data = thongKeHopDongTrangThai.Select(x => x.SoLuong).ToList() 
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: ThongKeBaoCao/GetBarChartData - Lấy dữ liệu biểu đồ cột (Doanh thu, Chi tiêu, Thiệt hại, Doanh thu thực tế)
        [HttpGet]
        [PermissionAuthorize("CanViewBaoCao")]
        public async Task<IActionResult> GetBarChartData(string filter = "7days", DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                List<ChartPeriod> chartPeriods;
                
                // Nếu có startDate và endDate thì tạo periods theo khoảng thời gian tùy chọn
                if (startDate.HasValue && endDate.HasValue)
                {
                    chartPeriods = GetCustomChartPeriods(startDate.Value, endDate.Value);
                }
                else
                {
                    chartPeriods = GetChartPeriods(filter);
                }
                
                var labels = new List<string>();
                var doanhThuGocData = new List<decimal>();
                var doanhThuData = new List<decimal>();
                var chiTieuData = new List<decimal>();
                var thietHaiData = new List<decimal>();
                var doanhThuThucTeData = new List<decimal>();

                foreach (var period in chartPeriods)
                {
                    labels.Add(period.Label);

                    // Doanh thu gốc (tổng tiền từ hợp đồng hoàn thành - không trừ gì cả)
                    var doanhThuGoc = await _context.HopDong
                        .Where(h => h.TrangThai == "Hoàn thành" && 
                                   h.NgayTraXeThucTe.HasValue && 
                                   h.NgayTraXeThucTe.Value.Date >= period.StartDate &&
                                   h.NgayTraXeThucTe.Value.Date <= period.EndDate)
                        .SumAsync(h => h.TongTien);

                    // Chi tiêu (từ bảng ChiTieu)
                    var chiTieu = await _context.ChiTieu
                        .Where(ct => ct.NgayChi.Date >= period.StartDate &&
                                    ct.NgayChi.Date <= period.EndDate)
                        .SumAsync(ct => ct.SoTien);

                    // Thiệt hại (từ bảng ThietHai) - chỉ tính những thiệt hại chưa được đền bù
                    var thietHai = await _context.ThietHai
                        .Where(t => t.NgayXayRa.Date >= period.StartDate &&
                                   t.NgayXayRa.Date <= period.EndDate &&
                                   t.TrangThaiXuLy != "Đã đền bù")
                        .SumAsync(t => t.SoTienDenBu);

                    // Doanh thu sau khi trừ chi tiêu (không trừ thiệt hại)
                    var doanhThu = doanhThuGoc - chiTieu;

                    // Doanh thu thực tế = Doanh thu gốc - Chi tiêu - Thiệt hại (chỉ những thiệt hại chưa đền bù)
                    var doanhThuThucTe = doanhThuGoc - chiTieu - thietHai;

                    doanhThuGocData.Add(doanhThuGoc);
                    doanhThuData.Add(doanhThu);
                    chiTieuData.Add(chiTieu);
                    thietHaiData.Add(thietHai);
                    doanhThuThucTeData.Add(doanhThuThucTe);
                }

                return Json(new
                {
                    success = true,
                    labels = labels,
                    datasets = new[]
                    {
                        new
                        {
                            label = "Doanh thu gốc",
                            data = doanhThuGocData,
                            backgroundColor = "rgba(54, 162, 235, 0.8)",
                            borderColor = "rgba(54, 162, 235, 1)",
                            borderWidth = 1
                        },
                        new
                        {
                            label = "Doanh thu sau chi tiêu",
                            data = doanhThuData,
                            backgroundColor = "rgba(100, 149, 237, 0.8)",
                            borderColor = "rgba(100, 149, 237, 1)",
                            borderWidth = 1
                        },
                        new
                        {
                            label = "Chi tiêu",
                            data = chiTieuData,
                            backgroundColor = "rgba(255, 99, 132, 0.8)",
                            borderColor = "rgba(255, 99, 132, 1)",
                            borderWidth = 1
                        },
                        new
                        {
                            label = "Thiệt hại",
                            data = thietHaiData,
                            backgroundColor = "rgba(255, 159, 64, 0.8)",
                            borderColor = "rgba(255, 159, 64, 1)",
                            borderWidth = 1
                        },
                        new
                        {
                            label = "Doanh thu thực tế",
                            data = doanhThuThucTeData,
                            backgroundColor = "rgba(75, 192, 192, 0.8)",
                            borderColor = "rgba(75, 192, 192, 1)",
                            borderWidth = 1
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: ThongKeBaoCao/GetStatistics - Lấy thống kê tổng quan theo filter
        [HttpGet]
        [PermissionAuthorize("CanViewBaoCao")]
        public async Task<IActionResult> GetStatistics(string filter = "today")
        {
            try
            {
                var chartPeriods = GetChartPeriods(filter);
                
                // Doanh thu gốc theo filter được chọn (chỉ tính hợp đồng hoàn thành - chưa trừ gì cả)
                var doanhThuGross = await _context.HopDong
                    .Where(h => h.TrangThai == "Hoàn thành" && 
                               h.NgayTraXeThucTe.HasValue && 
                               h.NgayTraXeThucTe.Value.Date >= chartPeriods.First().StartDate &&
                               h.NgayTraXeThucTe.Value.Date <= chartPeriods.Last().EndDate)
                    .SumAsync(h => h.TongTien);

                var chiTieu = await _context.ChiTieu
                    .Where(ct => ct.NgayChi.Date >= chartPeriods.First().StartDate &&
                                ct.NgayChi.Date <= chartPeriods.Last().EndDate)
                    .SumAsync(ct => ct.SoTien);

                var doanhThu = Math.Max(0, doanhThuGross - chiTieu);

                // Tổng tiền thiệt hại theo filter được chọn - chỉ tính những thiệt hại chưa được đền bù
                var tongThietHai = await _context.ThietHai
                    .Where(t => t.NgayXayRa.Date >= chartPeriods.First().StartDate &&
                               t.NgayXayRa.Date <= chartPeriods.Last().EndDate &&
                               t.TrangThaiXuLy != "Đã đền bù")
                    .SumAsync(t => t.SoTienDenBu);

                // Tổng đơn đặt xe theo filter được chọn
                var tongDonDat = await _context.DatCho
                    .Where(d => d.NgayDat >= chartPeriods.First().StartDate && 
                               d.NgayDat <= chartPeriods.Last().EndDate)
                    .CountAsync();

                // Khách hàng mới theo filter được chọn
                var khachHangMoi = await _context.Users
                    .Where(u => u.NgayTao.Date >= chartPeriods.First().StartDate && 
                               u.NgayTao.Date <= chartPeriods.Last().EndDate && 
                               u.VaiTro == "User")
                    .CountAsync();

                // Xe đang cho thuê (không thay đổi theo thời gian)
                var xeDangChoThue = await _context.Xe
                    .Where(x => x.TrangThai == "Đang thuê")
                    .CountAsync();

                // Hợp đồng hoạt động (không thay đổi theo thời gian)
                var hopDongHoatDong = await _context.HopDong
                    .Where(h => h.TrangThai == "Đang thuê")
                    .CountAsync();

                // Tổng số xe (không thay đổi theo thời gian)
                var tongSoXe = await _context.Xe.CountAsync();

                return Json(new
                {
                    success = true,
                    doanhThu = doanhThu,
                    doanhThuGoc = doanhThuGross,
                    tongChiTieu = chiTieu,
                    tongThietHai = tongThietHai,
                    tongDonDat = tongDonDat,
                    khachHangMoi = khachHangMoi,
                    xeDangChoThue = xeDangChoThue,
                    hopDongHoatDong = hopDongHoatDong,
                    tongSoXe = tongSoXe
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Helper method để lấy periods cho chart theo khoảng thời gian tùy chọn
        private List<ChartPeriod> GetCustomChartPeriods(DateTime startDate, DateTime endDate)
        {
            var periods = new List<ChartPeriod>();
            var currentDate = startDate.Date;
            var end = endDate.Date;
            
            // Nếu khoảng thời gian <= 31 ngày thì hiển thị theo ngày
            if ((end - currentDate).Days <= 31)
            {
                while (currentDate <= end)
                {
                    periods.Add(new ChartPeriod
                    {
                        StartDate = currentDate,
                        EndDate = currentDate,
                        Label = currentDate.ToString("dd/MM")
                    });
                    currentDate = currentDate.AddDays(1);
                }
            }
            // Nếu khoảng thời gian > 31 ngày và <= 365 ngày thì hiển thị theo tuần
            else if ((end - currentDate).Days <= 365)
            {
                var weekStart = currentDate.AddDays(-(int)currentDate.DayOfWeek);
                while (weekStart <= end)
                {
                    var weekEnd = weekStart.AddDays(6);
                    if (weekEnd > end) weekEnd = end;
                    
                    periods.Add(new ChartPeriod
                    {
                        StartDate = weekStart,
                        EndDate = weekEnd,
                        Label = weekStart.ToString("dd/MM") + " - " + weekEnd.ToString("dd/MM")
                    });
                    weekStart = weekStart.AddDays(7);
                }
            }
            // Nếu khoảng thời gian > 365 ngày thì hiển thị theo tháng
            else
            {
                var monthStart = new DateTime(currentDate.Year, currentDate.Month, 1);
                while (monthStart <= end)
                {
                    var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                    if (monthEnd > end) monthEnd = end;
                    
                    periods.Add(new ChartPeriod
                    {
                        StartDate = monthStart,
                        EndDate = monthEnd,
                        Label = monthStart.ToString("MM/yyyy")
                    });
                    monthStart = monthStart.AddMonths(1);
                }
            }
            
            return periods;
        }

        // Helper method để lấy periods cho chart theo filter
        private List<ChartPeriod> GetChartPeriods(string filter)
        {
            var periods = new List<ChartPeriod>();
            var now = DateTime.Now.Date;

            switch (filter?.ToLower())
            {
                case "today":
                    // Chỉ ngày hôm nay
                    periods.Add(new ChartPeriod
                    {
                        StartDate = now,
                        EndDate = now,
                        Label = now.ToString("dd/MM")
                    });
                    break;

                case "week":
                    // 7 ngày trong tuần này
                    var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
                    for (int i = 0; i < 7; i++)
                    {
                        var date = startOfWeek.AddDays(i);
                        periods.Add(new ChartPeriod
                        {
                            StartDate = date,
                            EndDate = date,
                            Label = date.ToString("dd/MM")
                        });
                    }
                    break;

                case "month":
                    // 30 ngày gần nhất
                    for (int i = 29; i >= 0; i--)
                    {
                        var date = now.AddDays(-i);
                        periods.Add(new ChartPeriod
                        {
                            StartDate = date,
                            EndDate = date,
                            Label = date.ToString("dd/MM")
                        });
                    }
                    break;

                case "year":
                    // 12 tháng gần nhất
                    for (int i = 11; i >= 0; i--)
                    {
                        var monthStart = new DateTime(now.Year, now.Month, 1).AddMonths(-i);
                        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                        periods.Add(new ChartPeriod
                        {
                            StartDate = monthStart,
                            EndDate = monthEnd,
                            Label = monthStart.ToString("MM/yyyy")
                        });
                    }
                    break;

                default: // "7days"
                    // 7 ngày gần nhất
                    for (int i = 6; i >= 0; i--)
                    {
                        var date = now.AddDays(-i);
                        periods.Add(new ChartPeriod
                        {
                            StartDate = date,
                            EndDate = date,
                            Label = date.ToString("dd/MM")
                        });
                    }
                    break;
            }

            return periods;
        }
    }

    // Helper class cho chart periods
    public class ChartPeriod
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Label { get; set; }
    }
} 