using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bike.Migrations
{
    /// <inheritdoc />
    public partial class RemoveQuanLyDonDatPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BaoCaoThietHai");

            migrationBuilder.DropColumn(
                name: "CanCancelDonDat",
                table: "UserPermissions");

            migrationBuilder.DropColumn(
                name: "CanConfirmDonDat",
                table: "UserPermissions");

            migrationBuilder.DropColumn(
                name: "CanCreateDonDat",
                table: "UserPermissions");

            migrationBuilder.DropColumn(
                name: "CanDeleteDonDat",
                table: "UserPermissions");

            migrationBuilder.DropColumn(
                name: "CanEditDonDat",
                table: "UserPermissions");

            migrationBuilder.DropColumn(
                name: "CanManageDonDat",
                table: "UserPermissions");

            migrationBuilder.DropColumn(
                name: "CanViewDonDat",
                table: "UserPermissions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CanCancelDonDat",
                table: "UserPermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanConfirmDonDat",
                table: "UserPermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanCreateDonDat",
                table: "UserPermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanDeleteDonDat",
                table: "UserPermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanEditDonDat",
                table: "UserPermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanManageDonDat",
                table: "UserPermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanViewDonDat",
                table: "UserPermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "BaoCaoThietHai",
                columns: table => new
                {
                    MaBaoCao = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaChiTiet = table.Column<int>(type: "int", nullable: false),
                    MaNguoiTao = table.Column<int>(type: "int", nullable: true),
                    ChiPhiSuaChuaThucTe = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ChiPhiSuaChuaUocTinh = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    GhiChuThanhToan = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    GiaTriXeSauKhiHong = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GiaTriXeTruocKhiHong = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LoaiThietHai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MoTaChiTiet = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayPhatHien = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayThanhToan = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PhiDenBuKhachHang = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SoTienDaThanhToan = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TrangThaiThanhToan = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TrangThaiXuLy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ViTriThietHai = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaoCaoThietHai", x => x.MaBaoCao);
                    table.ForeignKey(
                        name: "FK_BaoCaoThietHai_ChiTietHopDong_MaChiTiet",
                        column: x => x.MaChiTiet,
                        principalTable: "ChiTietHopDong",
                        principalColumn: "MaChiTiet",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaoCaoThietHai_Users_MaNguoiTao",
                        column: x => x.MaNguoiTao,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BaoCaoThietHai_MaChiTiet",
                table: "BaoCaoThietHai",
                column: "MaChiTiet");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCaoThietHai_MaNguoiTao",
                table: "BaoCaoThietHai",
                column: "MaNguoiTao");
        }
    }
}
