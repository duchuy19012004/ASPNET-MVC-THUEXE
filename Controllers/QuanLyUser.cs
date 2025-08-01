using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using bike.Models;
using bike.Attributes;
using bike.ViewModel;
using bike.Services.QuanLyUsers;
using System.Security.Claims;

namespace bike.Controllers
{
    
    public class QuanLyUser : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<QuanLyUser> _logger;

        public QuanLyUser(IUserService userService, ILogger<QuanLyUser> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        // GET: User
        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }

        // GET: User/Create
        public IActionResult Create()
        {
            ViewBag.Roles = new SelectList(_userService.GetRoleOptions(), "Value", "Text");
            return View();
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            var result = await _userService.CreateUserAsync(user);
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                if (result.IsSuccess && result.Data != null)
                {
                    return Json(new {
                        success = true,
                        message = result.Message,
                        user = new {
                            id = result.Data.Id,
                            ten = result.Data.Ten,
                            email = result.Data.Email,
                            vaiTro = result.Data.VaiTro,
                            isActive = result.Data.IsActive,
                            ngayTao = result.Data.NgayTao.ToString("dd/MM/yyyy")
                        }
                    });
                }
                // Trả về lỗi rõ ràng nếu có
                return Json(new { success = false, message = result.Message, errors = result.Errors });
            }

            if (result.IsSuccess)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            // Add validation errors to ModelState
            foreach (var error in result.Errors)
            {
                foreach (var message in error.Value)
                {
                    ModelState.AddModelError(error.Key, message);
                }
            }

            ViewBag.Roles = new SelectList(_userService.GetRoleOptions(), "Value", "Text", user.VaiTro);
            return View(user);
        }

        // GET: User/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var editViewModel = await _userService.GetUserForEditAsync(id.Value);
            if (editViewModel == null)
            {
                return NotFound();
            }

            ViewBag.Roles = new SelectList(_userService.GetRoleOptions(), "Value", "Text", editViewModel.VaiTro);

            // If AJAX request, return partial view
            bool isAjaxRequest = Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                                Request.Headers.Accept.ToString().Contains("application/json");
            
            if (isAjaxRequest)
            {
                ViewData["IsPartial"] = true;
                return PartialView("Edit", editViewModel);
            }

            return View(editViewModel);
        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditUserViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var result = await _userService.UpdateUserAsync(model);
                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        foreach (var message in error.Value)
                        {
                            ModelState.AddModelError(error.Key, message);
                        }
                    }
                }
            }

            ViewBag.Roles = new SelectList(_userService.GetRoleOptions(), "Value", "Text");
            return View(model);
        }

        // GET: User/Permissions/{id}
        public async Task<IActionResult> Permissions(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var permissions = await _userService.GetUserPermissionsAsync(id);
            if (permissions == null)
            {
                permissions = new UserPermissionViewModel
                {
                    UserId = id,
                    UserName = user.Ten,
                    UserEmail = user.Email
                };
            }

            ViewBag.PermissionOptions = UserPermissionViewModel.PermissionOptions;
            return View(permissions);
        }

        // POST: User/Permissions/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Permissions(int id, UserPermissionViewModel model)
        {
            if (id != model.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var success = await _userService.UpdateUserPermissionsAsync(model);
                if (success)
                {
                    TempData["SuccessMessage"] = "Cập nhật quyền thành công!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật quyền.");
                }
            }

            ViewBag.PermissionOptions = UserPermissionViewModel.PermissionOptions;
            return View(model);
        }

        // GET: User/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userService.GetUserByIdAsync(id.Value);
            if (user == null)
            {
                return NotFound();
            }

            var currentUserId = GetCurrentUserId();
            
            // Check if user can be deleted
            if (user.Id == currentUserId)
            {
                TempData["Error"] = "Không thể xóa tài khoản của chính mình!";
                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var currentUserId = GetCurrentUserId();
            var result = await _userService.DeleteUserAsync(id, currentUserId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                if (result.IsSuccess)
                {
                    return Json(new { success = true, message = result.Message });
                }
                return Json(new { success = false, message = result.Message });
            }

            if (result.IsSuccess)
            {
                return HandleSuccessResponse(result.Message);
            }

            return HandleErrorResponse(result.Message);
        }

        // Helper methods for handling responses
        private IActionResult HandleSuccessResponse(string message)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message });
            }
            
            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        private IActionResult HandleErrorResponse(string message)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message });
            }
            
            TempData["Error"] = message;
            return RedirectToAction(nameof(Index));
        }

        private IActionResult HandleNotFoundResponse(string message)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message });
            }
            
            return NotFound();
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }
    }
}