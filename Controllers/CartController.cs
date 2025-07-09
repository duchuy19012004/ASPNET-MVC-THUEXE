using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bike.Repository;
using bike.Models;
using bike.Services;
using System.Security.Claims;

namespace bike.Controllers
{
    public class CartController : Controller
    {
        private readonly BikeDbContext _context;
        private readonly ICartService _cartService;

        public CartController(BikeDbContext context, ICartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        // GET: Cart - Xem giỏ xe
        public IActionResult Index()
        {
            var cart = _cartService.GetCart();
            
            // Load thông tin người dùng nếu đã đăng nhập
            if (User.Identity.IsAuthenticated && string.IsNullOrEmpty(cart.HoTen))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = _context.Users.Find(int.Parse(userId));
                if (user != null)
                {
                    cart.HoTen = user.Ten;
                    cart.Email = user.Email;
                    cart.SoDienThoai = user.SoDienThoai ?? "";
                }
            }

            return View(cart);
        }

        // POST: Cart/Add - Thêm xe vào giỏ
        [HttpPost]
        public async Task<IActionResult> Add(int maXe, DateTime ngayNhanXe, DateTime ngayTraXe, string? ghiChu)
        {
            try
            {
                // Validation
                if (ngayNhanXe < DateTime.Now.Date)
                {
                    return Json(new { success = false, message = "Ngày nhận xe phải từ hôm nay trở đi" });
                }

                if (ngayTraXe <= ngayNhanXe)
                {
                    return Json(new { success = false, message = "Ngày trả xe phải sau ngày nhận xe" });
                }

                // Lấy thông tin xe
                var xe = await _context.Xe
                    .Include(x => x.LoaiXe)
                    .FirstOrDefaultAsync(x => x.MaXe == maXe);

                if (xe == null || xe.TrangThai != "Sẵn sàng")
                {
                    return Json(new { success = false, message = "Xe không khả dụng" });
                }

                // Tạo CartItem
                var cartItem = new CartItem
                {
                    MaXe = xe.MaXe,
                    TenXe = xe.TenXe,
                    BienSoXe = xe.BienSoXe,
                    GiaThue = xe.GiaThue,
                    HinhAnhXe = xe.HinhAnhXe,
                    TenLoaiXe = xe.LoaiXe?.TenLoaiXe,
                    NgayNhanXe = ngayNhanXe,
                    NgayTraXe = ngayTraXe,
                    GhiChu = ghiChu
                };

                // Thêm vào giỏ
                _cartService.AddToCart(cartItem);

                return Json(new 
                { 
                    success = true, 
                    message = "Đã thêm xe vào giỏ thành công!",
                    cartItemCount = _cartService.GetCartItemCount()
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // POST: Cart/Remove - Xóa xe khỏi giỏ
        [HttpPost]
        public IActionResult Remove(int maXe, DateTime ngayNhanXe, DateTime ngayTraXe)
        {
            try
            {
                _cartService.RemoveFromCart(maXe, ngayNhanXe, ngayTraXe);
                return Json(new 
                { 
                    success = true, 
                    message = "Đã xóa xe khỏi giỏ",
                    cartItemCount = _cartService.GetCartItemCount()
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // GET: Cart/Checkout - Trang thanh toán
        public IActionResult Checkout()
        {
            var cart = _cartService.GetCart();
            
            if (cart.TongSoXe == 0)
            {
                TempData["Error"] = "Giỏ xe của bạn đang trống";
                return RedirectToAction("Index");
            }

            // Load thông tin người dùng nếu đã đăng nhập
            if (User.Identity.IsAuthenticated && string.IsNullOrEmpty(cart.HoTen))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = _context.Users.Find(int.Parse(userId));
                if (user != null)
                {
                    cart.HoTen = user.Ten;
                    cart.Email = user.Email;
                    cart.SoDienThoai = user.SoDienThoai ?? "";
                }
            }

            return View(cart);
        }

        // POST: Cart/Checkout - Tạo hợp đồng từ giỏ xe
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(Cart model)
        {
            var cart = _cartService.GetCart();
            
            if (cart.TongSoXe == 0)
            {
                TempData["Error"] = "Giỏ xe của bạn đang trống";
                return RedirectToAction("Index");
            }

            // Validate model
            if (string.IsNullOrEmpty(model.HoTen) || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.SoDienThoai))
            {
                ModelState.AddModelError("", "Vui lòng điền đầy đủ thông tin");
                cart.HoTen = model.HoTen;
                cart.Email = model.Email;
                cart.SoDienThoai = model.SoDienThoai;
                cart.GhiChuChung = model.GhiChuChung;
                return View(cart);
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Kiểm tra tất cả xe vẫn còn khả dụng
                    foreach (var item in cart.Items)
                    {
                        var xe = await _context.Xe.FindAsync(item.MaXe);
                        if (xe == null || xe.TrangThai != "Sẵn sàng")
                        {
                            ModelState.AddModelError("", $"Xe {item.TenXe} không còn khả dụng");
                            cart.HoTen = model.HoTen;
                            cart.Email = model.Email;
                            cart.SoDienThoai = model.SoDienThoai;
                            cart.GhiChuChung = model.GhiChuChung;
                            return View(cart);
                        }
                    }

                    // Tạo hợp đồng chính
                    var hopDong = new HopDong
                    {
                        HoTenKhach = model.HoTen,
                        SoDienThoai = model.SoDienThoai,
                        SoCCCD = "", // Sẽ được cập nhật sau
                        DiaChi = "", // Sẽ được cập nhật sau
                        NgayNhanXe = cart.Items.Min(i => i.NgayNhanXe),
                        NgayTraXeDuKien = cart.Items.Max(i => i.NgayTraXe),
                        TienCoc = 0, // Sẽ được cập nhật sau
                        PhuPhi = 0,
                        GhiChu = model.GhiChuChung,
                        NgayTao = DateTime.Now,
                        TrangThai = "Chờ xác nhận", // Trạng thái đặt chỗ
                        ChiTietHopDong = new List<ChiTietHopDong>()
                    };

                    // Nếu đã đăng nhập, lưu người tạo
                    if (User.Identity.IsAuthenticated)
                    {
                        hopDong.MaNguoiTao = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    }

                    // Tạo chi tiết hợp đồng cho từng xe
                    foreach (var item in cart.Items)
                    {
                        var xe = await _context.Xe.FindAsync(item.MaXe);
                        var soNgayThue = (item.NgayTraXe - item.NgayNhanXe).Days + 1;

                        var chiTiet = new ChiTietHopDong
                        {
                            MaXe = item.MaXe,
                            GiaThueNgay = xe.GiaThue,
                            NgayNhanXe = item.NgayNhanXe,
                            NgayTraXeDuKien = item.NgayTraXe,
                            SoNgayThue = soNgayThue,
                            ThanhTien = xe.GiaThue * soNgayThue,
                            TrangThaiXe = "Chờ xác nhận",
                            GhiChu = item.GhiChu,
                            NgayTao = DateTime.Now
                        };

                        hopDong.ChiTietHopDong.Add(chiTiet);
                    }

                    // Tính tổng tiền
                    hopDong.TongTien = hopDong.ChiTietHopDong.Sum(ct => ct.ThanhTien);

                    // Lưu hợp đồng
                    _context.HopDong.Add(hopDong);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    // Xóa giỏ xe
                    _cartService.ClearCart();

                    // Chuyển đến trang xác nhận
                    return RedirectToAction("ConfirmationContract", new { id = hopDong.MaHopDong });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                    cart.HoTen = model.HoTen;
                    cart.Email = model.Email;
                    cart.SoDienThoai = model.SoDienThoai;
                    cart.GhiChuChung = model.GhiChuChung;
                    return View(cart);
                }
            }
        }

        // GET: Cart/ConfirmationContract - Xác nhận tạo hợp đồng thành công
        public async Task<IActionResult> ConfirmationContract(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hopDong = await _context.HopDong
                .Include(h => h.ChiTietHopDong)
                .ThenInclude(ct => ct.Xe)
                .ThenInclude(x => x.LoaiXe)
                .FirstOrDefaultAsync(h => h.MaHopDong == id);

            if (hopDong == null)
            {
                return NotFound();
            }

            return View(hopDong);
        }

        // GET: Cart/ConfirmationMultiple - Giữ lại để tương thích ngược
        public async Task<IActionResult> ConfirmationMultiple(string ids)
        {
            if (string.IsNullOrEmpty(ids))
            {
                return NotFound();
            }

            var idList = ids.Split(',').Select(int.Parse).ToList();
            
            var reservations = await _context.DatCho
                .Include(d => d.Xe)
                .ThenInclude(x => x.LoaiXe)
                .Where(d => idList.Contains(d.MaDatCho))
                .ToListAsync();

            if (!reservations.Any())
            {
                return NotFound();
            }

            return View(reservations);
        }

        // GET: Cart/GetItemCount - Lấy số lượng item trong giỏ (AJAX)
        public IActionResult GetItemCount()
        {
            return Json(new { count = _cartService.GetCartItemCount() });
        }
    }
} 