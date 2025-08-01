using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bike.Migrations
{
    /// <inheritdoc />
    public partial class addcanedithinhanhxe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CanEditHinhAnhXe",
                table: "UserPermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanEditHinhAnhXe",
                table: "UserPermissions");
        }
    }
}
