using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bike.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentUploadFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BangLaiXe",
                table: "HopDong",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CccdMatSau",
                table: "HopDong",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CccdMatTruoc",
                table: "HopDong",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GiayToKhac",
                table: "HopDong",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BangLaiXe",
                table: "HopDong");

            migrationBuilder.DropColumn(
                name: "CccdMatSau",
                table: "HopDong");

            migrationBuilder.DropColumn(
                name: "CccdMatTruoc",
                table: "HopDong");

            migrationBuilder.DropColumn(
                name: "GiayToKhac",
                table: "HopDong");
        }
    }
}
