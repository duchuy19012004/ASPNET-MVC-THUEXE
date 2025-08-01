using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bike.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RoleMaVaiTro",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SoDienThoai",
                table: "HopDong",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

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

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleMaVaiTro",
                table: "Users",
                column: "RoleMaVaiTro");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_UserId",
                table: "UserPermissions",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_RoleMaVaiTro",
                table: "Users",
                column: "RoleMaVaiTro",
                principalTable: "Roles",
                principalColumn: "MaVaiTro");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_RoleMaVaiTro",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "UserPermissions");

            migrationBuilder.DropIndex(
                name: "IX_Users_RoleMaVaiTro",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RoleMaVaiTro",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "SoDienThoai",
                table: "HopDong",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(11)",
                oldMaxLength: 11);
        }
    }
}
