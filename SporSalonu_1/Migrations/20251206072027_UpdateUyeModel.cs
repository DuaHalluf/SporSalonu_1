using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SporSalonu_1.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUyeModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResimUrl",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResimUrl",
                table: "AspNetUsers");
        }
    }
}
