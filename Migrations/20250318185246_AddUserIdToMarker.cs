using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpotMapApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToMarker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Markers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Markers");
        }
    }
}
