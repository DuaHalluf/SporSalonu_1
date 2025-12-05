using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SporSalonu_1.Migrations
{
    /// <inheritdoc />
    public partial class FixUyeIdType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Randevular_AspNetUsers_UyeId",
                table: "Randevular");

            migrationBuilder.AlterColumn<string>(
                name: "UyeId",
                table: "Randevular",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_Randevular_AspNetUsers_UyeId",
                table: "Randevular",
                column: "UyeId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Randevular_AspNetUsers_UyeId",
                table: "Randevular");

            migrationBuilder.AlterColumn<string>(
                name: "UyeId",
                table: "Randevular",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Randevular_AspNetUsers_UyeId",
                table: "Randevular",
                column: "UyeId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
