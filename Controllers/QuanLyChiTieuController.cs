using bike.Models; // Thêm dòng này để sử dụng Model ChiTieu
using bike.Repository; // Thêm dòng này để sử dụng BikeDbContext
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore; // Thêm dòng này để dùng các hàm như ToListAsync()

namespace bike.Controllers
{
    public class QuanLyChiTieuController : Controller
    {
        private readonly BikeDbContext _context; // Biến để chứa DbContext

        // Constructor để inject DbContext
        public QuanLyChiTieuController(BikeDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            DateTime homNay = DateTime.Today;

            try
            {
                // Tính tổng chi tiêu trong ngày hôm nay - sử dụng async
                decimal tongChiHomNay = await _context.ChiTieu
                    .Where(c => c.NgayChi.Date == homNay)
                    .SumAsync(c => c.SoTien);

                // Tính tổng chi tiêu trong tháng hiện tại - sử dụng async
                decimal tongChiThangNay = await _context.ChiTieu
                    .Where(c => c.NgayChi.Year == homNay.Year && c.NgayChi.Month == homNay.Month)
                    .SumAsync(c => c.SoTien);

                // Đưa các giá trị đã tính toán vào ViewData để View có thể sử dụng
                ViewData["TongChiHomNay"] = tongChiHomNay;
                ViewData["TongChiThangNay"] = tongChiThangNay;

                // --- PHẦN LẤY DANH SÁCH CHI TIÊU - thêm OrderBy để tối ưu ---
                var danhSachChiTieu = await _context.ChiTieu
                    .Include(c => c.Xe)
                    .OrderByDescending(c => c.NgayChi)
                    .Take(100) // Giới hạn 100 bản ghi gần nhất để tránh load quá nhiều
                    .ToListAsync();

                return View(danhSachChiTieu);
            }
            catch (Exception ex)
            {
                // Log lỗi và trả về view với dữ liệu rỗng
                ViewData["TongChiHomNay"] = 0;
                ViewData["TongChiThangNay"] = 0;
                ViewData["ErrorMessage"] = "Có lỗi xảy ra khi tải dữ liệu: " + ex.Message;
                return View(new List<ChiTieu>());
            }
        }
        // Action này có nhiệm vụ hiển thị ra form để người dùng nhập liệu
        public IActionResult Create()
        {
            // Tạo một SelectList chứa danh sách các xe để hiển thị trong dropdown
            // "MaXe" là giá trị của option, "TenXe" là text hiển thị
            ViewData["MaXe"] = new SelectList(_context.Xe, "MaXe", "BienSoXe");
            return View();
        }

        // Action này sẽ được gọi khi người dùng nhấn nút "Lưu" trên form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NoiDung,SoTien,NgayChi,GhiChu")] ChiTieu chiTieu)
        {
            // Đặt giá trị Id mặc định vì nó là identity, database sẽ tự tăng
            // và loại bỏ nó khỏi ModelState để không bị lỗi validation
            ModelState.Remove("Id");

            if (ModelState.IsValid)
            {
                _context.Add(chiTieu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); // Chuyển hướng về trang danh sách
            }
            // Nếu dữ liệu không hợp lệ, hiển thị lại form để người dùng sửa lỗi
            return View(chiTieu);
        }
        public async Task<IActionResult> ChiTiet(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chiTieu = await _context.ChiTieu
                .FirstOrDefaultAsync(m => m.Id == id);
            if (chiTieu == null)
            {
                return NotFound();
            }

            return View(chiTieu);
        }

        // Action để trả về dữ liệu chi tiết dạng JSON cho modal
        public async Task<IActionResult> GetChiTiet(int? id)
        {
            if (id == null)
            {
                return Json(new { success = false, message = "ID không hợp lệ" });
            }

            var chiTieu = await _context.ChiTieu
                .Include(c => c.Xe)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (chiTieu == null)
            {
                return Json(new { success = false, message = "Không tìm thấy khoản chi" });
            }

            var result = new
            {
                success = true,
                data = new
                {
                    id = chiTieu.Id,
                    noiDung = chiTieu.NoiDung,
                    soTien = chiTieu.SoTien,
                    ngayChi = chiTieu.NgayChi.ToString("dd/MM/yyyy"),
                    ghiChu = chiTieu.GhiChu ?? "Không có ghi chú",
                    xeLienKet = chiTieu.Xe?.BienSoXe ?? "Không liên kết xe"
                }
            };

            return Json(result);
        }

        // Action để trả về dữ liệu xóa dạng JSON cho modal
        public async Task<IActionResult> GetDeleteData(int? id)
        {
            if (id == null)
            {
                return Json(new { success = false, message = "ID không hợp lệ" });
            }

            var chiTieu = await _context.ChiTieu
                .Include(c => c.Xe)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (chiTieu == null)
            {
                return Json(new { success = false, message = "Không tìm thấy khoản chi" });
            }

            var result = new
            {
                success = true,
                data = new
                {
                    id = chiTieu.Id,
                    noiDung = chiTieu.NoiDung,
                    soTien = chiTieu.SoTien,
                    ngayChi = chiTieu.NgayChi.ToString("dd/MM/yyyy"),
                    xeLienKet = chiTieu.Xe?.BienSoXe ?? "Không liên kết xe"
                }
            };

            return Json(result);
        }

        // Action để lấy dữ liệu gốc (không ảnh hưởng đến thống kê)
        [HttpGet]
        public async Task<IActionResult> GetOriginalData()
        {
            try
            {
                // Lấy danh sách chi tiêu gốc
                var danhSachChiTieu = await _context.ChiTieu
                    .Include(c => c.Xe)
                    .OrderByDescending(c => c.NgayChi)
                    .Take(100) // Giới hạn 100 bản ghi gần nhất
                    .ToListAsync();

                // Tạo HTML cho các dòng trong bảng
                var rows = danhSachChiTieu.Select(item => 
                {
                    var xeBadge = item.Xe != null ? $"<span class=\"badge bg-secondary\">{item.Xe.BienSoXe}</span>" : "";
                    return $@"
                    <tr>
                        <td>{item.NoiDung}</td>
                        <td class=""text-danger fw-bold"">{item.SoTien.ToString("N0")}đ</td>
                        <td>{item.NgayChi.ToString("dd/MM/yyyy")}</td>
                        <td>{xeBadge}</td>
                        <td>{item.GhiChu ?? ""}</td>
                        <td class=""text-center"">
                            <a href=""/QuanLyChiTieu/Edit/{item.Id}"" class=""btn btn-warning btn-sm"" title=""Sửa"">
                                <i class=""bi bi-pencil-square""></i>
                            </a>
                            <button type=""button"" class=""btn btn-info btn-sm"" title=""Chi tiết"" onclick=""showChiTietModal({item.Id})"">
                                <i class=""bi bi-eye""></i>
                            </button>
                            <button type=""button"" class=""btn btn-danger btn-sm"" title=""Xóa"" onclick=""showDeleteModal({item.Id})"">
                                <i class=""bi bi-trash""></i>
                            </button>
                        </td>
                    </tr>";
                }).ToArray();

                var result = new
                {
                    rows = rows,
                    soLuong = danhSachChiTieu.Count
                };

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải dữ liệu: " + ex.Message });
            }
        }

        // Action để tìm kiếm theo nội dung chi tiêu
        [HttpPost]
        public async Task<IActionResult> SearchByContent(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return Json(new { success = false, message = "Vui lòng nhập từ khóa tìm kiếm" });
            }

            try
            {
                // Tìm kiếm theo nội dung chi tiêu (không phân biệt hoa thường)
                var searchResults = await _context.ChiTieu
                    .Include(c => c.Xe)
                    .Where(c => c.NoiDung.Contains(searchTerm))
                    .OrderByDescending(c => c.NgayChi)
                    .ToListAsync();

                // Tính tổng tiền của kết quả tìm kiếm
                decimal tongTien = searchResults.Sum(c => c.SoTien);

                // Tạo HTML cho các dòng trong bảng
                var rows = searchResults.Select(item => 
                {
                    var xeBadge = item.Xe != null ? $"<span class=\"badge bg-secondary\">{item.Xe.BienSoXe}</span>" : "";
                    return $@"
                    <tr>
                        <td>{item.NoiDung}</td>
                        <td class=""text-danger fw-bold"">{item.SoTien.ToString("N0")}đ</td>
                        <td>{item.NgayChi.ToString("dd/MM/yyyy")}</td>
                        <td>{xeBadge}</td>
                        <td>{item.GhiChu ?? ""}</td>
                        <td class=""text-center"">
                            <a href=""/QuanLyChiTieu/Edit/{item.Id}"" class=""btn btn-warning btn-sm"" title=""Sửa"">
                                <i class=""bi bi-pencil-square""></i>
                            </a>
                            <button type=""button"" class=""btn btn-info btn-sm"" title=""Chi tiết"" onclick=""showChiTietModal({item.Id})"">
                                <i class=""bi bi-eye""></i>
                            </button>
                            <button type=""button"" class=""btn btn-danger btn-sm"" title=""Xóa"" onclick=""showDeleteModal({item.Id})"">
                                <i class=""bi bi-trash""></i>
                            </button>
                        </td>
                    </tr>";
                }).ToArray();

                var result = new
                {
                    rows = rows,
                    tongChi = tongTien,
                    soLuong = searchResults.Count
                };

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi tìm kiếm: " + ex.Message });
            }
        }

        // Action để lọc dữ liệu theo ngày bằng AJAX
        public async Task<IActionResult> FilterByDate(string startDate, string endDate)
        {
            try
            {
                DateTime? start = null;
                DateTime? end = null;

                // Parse start date
                if (!string.IsNullOrEmpty(startDate))
                {
                    if (DateTime.TryParse(startDate, out DateTime parsedStart))
                    {
                        start = parsedStart.Date;
                    }
                }

                // Parse end date
                if (!string.IsNullOrEmpty(endDate))
                {
                    if (DateTime.TryParse(endDate, out DateTime parsedEnd))
                    {
                        end = parsedEnd.Date.AddDays(1).AddSeconds(-1); // End of day
                    }
                }
                // Build query
                var query = _context.ChiTieu.Include(c => c.Xe).AsQueryable();

                if (start.HasValue)
                {
                    query = query.Where(c => c.NgayChi >= start.Value);
                }

                if (end.HasValue)
                {
                    query = query.Where(c => c.NgayChi <= end.Value);
                }

                // Get filtered data
                var filteredData = await query
                    .OrderByDescending(c => c.NgayChi)
                    .Take(100)
                    .ToListAsync();

                // Calculate statistics for filtered data
                decimal tongChiFiltered = filteredData.Sum(c => c.SoTien);
                int soLuongFiltered = filteredData.Count;

                // Prepare HTML for table rows
                var tableRows = new List<string>();
                foreach (var item in filteredData)
                {
                    var xeInfo = item.Xe != null ? $"<span class=\"badge bg-secondary\">{item.Xe.BienSoXe}</span>" : "";
                    var ghiChu = !string.IsNullOrEmpty(item.GhiChu) ? item.GhiChu : "";
                    
                    var row = $@"
                        <tr>
                            <td>{item.NoiDung}</td>
                            <td class=""text-danger fw-bold"">{item.SoTien.ToString("N0")}đ</td>
                            <td>{item.NgayChi.ToString("dd/MM/yyyy")}</td>
                            <td>{xeInfo}</td>
                            <td>{ghiChu}</td>
                            <td class=""text-center"">
                                <a href=""/QuanLyChiTieu/Edit/{item.Id}"" class=""btn btn-warning btn-sm"" title=""Sửa"">
                                    <i class=""bi bi-pencil-square""></i>
                                </a>
                                <button type=""button"" class=""btn btn-info btn-sm"" title=""Chi tiết"" onclick=""showChiTietModal({item.Id})"">
                                    <i class=""bi bi-eye""></i>
                                </button>
                                <button type=""button"" class=""btn btn-danger btn-sm"" title=""Xóa"" onclick=""showDeleteModal({item.Id})"">
                                    <i class=""bi bi-trash""></i>
                                </button>
                            </td>
                        </tr>";
                    tableRows.Add(row);
                }

                var result = new
                {
                    success = true,
                    data = new
                    {
                        rows = tableRows,
                        tongChi = tongChiFiltered,
                        soLuong = soLuongFiltered,
                        startDate = start?.ToString("yyyy-MM-dd"),
                        endDate = end?.ToString("yyyy-MM-dd")
                    }
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chiTieu = await _context.ChiTieu.FindAsync(id);
            if (chiTieu == null)
            {
                return NotFound();
            }
            // Tương tự Create, nhưng thêm tham số thứ 4 để chọn sẵn xe đã được liên kết trước đó
            ViewData["MaXe"] = new SelectList(_context.Xe, "MaXe", "BienSoXe", chiTieu.MaXe);
            return View(chiTieu);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NoiDung,SoTien,NgayChi,GhiChu,MaXe")] ChiTieu chiTieu)
        {
            if (id != chiTieu.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chiTieu);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChiTieuExists(chiTieu.Id))
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
            return View(chiTieu);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chiTieu = await _context.ChiTieu
                .FirstOrDefaultAsync(m => m.Id == id);
            if (chiTieu == null)
            {
                return NotFound();
            }

            return View(chiTieu);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var chiTieu = await _context.ChiTieu.FindAsync(id);
            if (chiTieu != null)
            {
                _context.ChiTieu.Remove(chiTieu);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ChiTieuExists(int id)
        {
            return _context.ChiTieu.Any(e => e.Id == id);
        }
    }
}