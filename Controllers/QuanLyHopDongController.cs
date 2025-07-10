using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bike.Repository;
using bike.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using bike.Attributes;
using bike.Services;

namespace bike.Controllers
{
    [CustomAuthorize("Admin", "Staff")]
    public class QuanLyHopDongController : Controller
    {
        private readonly BikeDbContext _context;
        private readonly IDamageCompensationService _damageService;

        public QuanLyHopDongController(BikeDbContext context, IDamageCompensationService damageService)
        {
            _context = context;
            _damageService = damageService;
        }

        // GET: QuanLyHopDong - Danh sách hợp đồng
        public async Task<IActionResult> Index(string trangThai = "", int page = 1, int pageSize = 10)
        {
            var query = _context.HopDong
                .Include(h => h.ChiTietHopDong)
                .ThenInclude(ct => ct.Xe)
                .Include(h => h.DatCho)
                .Include(h => h.HoaDon) 
                .AsQueryable();

            if (!string.IsNullOrEmpty(trangThai))
            {
                query = query.Where(h => h.TrangThai == trangThai);
            }

            int totalItems = await query.CountAsync();
            var hopDongs = await query
                .OrderByDescending(h => h.NgayTao)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Đếm tổng số hợp đồng theo trạng thái trên toàn bộ bảng
            int tongDangThue = await _context.HopDong.CountAsync(h => h.TrangThai == "Đang thuê");
            int tongHoanThanh = await _context.HopDong.CountAsync(h => h.TrangThai == "Hoàn thành");
            
            // Đếm đơn chờ xử lý (phiếu đặt chỗ)
            int donChoXuLy = await _context.DatCho
                .CountAsync(d => d.TrangThai == "Chờ xác nhận" || d.TrangThai == "Đang giữ chỗ");

            // Đếm đơn chờ xử lý mới trong ngày hôm nay
            var homNay = DateTime.Today;
            int donChoXuLyMoi = await _context.DatCho
                .CountAsync(d => (d.TrangThai == "Chờ xác nhận" || d.TrangThai == "Đang giữ chỗ") 
                               && d.NgayDat.Date == homNay);

            ViewBag.TrangThai = trangThai;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TongDangThue = tongDangThue;
            ViewBag.TongHoanThanh = tongHoanThanh;
            ViewBag.DonChoXuLy = donChoXuLy;
            ViewBag.DonChoXuLyMoi = donChoXuLyMoi;

            return View(hopDongs);
        }

        // GET: QuanLyHopDong/TimPhieuDatCho - Tìm phiếu đặt chỗ theo SĐT
        [HttpGet]
        public IActionResult TimPhieuDatCho()
        {
            return View();
        }

        // POST: QuanLyHopDong/TimPhieuDatCho
        [HttpPost]
        public async Task<IActionResult> TimPhieuDatCho(string soDienThoai)
        {
            if (string.IsNullOrEmpty(soDienThoai))
            {
                ModelState.AddModelError("", "Vui lòng nhập số điện thoại");
                return View();
            }

            // Tìm các phiếu đặt chỗ theo SĐT
            var phieuDatCho = await _context.DatCho
                .Include(d => d.Xe)
                .Where(d => d.SoDienThoai == soDienThoai &&
                           (d.TrangThai == "Chờ xác nhận" || d.TrangThai == "Đang giữ chỗ"))
                .OrderByDescending(d => d.NgayDat)
                .ToListAsync();

            ViewBag.SoDienThoai = soDienThoai;
            ViewBag.DanhSachPhieu = phieuDatCho;

            return View();
        }

        // GET: QuanLyHopDong/TaoHopDong/5 - Tạo hợp đồng từ phiếu đặt chỗ
        [HttpGet]
        public async Task<IActionResult> TaoHopDong(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Lấy phiếu đặt chỗ
            var datCho = await _context.DatCho
                .Include(d => d.Xe)
                .FirstOrDefaultAsync(d => d.MaDatCho == id);

            if (datCho == null)
            {
                return NotFound();
            }

            // Kiểm tra xe còn sẵn sàng không
            if (datCho.Xe.TrangThai != "Sẵn sàng" && datCho.Xe.TrangThai != "Đang giữ chỗ")
            {
                TempData["Error"] = "Xe này hiện không khả dụng!";
                return RedirectToAction("TimPhieuDatCho");
            }

            // Tạo hợp đồng từ phiếu đặt chỗ
            var hopDong = new HopDong
            {
                MaDatCho = datCho.MaDatCho,
                MaKhachHang = datCho.MaUser,
                HoTenKhach = datCho.HoTen,
                SoDienThoai = datCho.SoDienThoai,
                NgayNhanXe = datCho.NgayNhanXe,
                NgayTraXeDuKien = datCho.NgayTraXe,
                TienCoc = 0, 
                PhuPhi = 0,
                GhiChu = datCho.GhiChu
            };

                    // Gán thông tin xe cho hợp đồng
        hopDong.ChiTietHopDong = new List<ChiTietHopDong>
        {
            new ChiTietHopDong
            {
                MaXe = datCho.MaXe,
                GiaThueNgay = datCho.Xe.GiaThue,
                NgayNhanXe = datCho.NgayNhanXe,
                NgayTraXeDuKien = datCho.NgayTraXe,
                SoNgayThue = (datCho.NgayTraXe - datCho.NgayNhanXe).Days + 1,
                ThanhTien = datCho.Xe.GiaThue * ((datCho.NgayTraXe - datCho.NgayNhanXe).Days + 1),
                TrangThaiXe = "Đang thuê"
            }
        };

        // Tính tổng tiền dự kiến (chỉ tiền thuê, chưa có cọc)
        hopDong.TongTien = hopDong.ChiTietHopDong.Sum(ct => ct.ThanhTien);

            return View(hopDong);
        }

        // POST: QuanLyHopDong/TaoHopDong
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoHopDong(HopDong hopDong)
        {
            if (!ModelState.IsValid)
            {
                // Load lại thông tin nếu có lỗi
                if (hopDong.MaDatCho.HasValue)
                {
                    var datCho = await _context.DatCho
                        .Include(d => d.Xe)
                        .FirstOrDefaultAsync(d => d.MaDatCho == hopDong.MaDatCho);
                    
                    if (datCho != null && hopDong.ChiTietHopDong == null)
                    {
                        hopDong.ChiTietHopDong = new List<ChiTietHopDong>
                        {
                            new ChiTietHopDong
                            {
                                MaXe = datCho.MaXe,
                                GiaThueNgay = datCho.Xe.GiaThue,
                                NgayNhanXe = datCho.NgayNhanXe,
                                NgayTraXeDuKien = datCho.NgayTraXe,
                                SoNgayThue = (datCho.NgayTraXe - datCho.NgayNhanXe).Days + 1
                            }
                        };
                    }
                }
                return View(hopDong);
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Lưu người tạo
                    hopDong.MaNguoiTao = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    hopDong.NgayTao = DateTime.Now;
                    hopDong.TrangThai = "Đang thuê";

                    // TÁI TẠO ChiTietHopDong từ thông tin DatCho
                    if (hopDong.MaDatCho.HasValue)
                    {
                        var datCho = await _context.DatCho
                            .Include(d => d.Xe)
                            .FirstOrDefaultAsync(d => d.MaDatCho == hopDong.MaDatCho.Value);

                        if (datCho == null)
                        {
                            TempData["Error"] = "Không tìm thấy phiếu đặt chỗ!";
                            return View(hopDong);
                        }

                        // Tính toán chi tiết hợp đồng từ DatCho
                        var soNgay = (hopDong.NgayTraXeDuKien - hopDong.NgayNhanXe).Days + 1;
                        var tienThueXe = datCho.Xe.GiaThue * soNgay;
                        
                        hopDong.TongTien = tienThueXe + hopDong.PhuPhi;

                        // Lưu hợp đồng trước để có MaHopDong
                        _context.HopDong.Add(hopDong);
                        await _context.SaveChangesAsync();

                        // Tạo và lưu chi tiết hợp đồng
                        var chiTietHopDong = new ChiTietHopDong
                        {
                            MaHopDong = hopDong.MaHopDong,
                            MaXe = datCho.MaXe,
                            GiaThueNgay = datCho.Xe.GiaThue,
                            NgayNhanXe = hopDong.NgayNhanXe,
                            NgayTraXeDuKien = hopDong.NgayTraXeDuKien,
                            SoNgayThue = soNgay,
                            ThanhTien = tienThueXe,
                            TrangThaiXe = "Đang thuê",
                            NgayTao = DateTime.Now
                        };
                        
                        _context.ChiTietHopDong.Add(chiTietHopDong);
                        await _context.SaveChangesAsync();

                        // Cập nhật trạng thái xe
                        datCho.Xe.TrangThai = "Đang thuê";
                        
                        // Cập nhật trạng thái phiếu đặt chỗ
                        datCho.TrangThai = "Đã xử lý";
                        
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        TempData["Success"] = $"Tạo hợp đồng thành công! Mã HĐ: HD{hopDong.MaHopDong:D6}";
                        return RedirectToAction("ChiTiet", new { id = hopDong.MaHopDong });
                    }
                    else
                    {
                        TempData["Error"] = "Thiếu thông tin phiếu đặt chỗ!";
                        return View(hopDong);
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = $"Có lỗi xảy ra: {ex.Message}";
                    
                    // Load lại thông tin nếu có lỗi
                    if (hopDong.MaDatCho.HasValue)
                    {
                        var datCho = await _context.DatCho
                            .Include(d => d.Xe)
                            .FirstOrDefaultAsync(d => d.MaDatCho == hopDong.MaDatCho);
                        
                        if (datCho != null && hopDong.ChiTietHopDong == null)
                        {
                            hopDong.ChiTietHopDong = new List<ChiTietHopDong>
                            {
                                new ChiTietHopDong
                                {
                                    MaXe = datCho.MaXe,
                                    GiaThueNgay = datCho.Xe.GiaThue,
                                    NgayNhanXe = datCho.NgayNhanXe,
                                    NgayTraXeDuKien = datCho.NgayTraXe,
                                    SoNgayThue = (datCho.NgayTraXe - datCho.NgayNhanXe).Days + 1
                                }
                            };
                        }
                    }
                    return View(hopDong);
                }
            }
        }

        // GET: QuanLyHopDong/ChiTiet/5 - Xem chi tiết hợp đồng
        public async Task<IActionResult> ChiTiet(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hopDong = await _context.HopDong
                .Include(h => h.ChiTietHopDong)
                .ThenInclude(ct => ct.Xe)
                .Include(h => h.DatCho)
                .Include(h => h.HoaDon) 
                .Include(h => h.NguoiTao)
                .FirstOrDefaultAsync(h => h.MaHopDong == id);

            if (hopDong == null)
            {
                return NotFound();
            }

            return View(hopDong);
        }

        // GET: QuanLyHopDong/TraXe/5 - Form trả xe
        [HttpGet]
        public async Task<IActionResult> TraXe(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hopDong = await _context.HopDong
                .Include(h => h.ChiTietHopDong)
                .ThenInclude(ct => ct.Xe)
                .FirstOrDefaultAsync(h => h.MaHopDong == id && h.TrangThai == "Đang thuê");

            if (hopDong == null)
            {
                return NotFound();
            }

            // Set ngày trả mặc định là hôm nay
            hopDong.NgayTraXeThucTe = DateTime.Now;

            return View(hopDong);
        }

        // POST: QuanLyHopDong/TraXe
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TraXe(int id, DateTime ngayTraThucTe, decimal phuPhi, string ghiChu, 
            List<string> tinhTrangTraXe, List<string> moTaThietHai, List<decimal> chiPhiSuaChua)
        {
            var hopDong = await _context.HopDong
                .Include(h => h.ChiTietHopDong)
                .ThenInclude(ct => ct.Xe)
                .FirstOrDefaultAsync(h => h.MaHopDong == id);

            if (hopDong == null)
            {
                return NotFound();
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Cập nhật hợp đồng
                    hopDong.NgayTraXeThucTe = ngayTraThucTe;
                    hopDong.PhuPhi += phuPhi;
                    hopDong.GhiChu += ghiChu;
                    hopDong.TrangThai = "Hoàn thành";

                    decimal tongPhiDenBu = 0;

                    // Cập nhật chi tiết từng xe với xử lý thiệt hại
                    for (int i = 0; i < hopDong.ChiTietHopDong.Count; i++)
                    {
                        var ct = hopDong.ChiTietHopDong.ElementAt(i);
                        
                        // Cập nhật thông tin cơ bản
                        ct.NgayTraXeThucTe = ngayTraThucTe;
                        var soNgayThucTe = (ngayTraThucTe - ct.NgayNhanXe).Days + 1;
                        ct.SoNgayThue = soNgayThucTe;
                        ct.ThanhTien = ct.GiaThueNgay * soNgayThucTe;

                        // Xử lý thiệt hại nếu có
                        string tinhTrang = i < tinhTrangTraXe.Count ? tinhTrangTraXe[i] : "Bình thường";
                        string moTa = i < moTaThietHai.Count ? moTaThietHai[i] : "";
                        decimal chiPhi = i < chiPhiSuaChua.Count ? chiPhiSuaChua[i] : 0;

                        // Cập nhật thông tin thiệt hại trong ChiTietHopDong
                        ct.TinhTrangTraXe = tinhTrang;
                        ct.MoTaThietHai = moTa;

                        // Tính phí đền bù bằng service
                        if (tinhTrang != "Bình thường")
                        {
                            var compensationResult = _damageService.ProcessDamageReport(
                                tinhTrang, ct.Xe.GiaTriXe, chiPhi, soNgayThucTe);
                            
                            ct.PhiDenBu = compensationResult.CompensationAmount;
                            tongPhiDenBu += ct.PhiDenBu;

                            // Cập nhật trạng thái xe theo kết quả xử lý
                            ct.Xe.TrangThai = compensationResult.NewBikeStatus;
                            ct.TrangThaiXe = compensationResult.NewBikeStatus;

                            // Cập nhật thông tin thiệt hại vào xe
                            ct.Xe.NgayGapSuCo = ngayTraThucTe;
                            ct.Xe.MoTaThietHai = moTa;
                            ct.Xe.ChiPhiSuaChua = chiPhi;

                            // Tạo báo cáo thiệt hại chi tiết
                            var baoCaoThietHai = new BaoCaoThietHai
                            {
                                MaChiTiet = ct.MaChiTiet,
                                LoaiThietHai = tinhTrang,
                                MoTaChiTiet = moTa,
                                NgayPhatHien = ngayTraThucTe,
                                ChiPhiSuaChuaUocTinh = chiPhi,
                                PhiDenBuKhachHang = ct.PhiDenBu,
                                GiaTriXeTruocKhiHong = ct.Xe.GiaTriXe,
                                GiaTriXeSauKhiHong = compensationResult.NewBikeStatus == "Mất" ? 0 : 
                                    ct.Xe.GiaTriXe - compensationResult.CompensationAmount,
                                TrangThaiXuLy = compensationResult.NewBikeStatus == "Mất" ? "Không thể sửa" : "Chờ xử lý",
                                MaNguoiTao = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                                NgayTao = DateTime.Now,
                                NgayCapNhat = DateTime.Now
                            };

                            _context.BaoCaoThietHai.Add(baoCaoThietHai);
                        }
                        else
                        {
                            // Xe bình thường - trạng thái sẵn sàng
                            ct.Xe.TrangThai = "Sẵn sàng";
                            ct.TrangThaiXe = "Đã trả";
                            ct.PhiDenBu = 0;
                        }
                    }

                    // Cộng phí đền bù vào tổng phí phụ
                    hopDong.PhuPhi += tongPhiDenBu;

                    // Tính lại tổng tiền
                    hopDong.TongTien = hopDong.ChiTietHopDong.Sum(ct => ct.ThanhTien) + hopDong.PhuPhi;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    if (tongPhiDenBu > 0)
                    {
                        TempData["Success"] = $"Xử lý trả xe thành công! Phí đền bù: {tongPhiDenBu:N0}đ";
                        TempData["Warning"] = "Có thiệt hại xe được ghi nhận. Vui lòng kiểm tra báo cáo thiệt hại.";
                    }
                    else
                    {
                        TempData["Success"] = "Xử lý trả xe thành công!";
                    }

                    return RedirectToAction("ChiTiet", new { id = hopDong.MaHopDong });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                    return View(hopDong);
                }
            }
        }

        // GET: QuanLyHopDong/LichSuKhachHang - Xem lịch sử thuê xe của khách
        public async Task<IActionResult> LichSuKhachHang(string soDienThoai)
        {
            if (string.IsNullOrEmpty(soDienThoai))
            {
                return View();
            }

            var lichSu = await _context.HopDong
                .Include(h => h.ChiTietHopDong)
                .ThenInclude(ct => ct.Xe)
                .Where(h => h.SoDienThoai == soDienThoai)
                .OrderByDescending(h => h.NgayTao)
                .ToListAsync();

            ViewBag.SoDienThoai = soDienThoai;
            return View(lichSu);
        }
        // GET: QuanLyHopDong/Create - Form tạo hợp đồng mới
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Lấy danh sách xe sẵn sàng
            await LoadXeListForView();

            // Tạo model mới với giá trị mặc định
            var hopDong = new HopDong
            {
                NgayNhanXe = DateTime.Today,
                NgayTraXeDuKien = DateTime.Today.AddDays(3),
                NgayTao = DateTime.Now,
                TrangThai = "Đang thuê"
            };

            return View(hopDong);
        }

        // POST: QuanLyHopDong/Create - Xử lý tạo hợp đồng mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HopDong hopDong, List<int> danhSachXe)
        {
            // Kiểm tra có chọn xe không
            if (danhSachXe == null || !danhSachXe.Any())
            {
                ModelState.AddModelError("DanhSachXe", "Vui lòng chọn ít nhất một xe!");
                await LoadXeListForView();
                return View(hopDong);
            }

            // Lấy thông tin tất cả xe được chọn
            var cacXeThue = await _context.Xe
                .Where(x => danhSachXe.Contains(x.MaXe))
                .ToListAsync();

            // Kiểm tra từng xe
            var xeKhongKhaDung = cacXeThue.Where(x => x.TrangThai != "Sẵn sàng").ToList();
            if (xeKhongKhaDung.Any())
            {
                ModelState.AddModelError("DanhSachXe", 
                    $"Xe không khả dụng: {string.Join(", ", xeKhongKhaDung.Select(x => x.BienSoXe))}");
            }

            // Kiểm tra xe có tồn tại không
            if (cacXeThue.Count != danhSachXe.Count)
            {
                ModelState.AddModelError("DanhSachXe", "Một số xe không tồn tại trong hệ thống!");
            }

            // Kiểm tra CCCD đã tồn tại trong hợp đồng đang thuê chưa
            var existingContract = await _context.HopDong
                .AnyAsync(h => h.SoCCCD == hopDong.SoCCCD && h.TrangThai == "Đang thuê");
            if (existingContract)
            {
                ModelState.AddModelError("SoCCCD", "Khách hàng này đang có hợp đồng thuê xe khác!");
            }

            if (ModelState.IsValid)
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Set thông tin tự động
                        hopDong.MaNguoiTao = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                        hopDong.NgayTao = DateTime.Now;
                        hopDong.TrangThai = "Đang thuê";

                        // Tính tổng tiền từ tất cả xe
                        var soNgay = (hopDong.NgayTraXeDuKien - hopDong.NgayNhanXe).Days + 1;
                        var tongTienThueXe = cacXeThue.Sum(xe => xe.GiaThue * soNgay);
                        hopDong.TongTien = tongTienThueXe + hopDong.PhuPhi;

                        // Lưu hợp đồng trước để có MaHopDong
                        _context.HopDong.Add(hopDong);
                        await _context.SaveChangesAsync();

                        // Tạo chi tiết hợp đồng cho từng xe
                        foreach (var xe in cacXeThue)
                        {
                            var tienThueXeNay = xe.GiaThue * soNgay;
                            
                            var chiTietHopDong = new ChiTietHopDong
                            {
                                MaHopDong = hopDong.MaHopDong,
                                MaXe = xe.MaXe,
                                GiaThueNgay = xe.GiaThue,
                                NgayNhanXe = hopDong.NgayNhanXe,
                                NgayTraXeDuKien = hopDong.NgayTraXeDuKien,
                                SoNgayThue = soNgay,
                                ThanhTien = tienThueXeNay,
                                TrangThaiXe = "Đang thuê",
                                NgayTao = DateTime.Now
                            };

                            _context.ChiTietHopDong.Add(chiTietHopDong);

                            // Cập nhật trạng thái xe
                            xe.TrangThai = "Đang thuê";
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        TempData["Success"] = $"Tạo hợp đồng thành công với {cacXeThue.Count} xe! Mã HĐ: HD{hopDong.MaHopDong:D6}";
                        return RedirectToAction("ChiTiet", new { id = hopDong.MaHopDong });
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                    }
                }
            }

            // Nếu có lỗi, load lại danh sách xe
            await LoadXeListForView();
            return View(hopDong);
        }

        // Helper method để load danh sách xe
        private async Task LoadXeListForView()
        {
            ViewBag.XeList = await _context.Xe
                .Include(x => x.LoaiXe)
                .Where(x => x.TrangThai == "Sẵn sàng")
                .Select(x => new
                {
                    x.MaXe,
                    x.TenXe,
                    x.BienSoXe,
                    x.HangXe,
                    x.DongXe,
                    x.GiaThue,
                    LoaiXe = x.LoaiXe != null ? x.LoaiXe.TenLoaiXe : "Chưa phân loại",
                    Display = x.TenXe + " - " + x.BienSoXe + " (" + (x.LoaiXe != null ? x.LoaiXe.TenLoaiXe : "Chưa phân loại") + ") - " + x.GiaThue.ToString("N0") + "đ/ngày"
                })
                .ToListAsync();
        }

        // AJAX: Lấy thông tin xe
        [HttpGet]
        public async Task<IActionResult> GetXeInfo(int maXe)
        {
            var xe = await _context.Xe
                .Where(x => x.MaXe == maXe)
                .Select(x => new
                {
                    x.MaXe,
                    x.TenXe,
                    x.BienSoXe,
                    x.HangXe,
                    x.DongXe,
                    x.GiaThue,
                    x.TrangThai
                })
                .FirstOrDefaultAsync();

            if (xe == null)
            {
                return Json(new { success = false, message = "Không tìm thấy xe" });
            }

            return Json(new { success = true, data = xe });
        }
        // GET: QuanLyHopDong/DonChoXuLy - Danh sách đơn chờ xử lý
        public async Task<IActionResult> DonChoXuLy(string searchString, DateTime? tuNgay, DateTime? denNgay)
        {
            // Query cơ bản - chỉ lấy đơn "Chờ xác nhận"
            var query = _context.DatCho
                .Include(d => d.Xe)
                .Include(d => d.User)
                .Where(d => d.TrangThai == "Chờ xác nhận")
                .AsQueryable();

            // Tìm kiếm theo tên hoặc SĐT
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(d =>
                    d.HoTen.Contains(searchString) ||
                    d.SoDienThoai.Contains(searchString) ||
                    d.Email.Contains(searchString));
            }

            // Lọc theo ngày
            if (tuNgay.HasValue)
            {
                query = query.Where(d => d.NgayDat >= tuNgay.Value);
            }
            if (denNgay.HasValue)
            {
                query = query.Where(d => d.NgayDat <= denNgay.Value.AddDays(1));
            }

            // Sắp xếp theo ngày đặt mới nhất
            var donChoXuLy = await query.OrderByDescending(d => d.NgayDat).ToListAsync();

            // ViewBag cho filter
            ViewBag.SearchString = searchString;
            ViewBag.TuNgay = tuNgay;
            ViewBag.DenNgay = denNgay;
            ViewBag.TongDonCho = donChoXuLy.Count;

            return View(donChoXuLy);
        }

        // POST: QuanLyHopDong/XuLyDon - Xác nhận và tạo hợp đồng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XuLyDon(int id)
        {
            // Tìm đơn đặt chỗ
            var datCho = await _context.DatCho
                .Include(d => d.Xe)
                .FirstOrDefaultAsync(d => d.MaDatCho == id);

            if (datCho == null)
            {
                TempData["Error"] = "Không tìm thấy đơn đặt chỗ!";
                return RedirectToAction(nameof(DonChoXuLy));
            }

            // Kiểm tra trạng thái đơn
            if (datCho.TrangThai != "Chờ xác nhận")
            {
                TempData["Error"] = "Đơn này đã được xử lý!";
                return RedirectToAction(nameof(DonChoXuLy));
            }

            // Kiểm tra xe còn sẵn sàng không
            if (datCho.Xe.TrangThai != "Sẵn sàng")
            {
                TempData["Error"] = "Xe này hiện không khả dụng!";
                return RedirectToAction(nameof(DonChoXuLy));
            }

            // Chuyển hướng đến tạo hợp đồng với thông tin từ đơn đặt chỗ
            return RedirectToAction(nameof(TaoHopDong), new { id = datCho.MaDatCho });
        }

        // POST: QuanLyHopDong/HuyDon - Hủy đơn với lý do
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyDon(int id, string lyDoHuy)
        {
            var datCho = await _context.DatCho.FindAsync(id);

            if (datCho == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn!" });
            }

            if (datCho.TrangThai != "Chờ xác nhận")
            {
                return Json(new { success = false, message = "Đơn này đã được xử lý!" });
            }

            try
            {
                // Lưu lý do hủy vào ghi chú
                datCho.GhiChu = $"[HỦY - {DateTime.Now:dd/MM/yyyy HH:mm}] Lý do: {lyDoHuy}\n{datCho.GhiChu}";

                // Xóa đơn
                _context.DatCho.Remove(datCho);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã hủy đơn thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // GET: QuanLyHopDong/ChiTietDonCho/5 - Xem chi tiết đơn đặt chỗ (AJAX)
        [HttpGet]
        public async Task<IActionResult> ChiTietDonCho(int id)
        {
            try
            {
                var datCho = await _context.DatCho
                    .Include(d => d.Xe)
                    .Include(d => d.User)
                    .FirstOrDefaultAsync(d => d.MaDatCho == id);

                if (datCho == null)
                {
                    return NotFound("Không tìm thấy đơn đặt chỗ!");
                }

                return PartialView("_ChiTietDonCho", datCho);
            }
            catch (Exception ex)
            {
                return Content($"Lỗi: {ex.Message}");
            }
        }
    }
}