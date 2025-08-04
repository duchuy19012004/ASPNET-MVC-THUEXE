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
        [PermissionAuthorize("CanViewHopDong")]
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

        // AJAX: Lọc hợp đồng theo khoảng thời gian với phân trang
        [HttpGet]
        [PermissionAuthorize("CanViewHopDong")]
        public async Task<IActionResult> FilterByDateRange(DateTime? tuNgay, DateTime? denNgay, string trangThai = "", int page = 1, int pageSize = 10)
        {
            try
            {
                var query = _context.HopDong
                    .Include(h => h.ChiTietHopDong)
                    .ThenInclude(ct => ct.Xe)
                    .Include(h => h.DatCho)
                    .Include(h => h.HoaDon)
                    .AsQueryable();

                // Lọc theo trạng thái nếu có
                if (!string.IsNullOrEmpty(trangThai))
                {
                    query = query.Where(h => h.TrangThai == trangThai);
                }

                // Lọc theo khoảng thời gian nếu có
                if (tuNgay.HasValue && denNgay.HasValue)
                {
                    // Lọc theo ngày tạo hợp đồng
                    query = query.Where(h => h.NgayTao.Date >= tuNgay.Value.Date && h.NgayTao.Date <= denNgay.Value.Date);
                }
                else if (tuNgay.HasValue)
                {
                    query = query.Where(h => h.NgayTao.Date >= tuNgay.Value.Date);
                }
                else if (denNgay.HasValue)
                {
                    query = query.Where(h => h.NgayTao.Date <= denNgay.Value.Date);
                }

                // Đếm tổng số bản ghi
                int totalItems = await query.CountAsync();
                int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                // Phân trang
                var hopDongs = await query
                    .OrderByDescending(h => h.NgayTao)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Tạo HTML cho bảng
                var html = "";
                foreach (var item in hopDongs)
                {
                    html += "<tr data-phone=\"" + item.SoDienThoai + "\">";
                    html += "<td><strong>HD" + item.MaHopDong.ToString("D6") + "</strong></td>";
                    
                    html += "<td>";
                    html += item.HoTenKhach + "<br/>";
                    html += "<small class=\"text-muted phone-number\">" + item.SoDienThoai + "</small>";
                    html += "</td>";
                    
                    html += "<td>";
                    if (item.ChiTietHopDong?.Any() == true)
                    {
                        var xe = item.ChiTietHopDong.First();
                        html += xe.Xe?.TenXe + "<br/>";
                        html += "<small class=\"text-muted\">" + xe.Xe?.BienSoXe + "</small>";
                        
                        if (item.ChiTietHopDong.Count > 1)
                        {
                            html += "<small class=\"text-primary\">(+" + (item.ChiTietHopDong.Count - 1) + " xe khác)</small>";
                        }
                    }
                    else
                    {
                        html += "<span class=\"text-muted\">Chưa có xe</span>";
                    }
                    html += "</td>";
                    
                    html += "<td>" + item.NgayNhanXe.ToString("dd/MM/yyyy") + "</td>";
                    
                    html += "<td>";
                    if (item.NgayTraXeThucTe.HasValue)
                    {
                        html += item.NgayTraXeThucTe.Value.ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        html += item.NgayTraXeDuKien.ToString("dd/MM/yyyy");
                        html += "<small class=\"text-muted\">(dự kiến)</small>";
                    }
                    html += "</td>";
                    
                    html += "<td class=\"text-danger fw-bold\">" + item.TongTien.ToString("N0") + "đ</td>";
                    
                    string statusClass = item.TrangThai switch
                    {
                        "Đang thuê" => "status-active",
                        "Hoàn thành" => "status-completed",
                        "Hủy" => "status-cancelled",
                        _ => ""
                    };
                    html += "<td><span class=\"status-badge " + statusClass + "\">" + item.TrangThai + "</span></td>";
                    
                    // Cột hóa đơn
                    html += "<td>";
                    if (item.HoaDon != null)
                    {
                        html += "<a href=\"/QuanLyHoaDon/ChiTiet/" + item.HoaDon.MaHoaDon + "\" class=\"badge bg-success text-decoration-none\" title=\"Xem hóa đơn\">";
                        html += "<i class=\"bi bi-check-circle\"></i> HD" + item.HoaDon.MaHoaDon.ToString("D6");
                        html += "</a>";
                    }
                    else if (item.TrangThai == "Hoàn thành")
                    {
                        html += "<span class=\"badge bg-warning\">";
                        html += "<i class=\"bi bi-exclamation-triangle\"></i> Chưa có";
                        html += "</span>";
                    }
                    else
                    {
                        html += "<span class=\"badge bg-secondary\">-</span>";
                    }
                    html += "</td>";
                    
                    // Cột thao tác
                    html += "<td>";
                    html += "<a href=\"/QuanLyHopDong/ChiTiet/" + item.MaHopDong + "\" class=\"btn btn-sm btn-info\" title=\"Chi tiết\">";
                    html += "<i class=\"bi bi-eye\"></i>";
                    html += "</a>";
                    
                    if (item.TrangThai == "Đang thuê")
                    {
                        html += "<a href=\"/QuanLyHopDong/TraXe/" + item.MaHopDong + "\" class=\"btn btn-sm btn-success\" title=\"Trả xe\">";
                        html += "<i class=\"bi bi-check-circle\"></i>";
                        html += "</a>";
                    }
                    
                    if (item.TrangThai == "Hoàn thành" && item.HoaDon == null)
                    {
                        html += "<button class=\"btn btn-sm btn-warning\" onclick=\"taoHoaDonTuIndex(" + item.MaHopDong + ", '" + item.HoTenKhach + "')\" title=\"Tạo hóa đơn thủ công (trường hợp đặc biệt)\">";
                        html += "<i class=\"bi bi-receipt\"></i>";
                        html += "</button>";
                    }
                    html += "</td>";
                    
                    html += "</tr>";
                }

                // Tạo HTML cho phân trang
                var paginationHtml = GeneratePaginationHtml(page, totalPages, totalItems, pageSize);

                return Json(new { 
                    success = true, 
                    html = html, 
                    pagination = paginationHtml,
                    count = hopDongs.Count,
                    totalItems = totalItems,
                    totalPages = totalPages,
                    currentPage = page,
                    message = totalItems > 0 ? $"Tìm thấy {totalItems} hợp đồng (Trang {page}/{totalPages})" : "Không tìm thấy hợp đồng nào trong khoảng thời gian này"
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = "Có lỗi xảy ra: " + ex.Message 
                });
            }
        }

        // Helper method để tạo HTML phân trang
        private string GeneratePaginationHtml(int currentPage, int totalPages, int totalItems, int pageSize)
        {
            if (totalPages <= 1) return "";

            var html = "<nav aria-label=\"Phân trang hợp đồng\"><ul class=\"pagination justify-content-center mb-0\">";
            
            // Nút Previous
            if (currentPage > 1)
            {
                html += "<li class=\"page-item\"><a class=\"page-link\" href=\"#\" onclick=\"changePage(" + (currentPage - 1) + ")\">";
                html += "<i class=\"bi bi-chevron-left\"></i></a></li>";
            }
            else
            {
                html += "<li class=\"page-item disabled\"><span class=\"page-link\"><i class=\"bi bi-chevron-left\"></i></span></li>";
            }

            // Hiển thị các trang
            int startPage = Math.Max(1, currentPage - 2);
            int endPage = Math.Min(totalPages, currentPage + 2);

            if (startPage > 1)
            {
                html += "<li class=\"page-item\"><a class=\"page-link\" href=\"#\" onclick=\"changePage(1)\">1</a></li>";
                if (startPage > 2)
                {
                    html += "<li class=\"page-item disabled\"><span class=\"page-link\">...</span></li>";
                }
            }

            for (int i = startPage; i <= endPage; i++)
            {
                if (i == currentPage)
                {
                    html += "<li class=\"page-item active\"><span class=\"page-link\">" + i + "</span></li>";
                }
                else
                {
                    html += "<li class=\"page-item\"><a class=\"page-link\" href=\"#\" onclick=\"changePage(" + i + ")\">" + i + "</a></li>";
                }
            }

            if (endPage < totalPages)
            {
                if (endPage < totalPages - 1)
                {
                    html += "<li class=\"page-item disabled\"><span class=\"page-link\">...</span></li>";
                }
                html += "<li class=\"page-item\"><a class=\"page-link\" href=\"#\" onclick=\"changePage(" + totalPages + ")\">" + totalPages + "</a></li>";
            }

            // Nút Next
            if (currentPage < totalPages)
            {
                html += "<li class=\"page-item\"><a class=\"page-link\" href=\"#\" onclick=\"changePage(" + (currentPage + 1) + ")\">";
                html += "<i class=\"bi bi-chevron-right\"></i></a></li>";
            }
            else
            {
                html += "<li class=\"page-item disabled\"><span class=\"page-link\"><i class=\"bi bi-chevron-right\"></i></span></li>";
            }

            html += "</ul></nav>";

            // Thông tin phân trang
            int startItem = (currentPage - 1) * pageSize + 1;
            int endItem = Math.Min(currentPage * pageSize, totalItems);
            html += "<div class=\"text-center mt-2\"><small class=\"text-muted\">";
            html += $"Hiển thị {startItem}-{endItem} trong tổng số {totalItems} hợp đồng";
            html += "</small></div>";

            return html;
        }

        // GET: QuanLyHopDong/TimPhieuDatCho - Tìm phiếu đặt chỗ theo SĐT
        [HttpGet]
        [PermissionAuthorize("CanViewHopDong")]
        public IActionResult TimPhieuDatCho()
        {
            return View();
        }

        // POST: QuanLyHopDong/TimPhieuDatCho
        [HttpPost]
        [PermissionAuthorize("CanViewHopDong")]
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
        [PermissionAuthorize("CanCreateHopDong")]
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
        [PermissionAuthorize("CanCreateHopDong")]
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

        // GET: QuanLyHopDong/ChiTiet/5 - Xem chi tiết hợp đồng
        [PermissionAuthorize("CanViewHopDong")]
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
        [PermissionAuthorize("CanEditHopDong")]
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
        [PermissionAuthorize("CanEditHopDong")]
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

                        // Tự động tạo chi tiêu cho việc sửa chữa xe (nếu có chi phí ước tính)
                        if (chiPhi > 0)
                        {
                            var chiTieuSuaChua = new ChiTieu
                            {
                                NoiDung = $"Sửa chữa xe {ct.Xe.TenXe} ({ct.Xe.BienSoXe}) - {tinhTrang}",
                                SoTien = chiPhi,
                                NgayChi = ngayTraThucTe,
                                GhiChu = string.IsNullOrEmpty(moTa) ? $"Sửa chữa do {tinhTrang.ToLower()}" : moTa,
                                MaXe = ct.MaXe
                            };

                            _context.ChiTieu.Add(chiTieuSuaChua);
                        }
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

                // Tự động tạo hóa đơn sau khi hoàn tất trả xe
                var existingHoaDon = await _context.HoaDon
                    .FirstOrDefaultAsync(h => h.MaHopDong == hopDong.MaHopDong);
                
                string hoaDonInfo = "";
                if (existingHoaDon == null)
                {
                    var hoaDon = new HoaDon
                    {
                        MaHopDong = hopDong.MaHopDong,
                        NgayThanhToan = ngayTraThucTe,
                        SoTien = hopDong.TongTien,
                        TrangThai = "Đã thanh toán",
                        GhiChu = $"Hóa đơn được tạo tự động sau khi hoàn tất trả xe. {ghiChu}",
                        NgayTao = DateTime.Now,
                        MaNguoiTao = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier))
                    };

                    _context.HoaDon.Add(hoaDon);
                    await _context.SaveChangesAsync();
                    
                    hoaDonInfo = $" Mã hóa đơn: HD{hoaDon.MaHoaDon:D6}";
                }

                if (tongPhiDenBu > 0)
                {
                    // Tính tổng chi phí sửa chữa đã tạo chi tiêu
                    var tongChiPhiSuaChua = hopDong.ChiTietHopDong
                        .Where(ct => ct.TinhTrangTraXe != "Bình thường")
                        .Sum(ct => ct.Xe.ChiPhiSuaChua);

                    TempData["Success"] = $"Xử lý trả xe và tạo hóa đơn thành công! Phí đền bù: {tongPhiDenBu:N0}đ.{hoaDonInfo}";
                    
                    if (tongChiPhiSuaChua > 0)
                    {
                        TempData["Warning"] = $"Có thiệt hại xe được ghi nhận. Chi phí sửa chữa {tongChiPhiSuaChua:N0}đ đã được thêm vào danh sách chi tiêu.";
                    }
                    else
                    {
                        TempData["Warning"] = "Có thiệt hại xe được ghi nhận. Vui lòng kiểm tra báo cáo thiệt hại.";
                    }
                }
                else
                {
                    TempData["Success"] = $"Xử lý trả xe và tạo hóa đơn thành công!{hoaDonInfo}";
                }

                return RedirectToAction("ChiTiet", new { id = hopDong.MaHopDong });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                return View(hopDong);
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
        // GET: QuanLyHopDong/Create - Hiển thị form tạo hợp đồng mới
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            Console.WriteLine("GET Create action called");
            
            try
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

                Console.WriteLine("GET Create action completed successfully");
                Console.WriteLine($"ViewBag.XeList count: {ViewBag.XeList?.Count ?? 0}");
                return View(hopDong);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GET Create: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return View(new HopDong());
            }
        }

        // GET: QuanLyHopDong/DebugXe - Debug để kiểm tra dữ liệu xe
        [HttpGet]
        public async Task<IActionResult> DebugXe()
        {
            try
            {
                // Kiểm tra tổng số xe
                var totalXe = await _context.Xe.CountAsync();
                
                // Kiểm tra xe theo trạng thái
                var xeSanSang = await _context.Xe.Where(x => x.TrangThai == "Sẵn sàng").CountAsync();
                var xeDangThue = await _context.Xe.Where(x => x.TrangThai == "Đang thuê").CountAsync();
                var xeBaoTri = await _context.Xe.Where(x => x.TrangThai == "Bảo trì").CountAsync();
                var xeHuHong = await _context.Xe.Where(x => x.TrangThai == "Hư hỏng").CountAsync();
                var xeMat = await _context.Xe.Where(x => x.TrangThai == "Mất").CountAsync();
                var xeDaXoa = await _context.Xe.Where(x => x.TrangThai == "Đã xóa").CountAsync();
                
                // Lấy danh sách tất cả xe với trạng thái
                var allXe = await _context.Xe
                    .Select(x => new { x.MaXe, x.TenXe, x.BienSoXe, x.TrangThai, x.GiaThue })
                    .ToListAsync();
                
                ViewBag.TotalXe = totalXe;
                ViewBag.XeSanSang = xeSanSang;
                ViewBag.XeDangThue = xeDangThue;
                ViewBag.XeBaoTri = xeBaoTri;
                ViewBag.XeHuHong = xeHuHong;
                ViewBag.XeMat = xeMat;
                ViewBag.XeDaXoa = xeDaXoa;
                ViewBag.AllXe = allXe;
                
                return View();
            }
            catch (Exception ex)
            {
                return Content($"Error: {ex.Message}");
            }
        }
        // POST: QuanLyHopDong/Create - Xử lý tạo hợp đồng mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HopDong hopDong, List<int> danhSachXe)
        {
            try
            {
                // Debug: Log thông tin đầu vào
                Console.WriteLine($"Create action called with danhSachXe count: {danhSachXe?.Count ?? 0}");
                Console.WriteLine($"HoTenKhach: {hopDong.HoTenKhach}");
                Console.WriteLine($"SoDienThoai: {hopDong.SoDienThoai}");
                Console.WriteLine($"SoCCCD: {hopDong.SoCCCD}");
                Console.WriteLine($"NgayNhanXe: {hopDong.NgayNhanXe}");
                Console.WriteLine($"NgayTraXeDuKien: {hopDong.NgayTraXeDuKien}");
                Console.WriteLine($"TienCoc: {hopDong.TienCoc}");
                Console.WriteLine($"PhuPhi: {hopDong.PhuPhi}");
            
            // Debug: Log all form data
            Console.WriteLine("All form data:");
            foreach (var key in Request.Form.Keys)
            {
                Console.WriteLine($"{key}: {Request.Form[key]}");
            }
            // Debug: Log danhSachXe specifically
            if (danhSachXe != null)
            {
                Console.WriteLine("danhSachXe details:");
                for (int i = 0; i < danhSachXe.Count; i++)
                {
                    Console.WriteLine($"danhSachXe[{i}]: {danhSachXe[i]}");
                }
            }
            
            // Kiểm tra và sửa lỗi binding ngày tháng
            if (hopDong.NgayNhanXe == default(DateTime))
            {
                var ngayNhanStr = Request.Form["NgayNhanXe"].ToString();
                if (DateTime.TryParse(ngayNhanStr, out var ngayNhan))
                {
                    hopDong.NgayNhanXe = ngayNhan;
                    Console.WriteLine($"Fixed NgayNhanXe from form: {ngayNhanStr} -> {ngayNhan}");
                }
            }
            
            if (hopDong.NgayTraXeDuKien == default(DateTime))
            {
                var ngayTraStr = Request.Form["NgayTraXeDuKien"].ToString();
                if (DateTime.TryParse(ngayTraStr, out var ngayTra))
                {
                    hopDong.NgayTraXeDuKien = ngayTra;
                    Console.WriteLine($"Fixed NgayTraXeDuKien from form: {ngayTraStr} -> {ngayTra}");
                }
            }
            
            // Kiểm tra và sửa lỗi binding số tiền
            if (Request.Form.ContainsKey("TienCoc"))
            {
                var tienCocStr = Request.Form["TienCoc"].ToString();
                if (decimal.TryParse(tienCocStr, out var tienCoc))
                {
                    hopDong.TienCoc = tienCoc;
                    Console.WriteLine($"Fixed TienCoc from form: {tienCocStr} -> {tienCoc}");
                }
            }
            
            if (Request.Form.ContainsKey("PhuPhi"))
            {
                var phuPhiStr = Request.Form["PhuPhi"].ToString();
                if (decimal.TryParse(phuPhiStr, out var phuPhi))
                {
                    hopDong.PhuPhi = phuPhi;
                    Console.WriteLine($"Fixed PhuPhi from form: {phuPhiStr} -> {phuPhi}");
                }
            }
            
            // Kiểm tra có chọn xe không
            Console.WriteLine($"Checking danhSachXe: null={danhSachXe == null}, count={danhSachXe?.Count ?? 0}");
            if (danhSachXe == null || !danhSachXe.Any())
            {
                Console.WriteLine("No vehicles selected, adding error to ModelState");
                ModelState.AddModelError("DanhSachXe", "Vui lòng chọn ít nhất một xe!");
                
                // Check if this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { 
                        success = false, 
                        message = "Vui lòng chọn ít nhất một xe!"
                    });
                }
                
                await LoadXeListForView();
                return View(hopDong);
            }

            // Lấy thông tin tất cả xe được chọn
            Console.WriteLine($"Looking for vehicles with IDs: {string.Join(", ", danhSachXe)}");
            var cacXeThue = await _context.Xe
                .Where(x => danhSachXe.Contains(x.MaXe))
                .ToListAsync();
            Console.WriteLine($"Found {cacXeThue.Count} vehicles in database");

            // Kiểm tra từng xe
            var xeKhongKhaDung = cacXeThue.Where(x => x.TrangThai != "Sẵn sàng").ToList();
            Console.WriteLine($"Found {xeKhongKhaDung.Count} unavailable vehicles");
            if (xeKhongKhaDung.Any())
            {
                var errorMsg = $"Xe không khả dụng: {string.Join(", ", xeKhongKhaDung.Select(x => x.BienSoXe))}";
                Console.WriteLine($"Adding error: {errorMsg}");
                ModelState.AddModelError("DanhSachXe", errorMsg);
                
                // Check if this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { 
                        success = false, 
                        message = errorMsg
                    });
                }
            }

            // Kiểm tra xe có tồn tại không
            Console.WriteLine($"Vehicle count check: found={cacXeThue.Count}, requested={danhSachXe.Count}");
            if (cacXeThue.Count != danhSachXe.Count)
            {
                var errorMsg = "Một số xe không tồn tại trong hệ thống!";
                Console.WriteLine($"Adding error: {errorMsg}");
                ModelState.AddModelError("DanhSachXe", errorMsg);
                
                // Check if this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { 
                        success = false, 
                        message = errorMsg
                    });
                }
            }

            // Kiểm tra CCCD đã tồn tại trong hợp đồng đang thuê chưa
            Console.WriteLine($"Checking existing contract for CCCD: {hopDong.SoCCCD}");
            var existingContract = await _context.HopDong
                .AnyAsync(h => h.SoCCCD == hopDong.SoCCCD && h.TrangThai == "Đang thuê");
            Console.WriteLine($"Existing contract found: {existingContract}");
            if (existingContract)
            {
                var errorMsg = "Khách hàng này đang có hợp đồng thuê xe khác!";
                Console.WriteLine($"Adding error: {errorMsg}");
                ModelState.AddModelError("SoCCCD", errorMsg);
                
                // Check if this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { 
                        success = false, 
                        message = errorMsg
                    });
                }
            }

            // Debug: Log ModelState errors
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is invalid. Errors:");
                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key].Errors;
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"ModelState Error - {key}: {error.ErrorMessage}");
                    }
                }
            }
            
            if (ModelState.IsValid)
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

                    Console.WriteLine("Saving changes to database...");
                    await _context.SaveChangesAsync();
                    Console.WriteLine("Changes saved successfully");



                    var successMsg = $"Tạo hợp đồng thành công với {cacXeThue.Count} xe! Mã HĐ: HD{hopDong.MaHopDong:D6}";
                    Console.WriteLine($"Success: {successMsg}");
                    TempData["Success"] = successMsg;
                    
                    // Check if this is an AJAX request
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { 
                            success = true, 
                            message = successMsg,
                            redirectUrl = Url.Action("ChiTiet", new { id = hopDong.MaHopDong })
                        });
                    }
                    
                    return RedirectToAction("ChiTiet", new { id = hopDong.MaHopDong });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception occurred: {ex.Message}");
                    Console.WriteLine($"StackTrace: {ex.StackTrace}");
                    ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                }
            }

            // Nếu có lỗi, load lại danh sách xe
            Console.WriteLine("Returning to view due to validation errors or other issues");
            
            // Thêm thông tin debug vào TempData
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["DebugErrors"] = string.Join("; ", errors);
                Console.WriteLine($"Debug errors: {TempData["DebugErrors"]}");
            }
            
            // Check if this is an AJAX request
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { 
                    success = false, 
                    message = string.Join("; ", errors),
                    errors = errors
                });
            }
            
            await LoadXeListForView();
            return View(hopDong);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in Create action: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Check if this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { 
                        success = false, 
                        message = "Có lỗi xảy ra: " + ex.Message
                    });
                }
                
                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                await LoadXeListForView();
                return View(hopDong);
            }
        }

        // Helper method để load danh sách xe
        private async Task LoadXeListForView()
        {
            try
            {
                Console.WriteLine("Loading available vehicles from database...");
                
                // Kiểm tra tổng số xe trong database
                var totalXe = await _context.Xe.CountAsync();
                Console.WriteLine($"Total vehicles in database: {totalXe}");
                
                // Kiểm tra số xe có trạng thái khác "Đã xóa"
                var availableXe = await _context.Xe.Where(x => x.TrangThai != "Đã xóa").CountAsync();
                Console.WriteLine($"Vehicles not deleted: {availableXe}");
                
                var xeList = await _context.Xe
                    .Include(x => x.LoaiXe)
                    .Include(x => x.HinhAnhXes)
                    .Where(x => x.TrangThai != "Đã xóa") // Load tất cả xe chưa bị xóa
                    .Select(x => new
                    {
                        maXe = x.MaXe,
                        tenXe = x.TenXe,
                        bienSoXe = x.BienSoXe,
                        hangXe = x.HangXe,
                        dongXe = x.DongXe,
                        giaThue = x.GiaThue,
                        trangThai = x.TrangThai,
                        hinhAnh = x.HinhAnhHienThi,
                        loaiXe = x.LoaiXe != null ? x.LoaiXe.TenLoaiXe : "Chưa phân loại",
                        display = x.TenXe + " - " + x.BienSoXe + " (" + (x.LoaiXe != null ? x.LoaiXe.TenLoaiXe : "Chưa phân loại") + ") - " + x.GiaThue.ToString("N0") + "đ/ngày - " + x.TrangThai
                    })
                    .ToListAsync();
                
                ViewBag.XeList = xeList;
                Console.WriteLine($"Loaded {xeList.Count} available vehicles");
                
                // Debug: In ra một số xe đầu tiên
                if (xeList.Count > 0)
                {
                    Console.WriteLine("First few vehicles:");
                    for (int i = 0; i < Math.Min(3, xeList.Count); i++)
                    {
                        Console.WriteLine($"  {i + 1}. {xeList[i].tenXe} - {xeList[i].bienSoXe} - {xeList[i].trangThai}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading vehicle list: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                ViewBag.XeList = new List<object>();
            }
        }

        // Helper method để lưu file upload


        // AJAX: Lấy thông tin xe
        [HttpGet]
        public async Task<IActionResult> GetXeInfo(int maXe)
        {
            var xe = await _context.Xe
                .Include(x => x.HinhAnhXes)
                .Where(x => x.MaXe == maXe)
                .Select(x => new
                {
                    maXe = x.MaXe,
                    tenXe = x.TenXe,
                    bienSoXe = x.BienSoXe,
                    hangXe = x.HangXe,
                    dongXe = x.DongXe,
                    giaThue = x.GiaThue,
                    trangThai = x.TrangThai,
                    hinhAnh = x.HinhAnhHienThi
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

        // GET: QuanLyHopDong/InHopDongA4/5 - In hợp đồng A4
        [HttpGet]
        public async Task<IActionResult> InHopDongA4(int? id)
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
    }
}