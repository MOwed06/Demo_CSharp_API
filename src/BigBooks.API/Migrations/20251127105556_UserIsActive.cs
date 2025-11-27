using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BigBooks.API.Migrations
{
    /// <inheritdoc />
    public partial class UserIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AppUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Key",
                keyValue: 1,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Key",
                keyValue: 2,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Key",
                keyValue: 3,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Key",
                keyValue: 4,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Key",
                keyValue: 5,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Key",
                keyValue: 6,
                column: "IsActive",
                value: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AppUsers");
        }
    }
}
