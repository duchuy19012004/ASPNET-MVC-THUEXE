using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bike.Migrations
{
    /// <inheritdoc />
    public partial class db : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Banner",
                columns: table => new
                {
                    MaBanner = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TieuDe = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HinhAnh = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    LinkLienKet = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ThuTu = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Banner", x => x.MaBanner);
                });

            migrationBuilder.CreateTable(
                name: "LoaiXe",
                columns: table => new
                {
                    MaLoaiXe = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenLoaiXe = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoaiXe", x => x.MaLoaiXe);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    MaVaiTro = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenVaiTro = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.MaVaiTro);
                });

            migrationBuilder.CreateTable(
                name: "Xe",
                columns: table => new
                {
                    MaXe = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenXe = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BienSoXe = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HangXe = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DongXe = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GiaThue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GiaTriXe = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NgayGapSuCo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MoTaThietHai = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ChiPhiSuaChua = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaLoaiXe = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Xe", x => x.MaXe);
                    table.ForeignKey(
                        name: "FK_Xe_LoaiXe_MaLoaiXe",
                        column: x => x.MaLoaiXe,
                        principalTable: "LoaiXe",
                        principalColumn: "MaLoaiXe",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ten = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MatKhau = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VaiTro = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Avatar = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayVaoLam = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NgayNghiViec = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MucLuong = table.Column<decimal>(type: "decimal(18,0)", nullable: true),
                    RoleMaVaiTro = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleMaVaiTro",
                        column: x => x.RoleMaVaiTro,
                        principalTable: "Roles",
                        principalColumn: "MaVaiTro");
                });

            migrationBuilder.CreateTable(
                name: "ChiTieu",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SoTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NgayChi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaXe = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTieu", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiTieu_Xe_MaXe",
                        column: x => x.MaXe,
                        principalTable: "Xe",
                        principalColumn: "MaXe");
                });

            migrationBuilder.CreateTable(
                name: "HinhAnhXe",
                columns: table => new
                {
                    MaHinhAnh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaXe = table.Column<int>(type: "int", nullable: false),
                    TenFile = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ThuTu = table.Column<int>(type: "int", nullable: false),
                    LaAnhChinh = table.Column<bool>(type: "bit", nullable: false),
                    NgayThem = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HinhAnhXe", x => x.MaHinhAnh);
                    table.ForeignKey(
                        name: "FK_HinhAnhXe_Xe_MaXe",
                        column: x => x.MaXe,
                        principalTable: "Xe",
                        principalColumn: "MaXe",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DatCho",
                columns: table => new
                {
                    MaDatCho = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaXe = table.Column<int>(type: "int", nullable: false),
                    MaUser = table.Column<int>(type: "int", nullable: true),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SoDienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NgayNhanXe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayTraXe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NgayDat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatCho", x => x.MaDatCho);
                    table.ForeignKey(
                        name: "FK_DatCho_Users_MaUser",
                        column: x => x.MaUser,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DatCho_Xe_MaXe",
                        column: x => x.MaXe,
                        principalTable: "Xe",
                        principalColumn: "MaXe",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CanManageXe = table.Column<bool>(type: "bit", nullable: false),
                    CanViewXe = table.Column<bool>(type: "bit", nullable: false),
                    CanCreateXe = table.Column<bool>(type: "bit", nullable: false),
                    CanEditXe = table.Column<bool>(type: "bit", nullable: false),
                    CanDeleteXe = table.Column<bool>(type: "bit", nullable: false),
                    CanManageLoaiXe = table.Column<bool>(type: "bit", nullable: false),
                    CanViewLoaiXe = table.Column<bool>(type: "bit", nullable: false),
                    CanCreateLoaiXe = table.Column<bool>(type: "bit", nullable: false),
                    CanEditLoaiXe = table.Column<bool>(type: "bit", nullable: false),
                    CanDeleteLoaiXe = table.Column<bool>(type: "bit", nullable: false),
                    CanManageHopDong = table.Column<bool>(type: "bit", nullable: false),
                    CanViewHopDong = table.Column<bool>(type: "bit", nullable: false),
                    CanCreateHopDong = table.Column<bool>(type: "bit", nullable: false),
                    CanEditHopDong = table.Column<bool>(type: "bit", nullable: false),
                    CanDeleteHopDong = table.Column<bool>(type: "bit", nullable: false),
                    CanPrintHopDong = table.Column<bool>(type: "bit", nullable: false),
                    CanManageHoaDon = table.Column<bool>(type: "bit", nullable: false),
                    CanViewHoaDon = table.Column<bool>(type: "bit", nullable: false),
                    CanCreateHoaDon = table.Column<bool>(type: "bit", nullable: false),
                    CanEditHoaDon = table.Column<bool>(type: "bit", nullable: false),
                    CanDeleteHoaDon = table.Column<bool>(type: "bit", nullable: false),
                    CanPrintHoaDon = table.Column<bool>(type: "bit", nullable: false),
                    CanManageNhanVien = table.Column<bool>(type: "bit", nullable: false),
                    CanViewNhanVien = table.Column<bool>(type: "bit", nullable: false),
                    CanCreateNhanVien = table.Column<bool>(type: "bit", nullable: false),
                    CanEditNhanVien = table.Column<bool>(type: "bit", nullable: false),
                    CanDeleteNhanVien = table.Column<bool>(type: "bit", nullable: false),
                    CanManageUser = table.Column<bool>(type: "bit", nullable: false),
                    CanViewUser = table.Column<bool>(type: "bit", nullable: false),
                    CanCreateUser = table.Column<bool>(type: "bit", nullable: false),
                    CanEditUser = table.Column<bool>(type: "bit", nullable: false),
                    CanDeleteUser = table.Column<bool>(type: "bit", nullable: false),
                    CanManageBanner = table.Column<bool>(type: "bit", nullable: false),
                    CanViewBanner = table.Column<bool>(type: "bit", nullable: false),
                    CanCreateBanner = table.Column<bool>(type: "bit", nullable: false),
                    CanEditBanner = table.Column<bool>(type: "bit", nullable: false),
                    CanDeleteBanner = table.Column<bool>(type: "bit", nullable: false),
                    CanManageChiTieu = table.Column<bool>(type: "bit", nullable: false),
                    CanViewChiTieu = table.Column<bool>(type: "bit", nullable: false),
                    CanCreateChiTieu = table.Column<bool>(type: "bit", nullable: false),
                    CanEditChiTieu = table.Column<bool>(type: "bit", nullable: false),
                    CanDeleteChiTieu = table.Column<bool>(type: "bit", nullable: false),
                    CanManageThietHai = table.Column<bool>(type: "bit", nullable: false),
                    CanViewThietHai = table.Column<bool>(type: "bit", nullable: false),
                    CanCreateThietHai = table.Column<bool>(type: "bit", nullable: false),
                    CanEditThietHai = table.Column<bool>(type: "bit", nullable: false),
                    CanDeleteThietHai = table.Column<bool>(type: "bit", nullable: false),
                    CanThanhToanThietHai = table.Column<bool>(type: "bit", nullable: false),
                    CanViewBaoCao = table.Column<bool>(type: "bit", nullable: false),
                    CanViewThongKe = table.Column<bool>(type: "bit", nullable: false),
                    CanExportBaoCao = table.Column<bool>(type: "bit", nullable: false),
                    CanManageCart = table.Column<bool>(type: "bit", nullable: false),
                    CanViewCart = table.Column<bool>(type: "bit", nullable: false),
                    CanCheckout = table.Column<bool>(type: "bit", nullable: false),
                    CanDatCho = table.Column<bool>(type: "bit", nullable: false),
                    CanViewDatCho = table.Column<bool>(type: "bit", nullable: false),
                    CanManageHinhAnhXe = table.Column<bool>(type: "bit", nullable: false),
                    CanViewHinhAnhXe = table.Column<bool>(type: "bit", nullable: false),
                    CanUploadHinhAnhXe = table.Column<bool>(type: "bit", nullable: false),
                    CanEditHinhAnhXe = table.Column<bool>(type: "bit", nullable: false),
                    CanDeleteHinhAnhXe = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPermissions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HopDong",
                columns: table => new
                {
                    MaHopDong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDatCho = table.Column<int>(type: "int", nullable: true),
                    MaKhachHang = table.Column<int>(type: "int", nullable: true),
                    HoTenKhach = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    SoCCCD = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    NgayNhanXe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayTraXeDuKien = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayTraXeThucTe = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TienCoc = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PhuPhi = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TongTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaNguoiTao = table.Column<int>(type: "int", nullable: true),
                    CccdMatTruoc = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CccdMatSau = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    BangLaiXe = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    GiayToKhac = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HopDong", x => x.MaHopDong);
                    table.ForeignKey(
                        name: "FK_HopDong_DatCho_MaDatCho",
                        column: x => x.MaDatCho,
                        principalTable: "DatCho",
                        principalColumn: "MaDatCho");
                    table.ForeignKey(
                        name: "FK_HopDong_Users_MaKhachHang",
                        column: x => x.MaKhachHang,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HopDong_Users_MaNguoiTao",
                        column: x => x.MaNguoiTao,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ChiTietHopDong",
                columns: table => new
                {
                    MaChiTiet = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHopDong = table.Column<int>(type: "int", nullable: false),
                    MaXe = table.Column<int>(type: "int", nullable: false),
                    GiaThueNgay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NgayNhanXe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayTraXeDuKien = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayTraXeThucTe = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SoNgayThue = table.Column<int>(type: "int", nullable: false),
                    ThanhTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TrangThaiXe = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TinhTrangTraXe = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MoTaThietHai = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PhiDenBu = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietHopDong", x => x.MaChiTiet);
                    table.ForeignKey(
                        name: "FK_ChiTietHopDong_HopDong_MaHopDong",
                        column: x => x.MaHopDong,
                        principalTable: "HopDong",
                        principalColumn: "MaHopDong",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietHopDong_Xe_MaXe",
                        column: x => x.MaXe,
                        principalTable: "Xe",
                        principalColumn: "MaXe");
                });

            migrationBuilder.CreateTable(
                name: "HoaDon",
                columns: table => new
                {
                    MaHoaDon = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHopDong = table.Column<int>(type: "int", nullable: false),
                    NgayThanhToan = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SoTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaNguoiTao = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoaDon", x => x.MaHoaDon);
                    table.ForeignKey(
                        name: "FK_HoaDon_HopDong_MaHopDong",
                        column: x => x.MaHopDong,
                        principalTable: "HopDong",
                        principalColumn: "MaHopDong",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HoaDon_Users_MaNguoiTao",
                        column: x => x.MaNguoiTao,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ThietHai",
                columns: table => new
                {
                    MaThietHai = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaXe = table.Column<int>(type: "int", nullable: false),
                    MaHopDong = table.Column<int>(type: "int", nullable: true),
                    LoaiThietHai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MoTaThietHai = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    NgayXayRa = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaKhachHang = table.Column<int>(type: "int", nullable: true),
                    TrangThaiXuLy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PhuongAnXuLy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SoTienDenBu = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NgayHoanThanh = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MaNguoiBaoCao = table.Column<int>(type: "int", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThietHai", x => x.MaThietHai);
                    table.ForeignKey(
                        name: "FK_ThietHai_HopDong_MaHopDong",
                        column: x => x.MaHopDong,
                        principalTable: "HopDong",
                        principalColumn: "MaHopDong");
                    table.ForeignKey(
                        name: "FK_ThietHai_Users_MaKhachHang",
                        column: x => x.MaKhachHang,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ThietHai_Users_MaNguoiBaoCao",
                        column: x => x.MaNguoiBaoCao,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ThietHai_Xe_MaXe",
                        column: x => x.MaXe,
                        principalTable: "Xe",
                        principalColumn: "MaXe",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietHopDong_MaHopDong",
                table: "ChiTietHopDong",
                column: "MaHopDong");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietHopDong_MaXe",
                table: "ChiTietHopDong",
                column: "MaXe");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTieu_MaXe",
                table: "ChiTieu",
                column: "MaXe");

            migrationBuilder.CreateIndex(
                name: "IX_DatCho_MaUser",
                table: "DatCho",
                column: "MaUser");

            migrationBuilder.CreateIndex(
                name: "IX_DatCho_MaXe",
                table: "DatCho",
                column: "MaXe");

            migrationBuilder.CreateIndex(
                name: "IX_HinhAnhXe_MaXe_LaAnhChinh",
                table: "HinhAnhXe",
                columns: new[] { "MaXe", "LaAnhChinh" });

            migrationBuilder.CreateIndex(
                name: "IX_HinhAnhXe_MaXe_ThuTu",
                table: "HinhAnhXe",
                columns: new[] { "MaXe", "ThuTu" });

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_MaHopDong",
                table: "HoaDon",
                column: "MaHopDong",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_MaNguoiTao",
                table: "HoaDon",
                column: "MaNguoiTao");

            migrationBuilder.CreateIndex(
                name: "IX_HopDong_MaDatCho",
                table: "HopDong",
                column: "MaDatCho");

            migrationBuilder.CreateIndex(
                name: "IX_HopDong_MaKhachHang",
                table: "HopDong",
                column: "MaKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_HopDong_MaNguoiTao",
                table: "HopDong",
                column: "MaNguoiTao");

            migrationBuilder.CreateIndex(
                name: "IX_ThietHai_MaHopDong",
                table: "ThietHai",
                column: "MaHopDong");

            migrationBuilder.CreateIndex(
                name: "IX_ThietHai_MaKhachHang",
                table: "ThietHai",
                column: "MaKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_ThietHai_MaNguoiBaoCao",
                table: "ThietHai",
                column: "MaNguoiBaoCao");

            migrationBuilder.CreateIndex(
                name: "IX_ThietHai_MaXe",
                table: "ThietHai",
                column: "MaXe");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_UserId",
                table: "UserPermissions",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleMaVaiTro",
                table: "Users",
                column: "RoleMaVaiTro");

            migrationBuilder.CreateIndex(
                name: "IX_Xe_MaLoaiXe",
                table: "Xe",
                column: "MaLoaiXe");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Banner");

            migrationBuilder.DropTable(
                name: "ChiTietHopDong");

            migrationBuilder.DropTable(
                name: "ChiTieu");

            migrationBuilder.DropTable(
                name: "HinhAnhXe");

            migrationBuilder.DropTable(
                name: "HoaDon");

            migrationBuilder.DropTable(
                name: "ThietHai");

            migrationBuilder.DropTable(
                name: "UserPermissions");

            migrationBuilder.DropTable(
                name: "HopDong");

            migrationBuilder.DropTable(
                name: "DatCho");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Xe");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "LoaiXe");
        }
    }
}
