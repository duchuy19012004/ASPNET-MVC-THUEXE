using Microsoft.AspNetCore.Mvc;
using bike.Models;
using bike.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using bike.Attributes;

namespace bike.Controllers
{
    public class LoaiXeController : Controller
    {
        private readonly BikeDbContext _context;
        public LoaiXeController(BikeDbContext context)
        {
            _context = context;
        }

        // GET: /LoaiXe
        [PermissionAuthorize("CanViewLoaiXe")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.LoaiXe.ToListAsync());
        }

        // GET: /LoaiXe/Create
        [PermissionAuthorize("CanCreateLoaiXe")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /LoaiXe/Create
        [HttpPost]
        [Route("LoaiXe/Create")]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("CanCreateLoaiXe")]
        public async Task<IActionResult> Create([FromBody] LoaiXe loaiXe)
        {
            // Kiểm tra dữ liệu đầu vào
            if (loaiXe == null || string.IsNullOrWhiteSpace(loaiXe.TenLoaiXe))
            {
                if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
                {
                    return Json(new { success = false, message = "Tên loại xe không được để trống!" });
                }
                return View(loaiXe);
            }

            // Kiểm tra ModelState validation
            if (!ModelState.IsValid)
            {
                if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
                {
                    var errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                    return Json(new { success = false, errors = errors });
                }
                return View(loaiXe);
            }

            try
            {
                loaiXe.NgayTao = DateTime.Now;
                _context.Add(loaiXe);
                await _context.SaveChangesAsync();
                
                if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
                {
                    return Json(new { success = true, message = "Thêm loại xe thành công!" });
                }
                
                TempData["SuccessMessage"] = "Thêm loại xe thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
                {
                    return Json(new { success = false, message = "Có lỗi xảy ra khi thêm loại xe!" });
                }
                
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi thêm loại xe!";
                return View(loaiXe);
            }
        }

        // GET: /LoaiXe/Edit/5
        [PermissionAuthorize("CanEditLoaiXe")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var loaiXe = await _context.LoaiXe.FindAsync(id);
            if (loaiXe == null) return NotFound();
            return View(loaiXe);
        }

        // POST: /LoaiXe/Edit/5
        [HttpPost]
        [Route("LoaiXe/Edit/{id}")]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("CanEditLoaiXe")]
        public async Task<IActionResult> Edit(int id, [FromBody] LoaiXe loaiXe)
        {
            try
            {
                if (loaiXe == null || id != loaiXe.MaLoaiXe) 
                {
                    if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
                    {
                        return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });
                    }
                    return NotFound();
                }

                // Kiểm tra dữ liệu đầu vào
                if (string.IsNullOrWhiteSpace(loaiXe.TenLoaiXe))
                {
                    if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
                    {
                        return Json(new { success = false, message = "Tên loại xe không được để trống!" });
                    }
                    return View(loaiXe);
                }

                // Kiểm tra xem loại xe có tồn tại không
                var existingLoaiXe = await _context.LoaiXe.FindAsync(id);
                if (existingLoaiXe == null)
                {
                    if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
                    {
                        return Json(new { success = false, message = "Không tìm thấy loại xe!" });
                    }
                    return NotFound();
                }

                // Cập nhật dữ liệu
                existingLoaiXe.TenLoaiXe = loaiXe.TenLoaiXe;
                existingLoaiXe.NgayCapNhat = DateTime.Now;
                
                _context.Update(existingLoaiXe);
                await _context.SaveChangesAsync();
                
                if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
                {
                    return Json(new { success = true, message = "Cập nhật loại xe thành công!" });
                }
                
                TempData["SuccessMessage"] = "Cập nhật loại xe thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LoaiXeExists(id)) 
                {
                    if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
                    {
                        return Json(new { success = false, message = "Loại xe không tồn tại!" });
                    }
                    return NotFound();
                }
                else throw;
            }
            catch (Exception ex)
            {
                if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
                {
                    return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật loại xe!" });
                }
                
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật loại xe!";
                return View(loaiXe);
            }
        }

        // GET: /LoaiXe/Delete/5
        [PermissionAuthorize("CanDeleteLoaiXe")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var loaiXe = await _context.LoaiXe.FindAsync(id);
            if (loaiXe == null) return NotFound();
            return View(loaiXe);
        }

        // POST: /LoaiXe/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("LoaiXe/Delete/{id}")]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("CanDeleteLoaiXe")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var loaiXe = await _context.LoaiXe.FindAsync(id);
            if (loaiXe == null)
            {
                if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
                {
                    return Json(new { success = false, message = "Không tìm thấy loại xe!" });
                }
                return NotFound();
            }

            // Kiểm tra xem loại xe có đang được sử dụng bởi xe nào không
            var xeUsingLoaiXe = await _context.Xe
                .Where(x => x.MaLoaiXe == id)
                .FirstOrDefaultAsync();

            if (xeUsingLoaiXe != null)
            {
                if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
                {
                    return Json(new { 
                        success = false, 
                        message = $"Không thể xóa loại xe '{loaiXe.TenLoaiXe}' vì đang được sử dụng bởi xe khác!" 
                    });
                }
                
                TempData["ErrorMessage"] = $"Không thể xóa loại xe '{loaiXe.TenLoaiXe}' vì đang được sử dụng bởi xe khác!";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.LoaiXe.Remove(loaiXe);
                await _context.SaveChangesAsync();
                
                if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
                {
                    return Json(new { success = true, message = "Xóa loại xe thành công!" });
                }
                
                TempData["SuccessMessage"] = "Xóa loại xe thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
                {
                    return Json(new { success = false, message = "Có lỗi xảy ra khi xóa loại xe!" });
                }
                
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa loại xe!";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool LoaiXeExists(int id)
        {
            return _context.LoaiXe.Any(e => e.MaLoaiXe == id);
        }
    }
}
