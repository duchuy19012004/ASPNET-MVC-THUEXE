using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bike.Migrations
{
    /// <inheritdoc />
    public partial class quanlyth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ThietHai",
                columns: table => new
                {
                    MaThietHai = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaXe = table.Column<int>(type: "int", nullable: false),
                    MaHopDong = table.Column<int>(type: "int", nullable: true),
                    LoaiThietHai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MoTaThietHai = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    NgayXayRa = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayPhatHien = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MaKhachHang = table.Column<int>(type: "int", nullable: true),
                    TenKhachHang = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SoDienThoaiKhach = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    TrangThaiXuLy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PhuongAnXuLy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ChiPhiXuLy = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ThietHai");
        }
    }
}
