using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpotMapApi.Migrations
{
    /// <inheritdoc />
    public partial class AddMarkersRatingsFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "MarkerImages",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MarkerRatings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MarkerId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarkerRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarkerRatings_Markers_MarkerId",
                        column: x => x.MarkerId,
                        principalTable: "Markers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MarkerRatings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MarkerImages_UserId",
                table: "MarkerImages",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MarkerRatings_MarkerId",
                table: "MarkerRatings",
                column: "MarkerId");

            migrationBuilder.CreateIndex(
                name: "IX_MarkerRatings_UserId_MarkerId",
                table: "MarkerRatings",
                columns: new[] { "UserId", "MarkerId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MarkerImages_Users_UserId",
                table: "MarkerImages",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MarkerImages_Users_UserId",
                table: "MarkerImages");

            migrationBuilder.DropTable(
                name: "MarkerRatings");

            migrationBuilder.DropIndex(
                name: "IX_MarkerImages_UserId",
                table: "MarkerImages");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "MarkerImages");
        }
    }
}
