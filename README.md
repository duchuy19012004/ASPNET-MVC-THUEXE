# 🏍️ Hệ Thống Quản Lý Cho Thuê Xe Máy Sài Gòn

Một hệ thống quản lý cho thuê xe máy toàn diện được xây dựng bằng ASP.NET Core MVC, cung cấp giải pháp số hóa cho doanh nghiệp cho thuê xe máy tại TP.HCM.

![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-blue)
![Entity Framework](https://img.shields.io/badge/Entity%20Framework-8.0-green)
![SQL Server](https://img.shields.io/badge/SQL%20Server-2019-red)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3-purple)

## 📋 Mục Lục

- [Tính Năng Chính](#-tính-năng-chính)
- [Công Nghệ Sử Dụng](#-công-nghệ-sử-dụng)
- [Cài Đặt](#-cài-đặt)
- [Cấu Hình](#-cấu-hình)
- [Sử Dụng](#-sử-dụng)
- [Phân Quyền](#-phân-quyền)
- [Database Schema](#-database-schema)
- [Screenshots](#-screenshots)
- [Đóng Góp](#-đóng-góp)
- [License](#-license)

## 🚀 Tính Năng Chính

### 👥 Quản Lý Người Dùng

- **Đăng ký/Đăng nhập**: Hệ thống xác thực an toàn với Cookie Authentication
- **Phân quyền 3 cấp**: User (Khách hàng), Staff (Nhân viên), Admin (Quản trị viên)
- **Quản lý hồ sơ**: Cập nhật thông tin cá nhân, đổi mật khẩu
- **Quản lý nhân viên**: CRUD nhân viên, phân quyền, quản lý lương

### 🏍️ Quản Lý Xe Máy

- **Catalog xe đầy đủ**: Hiển thị danh sách xe với hình ảnh, thông tin chi tiết
- **Phân loại xe**: Quản lý theo loại xe (Xe ga, Xe số, Xe côn tay...)
- **Tìm kiếm & Lọc**: Tìm kiếm theo tên, lọc theo loại xe, hãng xe, trạng thái
- **Quản lý trạng thái**: Sẵn sàng, Đang thuê, Bảo trì
- **Upload hình ảnh**: Quản lý hình ảnh xe máy

### 🛒 Hệ Thống Đặt Xe

- **Giỏ xe thông minh**: Thêm nhiều xe vào giỏ với thời gian thuê khác nhau
- **Đặt chỗ trực tuyến**: Đặt giữ chỗ xe với thông tin liên hệ
- **Tính toán giá tự động**: Tính toán chi phí theo thời gian thuê
- **Thông báo realtime**: Badge thông báo đơn mới cho Admin/Staff

### 📋 Quản Lý Hợp Đồng

- **Tạo hợp đồng**: Chuyển đổi từ đặt chỗ sang hợp đồng thuê
- **Theo dõi trạng thái**: Đang thuê, Hoàn thành, Hủy
- **Quản lý trả xe**: Xử lý trả xe với tính toán phí phụ
- **Lịch sử hợp đồng**: Theo dõi lịch sử thuê xe của khách hàng

### 💰 Quản Lý Tài Chính

- **Hóa đơn tự động**: Tạo hóa đơn khi hoàn thành hợp đồng
- **Quản lý chi tiêu**: Theo dõi chi phí vận hành
- **Báo cáo doanh thu**: Thống kê theo ngày, tháng, năm
- **Dashboard admin**: Tổng quan tình hình kinh doanh

### 🎨 Giao Diện & UX

- **Responsive Design**: Tương thích với mọi thiết bị
- **Bootstrap 5**: Giao diện hiện đại, thân thiện
- **Animation**: Hiệu ứng mượt mà, chuyển tiếp tự nhiên
- **Search & Filter**: Tìm kiếm thông minh với autocomplete

### 🔧 Tính Năng Quản Trị

- **Quản lý banner**: Upload và quản lý banner trang chủ
- **Thống kê realtime**: Cập nhật số liệu theo thời gian thực
- **Phân quyền chi tiết**: Kiểm soát truy cập theo vai trò
- **Quản lý session**: Bảo mật thông tin người dùng

## 🛠️ Công Nghệ Sử Dụng

### Backend

- **ASP.NET Core 8.0**: Framework chính
- **Entity Framework Core 8.0**: ORM cho database
- **SQL Server**: Hệ quản trị cơ sở dữ liệu
- **Cookie Authentication**: Xác thực người dùng
- **LINQ**: Truy vấn dữ liệu

### Frontend

- **Razor Pages**: Template engine
- **Bootstrap 5.3**: CSS Framework
- **jQuery**: JavaScript library
- **Bootstrap Icons**: Icon set
- **CSS3 Animations**: Hiệu ứng giao diện

### Tools & DevOps

- **Visual Studio Code**: IDE
- **SQL Server Management Studio**: Database management
- **Git**: Version control
- **IIS Express**: Development server

## 📦 Cài Đặt

### Yêu Cầu Hệ Thống

- .NET 8.0 SDK
- SQL Server 2019+ hoặc SQL Server Express
- Visual Studio 2022 hoặc VS Code
- Windows 10/11 hoặc macOS/Linux

### Bước 1: Clone Repository

```bash
git clone https://github.com/yourusername/bike-rental-system.git
cd bike-rental-system/clone-x1
```

### Bước 2: Cài Đặt Dependencies

```bash
dotnet restore
```

### Bước 3: Cấu Hình Database

1. Mở SQL Server Management Studio
2. Tạo database mới tên `BikeDB2`
3. Cập nhật connection string trong `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "BikeConnection": "Server=YOUR_SERVER;Database=BikeDB2;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Bước 4: Chạy Migration

```bash
dotnet ef database update
```

### Bước 5: Chạy Ứng Dụng

```bash
dotnet run
```

Truy cập: `https://localhost:7034` hoặc `http://localhost:5063`

## ⚙️ Cấu Hình

### Connection String

Cập nhật `appsettings.json` với thông tin SQL Server của bạn:

```json
{
  "ConnectionStrings": {
    "BikeConnection": "Server=LAPTOP-NAME\\SQLEXPRESS;Database=BikeDB2;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Upload Path

Hình ảnh xe được lưu tại: `wwwroot/images/xe/`
Banner được lưu tại: `wwwroot/images/banner/`

## 📖 Sử Dụng

### 1. Đăng Ký Tài Khoản Admin Đầu Tiên

- Truy cập `/Account/Register`
- Đăng ký tài khoản với email: `admin@bikerental.com`
- Cập nhật role thành "Admin" trong database

### 2. Thêm Loại Xe

- Đăng nhập Admin → Quản lý loại xe
- Thêm các loại: "Xe ga", "Xe số", "Xe côn tay"

### 3. Thêm Xe Máy

- Quản lý xe → Thêm xe mới
- Upload hình ảnh, nhập thông tin chi tiết
- Đặt trạng thái "Sẵn sàng"

### 4. Cấu Hình Banner

- Quản lý banner → Upload hình ảnh trang chủ
- Thiết lập thứ tự hiển thị

### 5. Tạo Nhân Viên

- Quản lý nhân viên → Thêm nhân viên mới
- Phân quyền Staff cho nhân viên

## 🔐 Phân Quyền

### 👑 Admin (Quản trị viên)

- Toàn quyền truy cập mọi chức năng
- Quản lý nhân viên và phân quyền
- Xem báo cáo và dashboard
- Quản lý tài chính và chi tiêu

### 👨‍💼 Staff (Nhân viên)

- Quản lý xe máy và đặt chỗ
- Xử lý hợp đồng và trả xe
- Tạo hóa đơn
- Quản lý banner

### 👤 User (Khách hàng)

- Xem catalog xe máy
- Đặt chỗ và thuê xe
- Quản lý hồ sơ cá nhân
- Xem lịch sử thuê xe

## 🗄️ Database Schema

### Bảng Chính

- **Users**: Thông tin người dùng và nhân viên
- **Xe**: Danh sách xe máy
- **LoaiXe**: Phân loại xe máy
- **DatCho**: Đặt chỗ trước
- **HopDong**: Hợp đồng thuê xe
- **ChiTietHopDong**: Chi tiết xe trong hợp đồng
- **HoaDon**: Hóa đơn thanh toán
- **ChiTieu**: Quản lý chi phí
- **Banner**: Banner trang chủ

### Relationships

```
Users (1) ←→ (n) DatCho
Users (1) ←→ (n) HopDong
Xe (1) ←→ (n) DatCho
Xe (1) ←→ (n) ChiTietHopDong
LoaiXe (1) ←→ (n) Xe
HopDong (1) ←→ (n) ChiTietHopDong
HopDong (1) ←→ (1) HoaDon
DatCho (1) ←→ (1) HopDong
```

## 🔧 Development

### Thêm Migration Mới

```bash
dotnet ef migrations add TenMigration
dotnet ef database update
```

### Debugging

```bash
dotnet run --configuration Debug
```

### Build Production

```bash
dotnet publish -c Release -o ./publish
```

## 📱 Screenshots

### 🏠 Trang Chủ

- Carousel banner động
- Catalog xe với filter thông minh
- Responsive design

### 🛒 Giỏ Xe & Đặt Chỗ

- Thêm nhiều xe với thời gian khác nhau
- Tính toán giá tự động
- Form đặt chỗ đầy đủ

### 🎛️ Admin Dashboard

- Thống kê tổng quan
- Quản lý đơn hàng realtime
- Báo cáo doanh thu

### 📋 Quản Lý Hợp Đồng

- Danh sách hợp đồng với filter
- Chi tiết hợp đồng đầy đủ
- Xử lý trả xe tự động

## 🚀 Deployment

### IIS Deployment

1. Build project: `dotnet publish -c Release`
2. Copy files đến IIS folder
3. Cấu hình Application Pool (.NET Core)
4. Thiết lập connection string production

### Docker (Optional)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY . .
EXPOSE 80
ENTRYPOINT ["dotnet", "bike.dll"]
```

## 🤝 Đóng Góp

1. Fork repository
2. Tạo feature branch: `git checkout -b feature/TenTinhNang`
3. Commit changes: `git commit -m 'Thêm tính năng XYZ'`
4. Push branch: `git push origin feature/TenTinhNang`
5. Tạo Pull Request

## 📞 Liên Hệ

- **Website**: 
- **Email**: zzcz4991@gmail.com
- **Hotline**: 0383407538
- **Địa chỉ**: TP.HCM

## 📄 License

Dự án này được phân phối dưới license MIT. Xem file `LICENSE` để biết thêm chi tiết.

---

**Được phát triển với ❤️ tại TP.HCM, Việt Nam**

> Hệ thống quản lý cho thuê xe máy chuyên nghiệp, giúp số hóa quy trình kinh doanh và nâng cao trải nghiệm khách hàng.
