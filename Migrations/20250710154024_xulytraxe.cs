using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bike.Migrations
{
    /// <inheritdoc />
    public partial class xulytraxe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ChiPhiSuaChua",
                table: "Xe",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "GiaTriXe",
                table: "Xe",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "MoTaThietHai",
                table: "Xe",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayGapSuCo",
                table: "Xe",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MoTaThietHai",
                table: "ChiTietHopDong",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PhiDenBu",
                table: "ChiTietHopDong",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TinhTrangTraXe",
                table: "ChiTietHopDong",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BaoCaoThietHai",
                columns: table => new
                {
                    MaBaoCao = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaChiTiet = table.Column<int>(type: "int", nullable: false),
                    LoaiThietHai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MoTaChiTiet = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    NgayPhatHien = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ViTriThietHai = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ChiPhiSuaChuaUocTinh = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ChiPhiSuaChuaThucTe = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PhiDenBuKhachHang = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GiaTriXeTruocKhiHong = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GiaTriXeSauKhiHong = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TrangThaiXuLy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MaNguoiTao = table.Column<int>(type: "int", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: false)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BaoCaoThietHai");

            migrationBuilder.DropColumn(
                name: "ChiPhiSuaChua",
                table: "Xe");

            migrationBuilder.DropColumn(
                name: "GiaTriXe",
                table: "Xe");

            migrationBuilder.DropColumn(
                name: "MoTaThietHai",
                table: "Xe");

            migrationBuilder.DropColumn(
                name: "NgayGapSuCo",
                table: "Xe");

            migrationBuilder.DropColumn(
                name: "MoTaThietHai",
                table: "ChiTietHopDong");

            migrationBuilder.DropColumn(
                name: "PhiDenBu",
                table: "ChiTietHopDong");

            migrationBuilder.DropColumn(
                name: "TinhTrangTraXe",
                table: "ChiTietHopDong");
        }
    }
}
