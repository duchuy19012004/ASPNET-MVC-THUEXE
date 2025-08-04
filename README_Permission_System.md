# Hệ thống Phân quyền Chi tiết

## Tổng quan

Hệ thống phân quyền mới cho phép quản lý quyền truy cập chi tiết cho từng user thay vì chỉ dựa vào role cơ bản (Admin, Staff, User). Mỗi user có thể được cấu hình quyền riêng biệt cho từng chức năng trong hệ thống.

## Các tính năng chính

### 1. Quản lý quyền chi tiết

- **Quản lý xe**: Xem, Thêm mới, Chỉnh sửa, Xóa
- **Quản lý loại xe**: Xem, Thêm mới, Chỉnh sửa, Xóa
- **Quản lý hợp đồng**: Xem, Thêm mới, Chỉnh sửa, Xóa, In
- **Quản lý hóa đơn**: Xem, Thêm mới, Chỉnh sửa, Xóa, In
- **Quản lý nhân viên**: Xem, Thêm mới, Chỉnh sửa, Xóa
- **Quản lý người dùng**: Xem, Thêm mới, Chỉnh sửa, Xóa
- **Quản lý banner**: Xem, Thêm mới, Chỉnh sửa, Xóa
- **Quản lý chi tiêu**: Xem, Thêm mới, Chỉnh sửa, Xóa
- **Quản lý thiệt hại**: Xem, Thêm mới, Chỉnh sửa, Xóa, Thanh toán
- **Báo cáo thống kê**: Xem, Xuất báo cáo
- **Quản lý hình ảnh xe**: Xem, Upload, Xóa

### 2. Các mức quyền

- **None**: Không có quyền
- **View**: Chỉ xem
- **Create**: Xem + Thêm mới
- **Edit**: Xem + Chỉnh sửa
- **Delete**: Xem + Xóa
- **All**: Toàn quyền (Xem + Thêm + Sửa + Xóa)

## Cách sử dụng

### 1. Truy cập quản lý quyền

1. Đăng nhập với tài khoản Admin
2. Vào menu "Quản lý người dùng"
3. Click vào nút "Quản lý quyền" (biểu tượng shield) bên cạnh user cần cấu hình
4. Sử dụng radio buttons để chọn quyền cho từng chức năng
5. Click "Lưu quyền" để áp dụng

### 2. Sử dụng trong Controller

```csharp
// Kiểm tra quyền xem xe
[PermissionAuthorize("CanViewXe")]
public async Task<IActionResult> Index()
{
    // Code xử lý
}

// Kiểm tra quyền thêm xe
[PermissionAuthorize("CanCreateXe")]
public async Task<IActionResult> Create()
{
    // Code xử lý
}

// Kiểm tra quyền sửa xe
[PermissionAuthorize("CanEditXe")]
public async Task<IActionResult> Edit(int id)
{
    // Code xử lý
}

// Kiểm tra quyền xóa xe
[PermissionAuthorize("CanDeleteXe")]
public async Task<IActionResult> Delete(int id)
{
    // Code xử lý
}
```

### 3. Sử dụng trong View

```html
<!-- Hiển thị nút dựa trên quyền -->
@if (Html.HasPermission("CanCreateXe")) {
<a href="/Xe/Create" class="btn btn-primary">Thêm xe mới</a>
}

<!-- Sử dụng helper để tạo nút -->
@Html.PermissionButton("CanCreateXe", "Create", "Xe", null, "Thêm xe mới", "btn
btn-primary")

<!-- Sử dụng helper để tạo link -->
@Html.PermissionLink("CanViewXe", "Details", "Xe", new { id = item.MaXe }, "Xem
chi tiết", "btn btn-info btn-sm")
```

### 4. Kiểm tra quyền trong code

```csharp
// Trong Controller
public class SomeController : Controller
{
    private readonly IPermissionService _permissionService;

    public SomeController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    public async Task<IActionResult> SomeAction()
    {
        // Kiểm tra quyền của user hiện tại
        var canEditXe = await _permissionService.HasPermissionAsync(User, "CanEditXe");

        if (canEditXe)
        {
            // Thực hiện hành động
        }

        return View();
    }
}
```

## Cấu trúc Database

### Bảng UserPermissions

- `Id`: Khóa chính
- `UserId`: Khóa ngoại đến bảng Users
- `CanViewXe`, `CanCreateXe`, `CanEditXe`, `CanDeleteXe`: Quyền xe
- `CanViewLoaiXe`, `CanCreateLoaiXe`, `CanEditLoaiXe`, `CanDeleteLoaiXe`: Quyền loại xe
- `CanViewHopDong`, `CanCreateHopDong`, `CanEditHopDong`, `CanDeleteHopDong`, `CanPrintHopDong`: Quyền hợp đồng
- `CanViewHoaDon`, `CanCreateHoaDon`, `CanEditHoaDon`, `CanDeleteHoaDon`, `CanPrintHoaDon`: Quyền hóa đơn
- `CanViewNhanVien`, `CanCreateNhanVien`, `CanEditNhanVien`, `CanDeleteNhanVien`: Quyền nhân viên
- `CanViewUser`, `CanCreateUser`, `CanEditUser`, `CanDeleteUser`: Quyền user
- `CanViewBanner`, `CanCreateBanner`, `CanEditBanner`, `CanDeleteBanner`: Quyền banner
- `CanViewChiTieu`, `CanCreateChiTieu`, `CanEditChiTieu`, `CanDeleteChiTieu`: Quyền chi tiêu
- `CanViewThietHai`, `CanCreateThietHai`, `CanEditThietHai`, `CanDeleteThietHai`, `CanThanhToanThietHai`: Quyền thiệt hại
- `CanViewBaoCao`, `CanViewThongKe`, `CanExportBaoCao`: Quyền báo cáo
- `CanViewCart`, `CanCheckout`: Quyền giỏ hàng

- `CanViewHinhAnhXe`, `CanUploadHinhAnhXe`, `CanDeleteHinhAnhXe`: Quyền hình ảnh xe

## Lưu ý quan trọng

1. **Quyền mặc định**: Khi tạo user mới, hệ thống sẽ tự động tạo record quyền với các quyền cơ bản (chủ yếu là View)

2. **Backward compatibility**: Hệ thống vẫn hỗ trợ CustomAuthorizeAttribute cũ dựa trên role

3. **Performance**: Quyền được cache trong session để tối ưu hiệu suất

4. **Security**: Tất cả các action đều được kiểm tra quyền trước khi thực thi

## Troubleshooting

### Lỗi thường gặp

1. **User không thấy nút**: Kiểm tra quyền của user trong bảng UserPermissions
2. **Lỗi 403 Access Denied**: User không có quyền truy cập action này
3. **Quyền không được lưu**: Kiểm tra kết nối database và quyền ghi

### Debug

```csharp
// Kiểm tra quyền của user
var permission = await _permissionService.GetUserPermissionsAsync(userId);
var canViewXe = permission?.CanViewXe ?? false;
```

## Migration

Để cập nhật database:

```bash
dotnet ef migrations add AddUserPermissions
dotnet ef database update
```
