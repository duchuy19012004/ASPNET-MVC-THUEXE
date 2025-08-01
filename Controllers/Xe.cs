using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bike.Models;
using bike.Attributes;
using bike.Repository;

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
        public async Task<IActionResult> Index(string searchString, int? loaiXe, string hangXe, string trangThai)
        {
            // Lấy danh sách xe với filtering
            var query = _context.Xe
                .Include(x => x.LoaiXe)
                .Include(x => x.ChiTieu)
                .Include(x => x.HinhAnhXes)
                .AsQueryable();

            // Tìm kiếm theo tên xe hoặc biển số
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(x => x.TenXe.Contains(searchString) || x.BienSoXe.Contains(searchString));
            }

            // Lọc theo loại xe
            if (loaiXe.HasValue)
            {
                query = query.Where(x => x.MaLoaiXe == loaiXe.Value);
            }

            // Lọc theo hãng xe
            if (!string.IsNullOrEmpty(hangXe))
            {
                query = query.Where(x => x.HangXe == hangXe);
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(trangThai))
            {
                query = query.Where(x => x.TrangThai == trangThai);
            }

            var xeList = await query.ToListAsync();

            // Set ViewBag cho thống kê
            ViewBag.TongSoXe = await _context.Xe.CountAsync();
            ViewBag.XeSanSang = await _context.Xe.CountAsync(x => x.TrangThai == "Sẵn sàng");
            ViewBag.DangChoThue = await _context.Xe.CountAsync(x => x.TrangThai == "Đang thuê");
            ViewBag.BaoTri = await _context.Xe.CountAsync(x => x.TrangThai == "Bảo trì");

            // Set ViewBag cho dropdown filters
            ViewBag.LoaiXeList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.LoaiXe, "MaLoaiXe", "TenLoaiXe");
            
            // Tạo danh sách hãng xe từ dữ liệu hiện có
            var hangXeList = await _context.Xe
                .Where(x => !string.IsNullOrEmpty(x.HangXe))
                .Select(x => x.HangXe)
                .Distinct()
                .ToListAsync();
            ViewBag.HangXeList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(hangXeList);
            
            ViewBag.TrangThaiList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(new[] { "Sẵn sàng", "Đang thuê", "Bảo trì" });

            return View(xeList);
        }

        // GET: Xe/Create
        [PermissionAuthorize("CanCreateXe")]
        public IActionResult Create()
        {
            ViewBag.MaLoaiXe = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.LoaiXe, "MaLoaiXe", "TenLoaiXe");
            ViewBag.TrangThaiList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(new[] { "Sẵn sàng", "Đang thuê", "Bảo trì", "Hư hỏng", "Mất" });
            return View();
        }

        // POST: Xe/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("CanCreateXe")]
        public async Task<IActionResult> Create([Bind("BienSoXe,TenXe,MaLoaiXe,GiaThue,TrangThai")] Xe xe)
        {
            if (ModelState.IsValid)
            {
                _context.Add(xe);
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
        public async Task<IActionResult> Edit(int id, [Bind("MaXe,BienSoXe,TenXe,MaLoaiXe,GiaThue,TrangThai")] Xe xe)
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
                _context.Xe.Remove(xe);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool XeExists(int id)
        {
            return _context.Xe.Any(e => e.MaXe == id);
        }
    }
}