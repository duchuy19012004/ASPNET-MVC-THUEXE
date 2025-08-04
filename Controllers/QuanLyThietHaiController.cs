using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using bike.Models;
using bike.Repository;
using bike.Attributes;
using System.Security.Claims;

namespace bike.Controllers
{
    [PermissionAuthorize("CanViewThietHai")]
    public class QuanLyThietHaiController : Controller
    {
        private readonly BikeDbContext _context;

        public QuanLyThietHaiController(BikeDbContext context)
        {
            _context = context;
        }

        // GET: QuanLyThietHai
        public async Task<IActionResult> Index()
        {
            try
            {
                var thietHaiList = await _context.ThietHai
                    .Include(t => t.Xe)
                    .Include(t => t.HopDong)
                        .ThenInclude(h => h.KhachHang)
                    .Include(t => t.NguoiBaoCao)
                    .OrderByDescending(t => t.NgayTao)
                    .ToListAsync();

                // Tính toán thống kê với null checks
                var tongThietHai = thietHaiList?.Count ?? 0;
                var thietHaiChuaXuLy = thietHaiList?.Where(t => t != null && t.TrangThaiXuLy == "Chưa xử lý").Count() ?? 0;
                var thietHaiDangXuLy = thietHaiList?.Where(t => t != null && t.TrangThaiXuLy == "Đang xử lý").Count() ?? 0;
                var thietHaiDaXuLy = thietHaiList?.Where(t => t != null && (t.TrangThaiXuLy == "Đã xử lý" || t.TrangThaiXuLy == "Đã đền bù")).Count() ?? 0;
                var tongDenBu = thietHaiList?.Where(t => t != null).Sum(t => t.SoTienDenBu) ?? 0;

                ViewData["TongThietHai"] = tongThietHai;
                ViewData["ThietHaiChuaXuLy"] = thietHaiChuaXuLy;
                ViewData["ThietHaiDangXuLy"] = thietHaiDangXuLy;
                ViewData["ThietHaiDaXuLy"] = thietHaiDaXuLy;
                ViewData["TongDenBu"] = tongDenBu;

                return View(thietHaiList ?? new List<ThietHai>());
            }
            catch (Exception ex)
            {
                // Log lỗi và trả về view với danh sách rỗng
                ViewData["TongThietHai"] = 0;
                ViewData["ThietHaiChuaXuLy"] = 0;
                ViewData["ThietHaiDangXuLy"] = 0;
                ViewData["ThietHaiDaXuLy"] = 0;
                ViewData["TongDenBu"] = 0;
                
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải dữ liệu: " + ex.Message;
                return View(new List<ThietHai>());
            }
        }

        // GET: QuanLyThietHai/Details/5 - Redirect to Index since we use modal now
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Redirect to Index since we use modal for details now
            TempData["InfoMessage"] = "Vui lòng sử dụng modal để xem chi tiết thiệt hại.";
            return RedirectToAction(nameof(Index));
        }

        // GET: QuanLyThietHai/Edit/5
        [PermissionAuthorize("CanEditThietHai")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var thietHai = await _context.ThietHai
                .Include(t => t.Xe)
                .Include(t => t.HopDong)
                    .ThenInclude(h => h.KhachHang)
                .Include(t => t.NguoiBaoCao)
                .FirstOrDefaultAsync(t => t.MaThietHai == id);
                
            if (thietHai == null)
            {
                return NotFound();
            }

            await PopulateViewData();
            return View(thietHai);
        }

        // POST: QuanLyThietHai/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("CanEditThietHai")]
        public async Task<IActionResult> Edit(int id, [Bind("MaThietHai,LoaiThietHai,MoTaThietHai,SoTienDenBu,TrangThaiXuLy,PhuongAnXuLy,GhiChu")] ThietHai thietHai)
        {
            if (id != thietHai.MaThietHai)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy thông tin thiệt hại hiện tại
                    var existingThietHai = await _context.ThietHai
                        .Include(t => t.HopDong)
                        .FirstOrDefaultAsync(t => t.MaThietHai == id);

                    if (existingThietHai == null)
                    {
                        return NotFound();
                    }

                    // Chỉ cập nhật các trường được phép sửa
                    existingThietHai.LoaiThietHai = thietHai.LoaiThietHai;
                    existingThietHai.MoTaThietHai = thietHai.MoTaThietHai;
                    existingThietHai.SoTienDenBu = thietHai.SoTienDenBu;
                    existingThietHai.TrangThaiXuLy = thietHai.TrangThaiXuLy;
                    existingThietHai.PhuongAnXuLy = thietHai.PhuongAnXuLy;
                    existingThietHai.GhiChu = thietHai.GhiChu;
                    existingThietHai.NgayCapNhat = DateTime.Now;

                    // Nếu trạng thái là "Đã xử lý" hoặc "Đã đền bù" và chưa có ngày hoàn thành
                    if ((existingThietHai.TrangThaiXuLy == "Đã xử lý" || existingThietHai.TrangThaiXuLy == "Đã đền bù") && !existingThietHai.NgayHoanThanh.HasValue)
                    {
                        existingThietHai.NgayHoanThanh = DateTime.Now;
                    }

                    _context.Update(existingThietHai);
                    await _context.SaveChangesAsync();

                    // Check if it's an AJAX request
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, message = "Thông tin thiệt hại đã được cập nhật thành công!" });
                    }

                    TempData["SuccessMessage"] = "Thông tin thiệt hại đã được cập nhật thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ThietHaiExists(thietHai.MaThietHai))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Check if it's an AJAX request
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join(", ", errors) });
            }

            await PopulateViewData();
            return View(thietHai);
        }
        // GET: QuanLyThietHai/Delete/5 - Redirect to Index since we use modal now
        [PermissionAuthorize("CanDeleteThietHai")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Redirect to Index since we use modal for delete now
            TempData["InfoMessage"] = "Vui lòng sử dụng modal để xóa thiệt hại.";
            return RedirectToAction(nameof(Index));
        }
        // POST: QuanLyThietHai/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("CanDeleteThietHai")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Check if this is an AJAX request
            bool isAjaxRequest = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            try
            {
                var thietHai = await _context.ThietHai.FindAsync(id);
                if (thietHai == null)
                {
                    if (isAjaxRequest)
                    {
                        return Json(new { success = false, message = "Không tìm thấy thiệt hại cần xóa" });
                    }
                    return NotFound();
                }

                _context.ThietHai.Remove(thietHai);
                await _context.SaveChangesAsync();

                if (isAjaxRequest)
                {
                    return Json(new { success = true, message = "Thiệt hại đã được xóa thành công!" });
                }

                TempData["SuccessMessage"] = "Báo cáo thiệt hại đã được xóa thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                if (isAjaxRequest)
                {
                    return Json(new { success = false, message = "Có lỗi xảy ra khi xóa thiệt hại: " + ex.Message });
                }
                
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa thiệt hại: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // API để lấy thông tin chi tiết thiệt hại
        [HttpGet]
        public async Task<IActionResult> GetChiTiet(int id)
        {
            var thietHai = await _context.ThietHai
                .Include(t => t.Xe)
                .Include(t => t.HopDong)
                    .ThenInclude(h => h.KhachHang)
                .Include(t => t.KhachHang)
                .Include(t => t.NguoiBaoCao)
                .FirstOrDefaultAsync(t => t.MaThietHai == id);

            if (thietHai == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin thiệt hại" });
            }

            var data = new
            {
                maThietHai = thietHai.MaThietHai,
                tenXe = thietHai.Xe?.TenXe ?? "Không xác định",
                bienSoXe = thietHai.Xe?.BienSoXe ?? "Không xác định",
                loaiThietHai = thietHai.LoaiThietHai,
                loaiThietHaiClass = thietHai.LoaiThietHaiClass,
                moTaThietHai = thietHai.MoTaThietHai,
                ngayXayRa = thietHai.NgayXayRa.ToString("dd/MM/yyyy"),
                khachHang = !string.IsNullOrEmpty(thietHai.HopDong?.HoTenKhach) ? thietHai.HopDong.HoTenKhach :
                           !string.IsNullOrEmpty(thietHai.HopDong?.KhachHang?.Ten) ? thietHai.HopDong.KhachHang.Ten :
                           !string.IsNullOrEmpty(thietHai.KhachHang?.Ten) ? thietHai.KhachHang.Ten : "Chưa xác định",
                soDienThoai = !string.IsNullOrEmpty(thietHai.HopDong?.SoDienThoai) ? thietHai.HopDong.SoDienThoai :
                             !string.IsNullOrEmpty(thietHai.HopDong?.KhachHang?.SoDienThoai) ? thietHai.HopDong.KhachHang.SoDienThoai :
                             !string.IsNullOrEmpty(thietHai.KhachHang?.SoDienThoai) ? thietHai.KhachHang.SoDienThoai : "Chưa xác định",
                cccd = thietHai.HopDong?.SoCCCD ?? "Chưa xác định",
                diaChi = !string.IsNullOrEmpty(thietHai.HopDong?.DiaChi) ? thietHai.HopDong.DiaChi : 
                         !string.IsNullOrEmpty(thietHai.HopDong?.KhachHang?.DiaChi) ? thietHai.HopDong.KhachHang.DiaChi :
                         !string.IsNullOrEmpty(thietHai.KhachHang?.DiaChi) ? thietHai.KhachHang.DiaChi : "Chưa xác định",
                trangThaiXuLy = thietHai.TrangThaiXuLy,
                trangThaiClass = thietHai.TrangThaiClass,
                phuongAnXuLy = thietHai.PhuongAnXuLy ?? "Chưa có",
                soTienDenBu = thietHai.SoTienDenBu.ToString("N0") + "đ",
                soTienConLai = thietHai.SoTienConLai.ToString("N0") + "đ",
                ngayHoanThanh = thietHai.NgayHoanThanh?.ToString("dd/MM/yyyy") ?? "Chưa hoàn thành",
                ghiChu = thietHai.GhiChu ?? "Không có",
                nguoiBaoCao = thietHai.NguoiBaoCao?.Ten ?? "Không xác định",
                ngayTao = thietHai.NgayTao.ToString("dd/MM/yyyy HH:mm")
            };
            return Json(new { success = true, data = data });
        }
        // API để lọc theo trạng thái
        [HttpPost]
        public async Task<IActionResult> FilterByStatus(string trangThai)
        {
            var query = _context.ThietHai
                .Include(t => t.Xe)
                .Include(t => t.HopDong)
                    .ThenInclude(h => h.KhachHang)
                .AsQueryable();

            if (!string.IsNullOrEmpty(trangThai) && trangThai != "Tất cả")
            {
                query = query.Where(t => t.TrangThaiXuLy == trangThai);
            }

            var thietHaiList = await query.OrderByDescending(t => t.NgayTao).ToListAsync();

            var rows = thietHaiList.Select(t => $@"
                <tr>
                    <td>
                        <span class='badge bg-{t.LoaiThietHaiClass}'>{t.LoaiThietHai}</span>
                        <br><small class='text-muted'>{t.Xe?.TenXe} ({t.Xe?.BienSoXe})</small>
                    </td>
                    <td>{t.MoTaThietHai}</td>
                    <td>{t.NgayXayRa.ToString("dd/MM/yyyy")}</td>
                    <td>
                        <span class='badge bg-{t.TrangThaiClass}'>{t.TrangThaiXuLy}</span>
                    </td>
                    <td class='text-success fw-bold'>{t.SoTienDenBu.ToString("N0")}đ</td>
                    <td class='text-center'>
                        <button type='button' class='btn btn-warning btn-sm' title='Sửa' onclick='showEditModal({t.MaThietHai})'>
                            <i class='bi bi-pencil-square'></i>
                        </button>
                        <button type='button' class='btn btn-info btn-sm' title='Chi tiết' onclick='showChiTietModal({t.MaThietHai})'>
                            <i class='bi bi-eye'></i>
                        </button>
                        <button type='button' class='btn btn-danger btn-sm' title='Xóa' onclick='showDeleteModal({t.MaThietHai})'>
                            <i class='bi bi-trash'></i>
                        </button>
                    </td>
                </tr>").ToArray();

            var data = new
            {
                rows = rows,
                soLuong = thietHaiList.Count,
                tongDenBu = thietHaiList.Sum(t => t.SoTienDenBu)
            };

            return Json(new { success = true, data = data });
        }

        // API để tìm kiếm
        [HttpPost]
        public async Task<IActionResult> SearchByContent(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return Json(new { success = false, message = "Vui lòng nhập từ khóa tìm kiếm" });
            }

            var thietHaiList = await _context.ThietHai
                .Include(t => t.Xe)
                .Include(t => t.HopDong)
                    .ThenInclude(h => h.KhachHang)
                                 .Where(t => t.MoTaThietHai.Contains(searchTerm) || 
                            t.Xe.TenXe.Contains(searchTerm) || 
                            t.Xe.BienSoXe.Contains(searchTerm) ||
                            t.LoaiThietHai.Contains(searchTerm) ||
                            (t.HopDong != null && ((t.HopDong.HoTenKhach != null && t.HopDong.HoTenKhach.Contains(searchTerm)) ||
                             (t.HopDong.KhachHang != null && t.HopDong.KhachHang.Ten.Contains(searchTerm)))))
                .OrderByDescending(t => t.NgayTao)
                .ToListAsync();

            var rows = thietHaiList.Select(t => $@"
                <tr>
                    <td>
                        <span class='badge bg-{t.LoaiThietHaiClass}'>{t.LoaiThietHai}</span>
                        <br><small class='text-muted'>{t.Xe?.TenXe} ({t.Xe?.BienSoXe})</small>
                    </td>
                    <td>{t.MoTaThietHai}</td>
                    <td>{t.NgayXayRa.ToString("dd/MM/yyyy")}</td>
                    <td>
                        <span class='badge bg-{t.TrangThaiClass}'>{t.TrangThaiXuLy}</span>
                    </td>
                    <td class='text-success fw-bold'>{t.SoTienDenBu.ToString("N0")}đ</td>
                    <td class='text-center'>
                        <button type='button' class='btn btn-warning btn-sm' title='Sửa' onclick='showEditModal({t.MaThietHai})'>
                            <i class='bi bi-pencil-square'></i>
                        </button>
                        <button type='button' class='btn btn-info btn-sm' title='Chi tiết' onclick='showChiTietModal({t.MaThietHai})'>
                            <i class='bi bi-eye'></i>
                        </button>
                        <button type='button' class='btn btn-danger btn-sm' title='Xóa' onclick='showDeleteModal({t.MaThietHai})'>
                            <i class='bi bi-trash'></i>
                        </button>
                    </td>
                </tr>").ToArray();

            var data = new
            {
                rows = rows,
                soLuong = thietHaiList.Count
            };

            return Json(new { success = true, data = data });
        }
        // API để lấy dữ liệu gốc
        [HttpGet]
        public async Task<IActionResult> GetOriginalData()
        {
            var thietHaiList = await _context.ThietHai
                .Include(t => t.Xe)
                .Include(t => t.HopDong)
                    .ThenInclude(h => h.KhachHang)
                .OrderByDescending(t => t.NgayTao)
                .ToListAsync();

            var rows = thietHaiList.Select(t => $@"
                <tr>
                    <td>
                        <span class='badge bg-{t.LoaiThietHaiClass}'>{t.LoaiThietHai}</span>
                        <br><small class='text-muted'>{t.Xe?.TenXe} ({t.Xe?.BienSoXe})</small>
                    </td>
                    <td>{t.MoTaThietHai}</td>
                    <td>{t.NgayXayRa.ToString("dd/MM/yyyy")}</td>
                    <td>
                        <span class='badge bg-{t.TrangThaiClass}'>{t.TrangThaiXuLy}</span>
                    </td>
                    <td class='text-success fw-bold'>{t.SoTienDenBu.ToString("N0")}đ</td>
                    <td class='text-center'>
                        <button type='button' class='btn btn-warning btn-sm' title='Sửa' onclick='showEditModal({t.MaThietHai})'>
                            <i class='bi bi-pencil-square'></i>
                        </button>
                        <button type='button' class='btn btn-info btn-sm' title='Chi tiết' onclick='showChiTietModal({t.MaThietHai})'>
                            <i class='bi bi-eye'></i>
                        </button>
                        <button type='button' class='btn btn-danger btn-sm' title='Xóa' onclick='showDeleteModal({t.MaThietHai})'>
                            <i class='bi bi-trash'></i>
                        </button>
                    </td>
                </tr>").ToArray();

            var data = new
            {
                rows = rows,
                soLuong = thietHaiList.Count
            };

            return Json(new { success = true, data = data });
        }

        private async Task PopulateViewData()
        {
            // Danh sách trạng thái xử lý
            var trangThaiList = new List<SelectListItem>
            {
                new SelectListItem { Value = "Chưa xử lý", Text = "Chưa xử lý" },
                new SelectListItem { Value = "Đang xử lý", Text = "Đang xử lý" },
                new SelectListItem { Value = "Đã xử lý", Text = "Đã xử lý" },
                new SelectListItem { Value = "Đã đền bù", Text = "Đã đền bù" }
            };
            ViewData["TrangThaiXuLy"] = new SelectList(trangThaiList, "Value", "Text");
        }

        private bool ThietHaiExists(int id)
        {
            return _context.ThietHai.Any(e => e.MaThietHai == id);
        }

        // Debug action để xem dữ liệu thực tế
        [HttpGet]
        public async Task<IActionResult> DebugThietHai(int id)
        {
            var thietHai = await _context.ThietHai
                .Include(t => t.Xe)
                .Include(t => t.HopDong)
                    .ThenInclude(h => h.KhachHang)
                .Include(t => t.KhachHang)
                .Include(t => t.NguoiBaoCao)
                .FirstOrDefaultAsync(t => t.MaThietHai == id);

            if (thietHai == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin thiệt hại" });
            }

            var debugData = new
            {
                maThietHai = thietHai.MaThietHai,
                hopDongDiaChi = thietHai.HopDong?.DiaChi,
                hopDongKhachHangDiaChi = thietHai.HopDong?.KhachHang?.DiaChi,
                khachHangDiaChi = thietHai.KhachHang?.DiaChi,
                hopDongHoTenKhach = thietHai.HopDong?.HoTenKhach,
                hopDongKhachHangTen = thietHai.HopDong?.KhachHang?.Ten,
                khachHangTen = thietHai.KhachHang?.Ten,
                hopDongSoDienThoai = thietHai.HopDong?.SoDienThoai,
                hopDongKhachHangSoDienThoai = thietHai.HopDong?.KhachHang?.SoDienThoai,
                khachHangSoDienThoai = thietHai.KhachHang?.SoDienThoai
            };

            return Json(new { success = true, data = debugData });
        }
    }
} 