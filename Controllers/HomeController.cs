using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bike.Repository;
using bike.Models;
using bike.ViewModel;
using System.Diagnostics;
using System.Collections.Generic;

namespace bike.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly BikeDbContext _context;

        public HomeController(ILogger<HomeController> logger, BikeDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: Home/Index
        public async Task<IActionResult> Index(int? loaiXe)
        {
            // Tạo ViewModel
            var viewModel = new XeMayLoaiXe();

            // Lấy danh sách loại xe
            viewModel.DanhSachLoaiXe = await _context.LoaiXe
                .OrderBy(l => l.TenLoaiXe)
                .ToListAsync();

            // Lấy danh sách xe với filtering - ẩn xe bị mất
            var queryXe = _context.Xe
                .Include(x => x.LoaiXe)
                .Include(x => x.HinhAnhXes)
                .Where(x => x.TrangThai != "Mất") // Ẩn xe bị mất khỏi danh sách
                .AsQueryable();
            
            // Nếu có filter theo loại xe
            if (loaiXe.HasValue)
            {
                queryXe = queryXe.Where(x => x.MaLoaiXe == loaiXe.Value);
            }

            viewModel.DanhSachXeMay = await queryXe
                .OrderByDescending(x => x.MaXe) // Hoặc order theo số lần thuê nếu có
                .Take(8) // Lấy 8 xe đầu
                .ToListAsync();

            // Lấy danh sách banner hiển thị
            ViewBag.Banners = await _context.Banner
                .Where(b => b.TrangThai) // Chỉ lấy banner đang hiển thị
                .OrderBy(b => b.ThuTu) // Sắp xếp theo thứ tự
                .Take(3) // Tối đa 3 banner
                .ToListAsync();

            // Truyền dữ liệu cho navbar dropdown (cho layout)
            ViewBag.DanhSachLoaiXe = viewModel.DanhSachLoaiXe;
            
            // Lấy tất cả xe theo từng loại để hiển thị trong dropdown
            ViewBag.XeTheoLoai = new Dictionary<int, List<Xe>>();
            foreach (var loai in viewModel.DanhSachLoaiXe)
            {
                var xeTheoLoai = await _context.Xe
                    .Include(x => x.LoaiXe)
                    .Include(x => x.HinhAnhXes)
                    .Where(x => x.MaLoaiXe == loai.MaLoaiXe && x.TrangThai == "Sẵn sàng")
                    .Take(5) // Chỉ lấy 5 xe đầu để không quá dài
                    .ToListAsync();
                ViewBag.XeTheoLoai.Add(loai.MaLoaiXe, xeTheoLoai);
            }

            ViewBag.LoaiXeSelected = loaiXe;

            return View(viewModel);
        }
        // GET: Home/XemChiTiet/5
        public async Task<IActionResult> XemChiTiet(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Lấy thông tin xe kèm loại xe và hình ảnh
            var xe = await _context.Xe
                .Include(x => x.LoaiXe)
                .Include(x => x.HinhAnhXes.OrderBy(h => h.ThuTu))
                .FirstOrDefaultAsync(x => x.MaXe == id);

            if (xe == null)
            {
                return NotFound();
            }

            // Lấy xe liên quan (cùng loại)
            ViewBag.XeLienQuan = await _context.Xe
                .Include(x => x.LoaiXe)
                .Include(x => x.HinhAnhXes)
                .Where(x => x.MaLoaiXe == xe.MaLoaiXe && x.MaXe != xe.MaXe)
                .Take(4)
                .ToListAsync();

            // Truyền dữ liệu cho navbar dropdown (giống như Index)
            var danhSachLoaiXe = await _context.LoaiXe
                .OrderBy(l => l.TenLoaiXe)
                .ToListAsync();
            ViewBag.DanhSachLoaiXe = danhSachLoaiXe;
            
            // Lấy tất cả xe theo từng loại để hiển thị trong dropdown
            ViewBag.XeTheoLoai = new Dictionary<int, List<Xe>>();
            foreach (var loai in danhSachLoaiXe)
            {
                var xeTheoLoai = await _context.Xe
                    .Include(x => x.LoaiXe)
                    .Include(x => x.HinhAnhXes)
                    .Where(x => x.MaLoaiXe == loai.MaLoaiXe && x.TrangThai == "Sẵn sàng")
                    .Take(5) // Chỉ lấy 5 xe đầu để không quá dài
                    .ToListAsync();
                ViewBag.XeTheoLoai.Add(loai.MaLoaiXe, xeTheoLoai);
            }

            return View(xe);
        }
    }
}