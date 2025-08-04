using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bike.Migrations
{
    /// <inheritdoc />
    public partial class AddDonDatPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
    }
}
