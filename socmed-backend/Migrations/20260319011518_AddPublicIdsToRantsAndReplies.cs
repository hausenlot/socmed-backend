using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace socmed_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicIdsToRantsAndReplies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PublicId",
                table: "Rants",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PublicId",
                table: "RantReplies",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            // Retroactive: Fill existing records with their integer ID as string
            migrationBuilder.Sql("UPDATE Rants SET PublicId = CAST(Id AS TEXT)");
            migrationBuilder.Sql("UPDATE RantReplies SET PublicId = CAST(Id AS TEXT)");

            migrationBuilder.CreateIndex(
                name: "IX_Rants_PublicId",
                table: "Rants",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RantReplies_PublicId",
                table: "RantReplies",
                column: "PublicId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rants_PublicId",
                table: "Rants");

            migrationBuilder.DropIndex(
                name: "IX_RantReplies_PublicId",
                table: "RantReplies");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Rants");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "RantReplies");
        }
    }
}
