using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace socmed_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddQuoteRantId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuoteRantId",
                table: "Rants",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rants_QuoteRantId",
                table: "Rants",
                column: "QuoteRantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rants_Rants_QuoteRantId",
                table: "Rants",
                column: "QuoteRantId",
                principalTable: "Rants",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rants_Rants_QuoteRantId",
                table: "Rants");

            migrationBuilder.DropIndex(
                name: "IX_Rants_QuoteRantId",
                table: "Rants");

            migrationBuilder.DropColumn(
                name: "QuoteRantId",
                table: "Rants");
        }
    }
}
